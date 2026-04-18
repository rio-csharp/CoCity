using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultMinistryFrameworkService : IMinistryFrameworkService
    {
        private const int LoyaltyWatchThreshold = 85;
        private const int LoyaltyEscalationThreshold = 78;

        public RealmMinistryState Initialize(
            RealmState foundation,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState)
        {
            var seededState = new RealmMinistryState(
                foundation.Ministries
                    .Select(ministry => new MinistrySimulationState(
                        MinistryId: ministry.Id,
                        MinistryName: ministry.Name,
                        Authority: ministry.Authority,
                        Standard: ministry.Standard,
                        Minister: ministry.Minister,
                        SupportingOfficials: ministry.SupportingOfficials,
                        HandlingCapacity: 0m,
                        AutomationSuccessRate: 0m,
                        ActiveCaseCount: 0,
                        ProcessedCaseCount: 0,
                        EscalatedCaseCount: 0,
                        ActiveCases: [],
                        PendingEscalations: [],
                        LastSummary: "No ministry activity recorded yet."))
                    .ToImmutableArray());

            return BuildState(seededState, mortalRealmState, buildingState, taxationState, processAutomation: false);
        }

        public MinistryFrameworkTurnResult Step(
            RealmState foundation,
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState)
        {
            var nextState = BuildState(currentState, mortalRealmState, buildingState, taxationState, processAutomation: true);
            var report = new MinistryTurnReport(
                TurnNumber: mortalRealmState.TurnNumber,
                MinistryEvents: nextState.Ministries
                    .Select(ministry => new MinistryTurnEvent(
                        MinistryId: ministry.MinistryId,
                        MinistryName: ministry.MinistryName,
                        HandlingCapacity: ministry.HandlingCapacity,
                        AutomationSuccessRate: ministry.AutomationSuccessRate,
                        ActiveCases: ministry.ActiveCaseCount,
                        ProcessedCases: ministry.ProcessedCaseCount,
                        EscalatedCases: ministry.EscalatedCaseCount,
                        Summary: ministry.LastSummary))
                    .ToImmutableArray());

            return new MinistryFrameworkTurnResult(
                NextState: nextState,
                Report: report);
        }

        public RealmMinistryState Recalculate(
            RealmState foundation,
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState)
            => BuildState(currentState, mortalRealmState, buildingState, taxationState, processAutomation: false);

        private static RealmMinistryState BuildState(
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            bool processAutomation)
        {
            var buildingInventoriesBySectId = buildingState.Sects.ToDictionary(item => item.SectId);

            return new RealmMinistryState(
                currentState.Ministries
                    .OrderBy(ministry => ministry.MinistryId)
                    .Select(ministry => BuildMinistryState(
                        ministry,
                        mortalRealmState,
                        buildingInventoriesBySectId,
                        taxationState,
                        processAutomation))
                    .ToImmutableArray());
        }

        private static MinistrySimulationState BuildMinistryState(
            MinistrySimulationState ministry,
            MortalRealmState mortalRealmState,
            IReadOnlyDictionary<string, SectBuildingInventoryState> buildingInventoriesBySectId,
            RealmTaxationState taxationState,
            bool processAutomation)
        {
            var activeCases = ministry.MinistryId switch
            {
                "ministry.personnel" => BuildPersonnelCases(mortalRealmState.Sects, buildingInventoriesBySectId),
                "ministry.revenue" => BuildRevenueCases(taxationState),
                "ministry.rites" => BuildRitesCases(mortalRealmState.Sects, buildingInventoriesBySectId),
                _ => []
            };

            var handlingCapacity = CalculateHandlingCapacity(ministry.Minister, ministry.SupportingOfficials);
            var automationSuccessRate = CalculateAutomationSuccessRate(
                ministry,
                activeCases.Length,
                handlingCapacity);

            if (!processAutomation)
            {
                var previewEscalations = activeCases
                    .Where(item => ShouldEscalate(ministry, item, automationSuccessRate))
                    .Select(item => BuildEscalation(ministry, item, automationSuccessRate))
                    .ToImmutableArray();

                return ministry with
                {
                    HandlingCapacity = handlingCapacity,
                    AutomationSuccessRate = automationSuccessRate,
                    ActiveCaseCount = activeCases.Length,
                    ProcessedCaseCount = 0,
                    EscalatedCaseCount = previewEscalations.Length,
                    ActiveCases = activeCases,
                    PendingEscalations = previewEscalations,
                    LastSummary = BuildPreviewSummary(ministry.MinistryName, activeCases.Length, previewEscalations.Length, automationSuccessRate)
                };
            }

            var escalations = ImmutableArray.CreateBuilder<MinistryEscalationState>();
            var processedCount = 0;

            foreach (var activeCase in activeCases)
            {
                if (ShouldEscalate(ministry, activeCase, automationSuccessRate))
                {
                    escalations.Add(BuildEscalation(ministry, activeCase, automationSuccessRate));
                    continue;
                }

                processedCount++;
            }

            return ministry with
            {
                HandlingCapacity = handlingCapacity,
                AutomationSuccessRate = automationSuccessRate,
                ActiveCaseCount = activeCases.Length,
                ProcessedCaseCount = processedCount,
                EscalatedCaseCount = escalations.Count,
                ActiveCases = activeCases,
                PendingEscalations = escalations.ToImmutable(),
                LastSummary = BuildAutomationSummary(
                    ministry.MinistryName,
                    activeCases.Length,
                    processedCount,
                    escalations.Count,
                    automationSuccessRate,
                    escalations.Count == 0 ? null : escalations[0].Reason)
            };
        }

        private static ImmutableArray<MinistryCaseState> BuildPersonnelCases(
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyDictionary<string, SectBuildingInventoryState> buildingInventoriesBySectId)
        {
            var cases = ImmutableArray.CreateBuilder<MinistryCaseState>();

            foreach (var sect in sects.OrderBy(item => item.SectId))
            {
                if (!buildingInventoriesBySectId.TryGetValue(sect.SectId, out var inventory) || inventory.ActiveProject is null)
                {
                    continue;
                }

                var buildingDefinition = BuildingCatalog.Get(inventory.ActiveProject.Building);
                var completedBuildings = inventory.Buildings.Sum(item => item.Quantity);
                var requiresEscalation = inventory.ActiveProject.Building is SectBuildingType.SpiritGatheringArray or SectBuildingType.AlchemyRoom
                    || sect.CurrentPopulation >= 200;

                cases.Add(new MinistryCaseState(
                    CaseId: $"{sect.SectId}:expansion",
                    CaseType: MinistryCaseType.SectApplication,
                    SubjectId: sect.SectId,
                    SubjectName: sect.SectName,
                    RequiresEscalation: requiresEscalation,
                    Summary: requiresEscalation
                        ? $"{sect.SectName} requested sensitive {buildingDefinition.DisplayName} expansion; {completedBuildings} buildings are already on record."
                        : $"{sect.SectName} filed a routine {buildingDefinition.DisplayName} expansion petition with {inventory.ActiveProject.TurnsRemaining} turn(s) remaining."));
            }

            return cases.ToImmutable();
        }

        private static ImmutableArray<MinistryCaseState> BuildRevenueCases(RealmTaxationState taxationState)
        {
            var policy = TaxationPolicyCatalog.Get(taxationState.SelectedTaxRate);

            return taxationState.Towns
                .OrderBy(town => town.TownId)
                .Select(town => new MinistryCaseState(
                    CaseId: $"{town.TownId}:revenue",
                    CaseType: MinistryCaseType.TaxCollection,
                    SubjectId: town.TownId,
                    SubjectName: town.TownName,
                    RequiresEscalation: taxationState.SelectedTaxRate == TaxRateLevel.Heavy || town.StabilityDelta <= -3,
                    Summary: $"{town.TownName} remittance is scheduled at {town.CollectedRevenue} taels under {policy.DisplayName}."))
                .ToImmutableArray();
        }

        private static ImmutableArray<MinistryCaseState> BuildRitesCases(
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyDictionary<string, SectBuildingInventoryState> buildingInventoriesBySectId)
        {
            var cases = ImmutableArray.CreateBuilder<MinistryCaseState>();

            foreach (var sect in sects.OrderBy(item => item.SectId))
            {
                var inventory = buildingInventoriesBySectId.GetValueOrDefault(sect.SectId);
                if (sect.Loyalty > LoyaltyWatchThreshold && inventory?.ActiveProject is null)
                {
                    continue;
                }

                var projectSummary = inventory?.ActiveProject is null
                    ? "no active expansion petition"
                    : $"an active {BuildingCatalog.Get(inventory.ActiveProject.Building).DisplayName} project";

                cases.Add(new MinistryCaseState(
                    CaseId: $"{sect.SectId}:rites",
                    CaseType: MinistryCaseType.SectDiplomacy,
                    SubjectId: sect.SectId,
                    SubjectName: sect.SectName,
                    RequiresEscalation: sect.Loyalty <= LoyaltyEscalationThreshold,
                    Summary: sect.Loyalty <= LoyaltyEscalationThreshold
                        ? $"{sect.SectName} requires diplomatic attention; loyalty is {sect.Loyalty} with {projectSummary}."
                        : $"{sect.SectName} remains on protocol watch with loyalty {sect.Loyalty} and {projectSummary}."));
            }

            return cases.ToImmutable();
        }

        private static decimal CalculateHandlingCapacity(
            OfficialState minister,
            IReadOnlyList<OfficialState> supportingOfficials)
        {
            var ministerScore = CalculateOfficialScore(minister);
            var supportScore = supportingOfficials.Count == 0
                ? ministerScore
                : supportingOfficials.Average(CalculateOfficialScore);

            return Math.Round(((ministerScore * 0.65m) + (supportScore * 0.35m)) / 12m, 1, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateAutomationSuccessRate(
            MinistrySimulationState ministry,
            int activeCaseCount,
            decimal handlingCapacity)
        {
            var baseRate = CalculateOfficialScore(ministry.Minister) / 100m;
            var workloadPenalty = handlingCapacity <= 0m
                ? 0.35m
                : Math.Max(0m, (activeCaseCount - handlingCapacity) / (handlingCapacity * 3m));

            var delegationModifier = ministry.Authority.DelegationLevel switch
            {
                "Broad delegation" => 0.05m,
                "Narrow delegation" => -0.05m,
                _ => 0m
            };

            return Math.Clamp(
                Math.Round(baseRate + delegationModifier - workloadPenalty, 2, MidpointRounding.AwayFromZero),
                0.45m,
                0.97m);
        }

        private static decimal CalculateOfficialScore(OfficialState official)
            => (official.Ratings.Administration * 0.5m) +
               (official.Ratings.Integrity * 0.25m) +
               (official.Ratings.Loyalty * 0.25m);

        private static bool ShouldEscalate(
            MinistrySimulationState ministry,
            MinistryCaseState activeCase,
            decimal automationSuccessRate)
        {
            if (activeCase.RequiresEscalation)
            {
                return true;
            }

            return automationSuccessRate < GetAutomationThreshold(ministry, activeCase.CaseType);
        }

        private static decimal GetAutomationThreshold(
            MinistrySimulationState ministry,
            MinistryCaseType caseType)
        {
            var baseThreshold = caseType switch
            {
                MinistryCaseType.SectApplication => 0.70m,
                MinistryCaseType.TaxCollection => 0.62m,
                MinistryCaseType.SectDiplomacy => 0.72m,
                _ => 0.70m
            };

            var standardModifier = ministry.Standard.Name switch
            {
                "Conservative review" => 0.05m,
                "Precautionary mediation" => 0.08m,
                _ => 0m
            };

            return baseThreshold + standardModifier;
        }

        private static MinistryEscalationState BuildEscalation(
            MinistrySimulationState ministry,
            MinistryCaseState activeCase,
            decimal automationSuccessRate)
            => new(
                CaseId: activeCase.CaseId,
                CaseType: activeCase.CaseType,
                SubjectId: activeCase.SubjectId,
                SubjectName: activeCase.SubjectName,
                Reason: activeCase.RequiresEscalation
                    ? $"{activeCase.SubjectName} exceeded {ministry.MinistryName}'s delegated authority."
                    : $"{activeCase.SubjectName} failed {ministry.MinistryName}'s automation threshold at {FormatPercent(automationSuccessRate)} success.");

        private static string BuildPreviewSummary(
            string ministryName,
            int activeCases,
            int escalatedCases,
            decimal automationSuccessRate)
        {
            if (activeCases == 0)
            {
                return $"{ministryName} is on routine watch with no active cases.";
            }

            return $"Ready to automate {activeCases} case(s) at {FormatPercent(automationSuccessRate)} success; {escalatedCases} case(s) would escalate.";
        }

        private static string BuildAutomationSummary(
            string ministryName,
            int activeCases,
            int processedCases,
            int escalatedCases,
            decimal automationSuccessRate,
            string? firstEscalationReason)
        {
            if (activeCases == 0)
            {
                return $"{ministryName} had no ministry cases to process this turn.";
            }

            return escalatedCases == 0
                ? $"Automated {processedCases}/{activeCases} case(s) at {FormatPercent(automationSuccessRate)} success with no escalations."
                : $"Automated {processedCases}/{activeCases} case(s) at {FormatPercent(automationSuccessRate)} success; escalated {escalatedCases}. {firstEscalationReason}";
        }

        private static string FormatPercent(decimal value)
            => $"{Math.Round(value * 100m, 0, MidpointRounding.AwayFromZero)}%";
    }
}
