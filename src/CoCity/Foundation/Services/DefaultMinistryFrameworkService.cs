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
                        ActiveCaseCount: 0,
                        EscalatedCaseCount: 0,
                        ActiveCases: [],
                        LastSummary: "No ministry activity recorded yet."))
                    .ToImmutableArray());

            return BuildState(seededState, mortalRealmState, buildingState, taxationState);
        }

        public MinistryFrameworkTurnResult Step(
            RealmState foundation,
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState)
        {
            var nextState = Recalculate(foundation, currentState, mortalRealmState, buildingState, taxationState);
            var report = new MinistryTurnReport(
                TurnNumber: mortalRealmState.TurnNumber,
                MinistryEvents: nextState.Ministries
                    .Select(ministry => new MinistryTurnEvent(
                        MinistryId: ministry.MinistryId,
                        MinistryName: ministry.MinistryName,
                        HandlingCapacity: ministry.HandlingCapacity,
                        ActiveCases: ministry.ActiveCaseCount,
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
            => BuildState(currentState, mortalRealmState, buildingState, taxationState);

        private static RealmMinistryState BuildState(
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState)
        {
            var buildingInventoriesBySectId = buildingState.Sects.ToDictionary(item => item.SectId);

            return new RealmMinistryState(
                currentState.Ministries
                    .OrderBy(ministry => ministry.MinistryId)
                    .Select(ministry => BuildMinistryState(
                        ministry,
                        mortalRealmState,
                        buildingInventoriesBySectId,
                        taxationState))
                    .ToImmutableArray());
        }

        private static MinistrySimulationState BuildMinistryState(
            MinistrySimulationState ministry,
            MortalRealmState mortalRealmState,
            IReadOnlyDictionary<string, SectBuildingInventoryState> buildingInventoriesBySectId,
            RealmTaxationState taxationState)
        {
            var activeCases = ministry.MinistryId switch
            {
                "ministry.personnel" => BuildPersonnelCases(mortalRealmState.Sects, buildingInventoriesBySectId),
                "ministry.revenue" => BuildRevenueCases(taxationState),
                "ministry.rites" => BuildRitesCases(mortalRealmState.Sects, buildingInventoriesBySectId),
                _ => []
            };

            var escalatedCaseCount = activeCases.Count(item => item.RequiresEscalation);
            var handlingCapacity = CalculateHandlingCapacity(ministry.Minister, ministry.SupportingOfficials);

            return ministry with
            {
                HandlingCapacity = handlingCapacity,
                ActiveCaseCount = activeCases.Length,
                EscalatedCaseCount = escalatedCaseCount,
                ActiveCases = activeCases,
                LastSummary = BuildSummary(ministry.MinistryName, activeCases.Length, escalatedCaseCount, handlingCapacity)
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

        private static decimal CalculateOfficialScore(OfficialState official)
            => (official.Ratings.Administration * 0.5m) +
               (official.Ratings.Integrity * 0.25m) +
               (official.Ratings.Loyalty * 0.25m);

        private static string BuildSummary(
            string ministryName,
            int activeCases,
            int escalatedCases,
            decimal handlingCapacity)
        {
            if (activeCases == 0)
            {
                return $"{ministryName} is on routine watch with no active cases.";
            }

            var loadRatio = handlingCapacity <= 0m ? decimal.MaxValue : activeCases / handlingCapacity;
            var workloadSummary = loadRatio switch
            {
                <= 0.75m => "Holding pace",
                <= 1.15m => "Working at full pace",
                _ => "Under visible strain"
            };

            return $"{workloadSummary}: {activeCases} active case(s), {escalatedCases} escalated, handling capacity {handlingCapacity}.";
        }
    }
}
