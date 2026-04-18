using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultMortalRealmSimulationService : IMortalRealmSimulationService
    {
        private const decimal GrowthRateWhenSatisfied = 0.01m;
        private const decimal DeclineRateWhenUnsatisfied = 0.02m;
        private const decimal RecruitablesRate = 0.05m;
        private const int MinimumPopulation = 100;

        public MortalRealmState Initialize(RealmState foundation)
        {
            var regionTownIdsByRegion = foundation.Regions.ToDictionary(region => region.Id, region => region.TownIds);
            var townStates = foundation.Towns
                .Select(CreateInitialTownState)
                .ToImmutableArray();

            return new MortalRealmState(
                Towns: townStates,
                Sects: CreateInitialSectStates(foundation.Sects, regionTownIdsByRegion, townStates),
                TurnNumber: 0);
        }

        public TurnResult Step(RealmState foundation, MortalRealmState currentState, TaxRateLevel taxRate = TaxRateLevel.Standard)
        {
            var nextTurn = currentState.TurnNumber + 1;
            var taxPolicy = TaxationPolicyCatalog.Get(taxRate);
            var townDefinitions = foundation.Towns.ToDictionary(town => town.Id);
            var regionTownIdsByRegion = foundation.Regions.ToDictionary(region => region.Id, region => region.TownIds);
            var currentTowns = currentState.Towns.ToDictionary(town => town.TownId);
            var currentSects = currentState.Sects
                .OrderBy(sect => sect.RegionId)
                .ThenBy(sect => sect.SectId)
                .ToImmutableArray();

            var naturalResults = foundation.Towns
                .Select(town => ResolveNaturalChange(town, currentTowns[town.Id], taxPolicy))
                .ToImmutableArray();
            var naturalResultsByTownId = naturalResults.ToDictionary(result => result.TownId);

            var remainingRecruitmentPools = naturalResults.ToDictionary(result => result.TownId, result => result.RecruitmentPoolBeforeRecruitment);
            var recruitsLostByTown = naturalResults.ToDictionary(result => result.TownId, _ => 0);
            var recruitmentEvents = ResolveSectRecruitment(currentSects, regionTownIdsByRegion, remainingRecruitmentPools, recruitsLostByTown);
            var recruitsBySectId = recruitmentEvents.ToDictionary(item => item.SectId, item => item.RecruitsGathered);
            var wagesPaidBySectId = recruitmentEvents.ToDictionary(item => item.SectId, item => item.WagesPaid);

            var nextTownStates = naturalResults
                .Select(result =>
                {
                    var town = townDefinitions[result.TownId];
                    var recruitsLost = recruitsLostByTown[result.TownId];
                    var finalPopulation = Math.Max(MinimumPopulation, result.PopulationAfterNaturalChange - recruitsLost);
                    var finalFoodBalance = CalculateFoodBalance(town, finalPopulation);

                    return new MortalTownSimulationState(
                        TownId: result.TownId,
                        TownName: result.TownName,
                        CurrentPopulation: finalPopulation,
                        FoodBalance: finalFoodBalance,
                        RecruitmentPool: remainingRecruitmentPools[result.TownId],
                        PopulationChange: finalPopulation - result.PreviousPopulation,
                        RecruitsLostLastTurn: recruitsLost,
                        ChangeReason: BuildChangeReason(result.NaturalReason, recruitsLost));
                })
                .ToImmutableArray();

            var townEvents = nextTownStates
                .Select(state =>
                {
                    var result = naturalResultsByTownId[state.TownId];

                    return new TownTurnEvent(
                        TownId: state.TownId,
                        TownName: state.TownName,
                        PreviousPopulation: result.PreviousPopulation,
                        CurrentPopulation: state.CurrentPopulation,
                        FoodBalance: state.FoodBalance,
                        RecruitsLost: state.RecruitsLostLastTurn,
                        ChangeReason: state.ChangeReason);
                })
                .ToImmutableArray();

            var report = new TurnReport(
                TurnNumber: nextTurn,
                TownEvents: townEvents,
                RecruitmentEvents: recruitmentEvents);

            var nextState = new MortalRealmState(
                Towns: nextTownStates,
                Sects: BuildNextSectStates(currentSects, regionTownIdsByRegion, nextTownStates, recruitsBySectId, wagesPaidBySectId),
                TurnNumber: nextTurn);

            return new TurnResult(NextState: nextState, Report: report);
        }

        private static MortalTownSimulationState CreateInitialTownState(MortalTownState town)
        {
            var foodBalance = CalculateFoodBalance(town, town.Population);

            return new MortalTownSimulationState(
                TownId: town.Id,
                TownName: town.Name,
                CurrentPopulation: town.Population,
                FoodBalance: foodBalance,
                RecruitmentPool: CalculateRecruitmentPool(town.Population),
                PopulationChange: 0,
                RecruitsLostLastTurn: 0,
                ChangeReason: DescribeNaturalReason(foodBalance, 0, TaxationPolicyCatalog.Get(TaxRateLevel.Standard)));
        }

        private static ImmutableArray<SectRecruitmentEvent> ResolveSectRecruitment(
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyDictionary<string, IReadOnlyList<string>> regionTownIdsByRegion,
            Dictionary<string, int> remainingRecruitmentPools,
            Dictionary<string, int> recruitsLostByTown)
        {
            var recruitmentEvents = new List<SectRecruitmentEvent>();

            foreach (var sect in sects)
            {
                var townIdsInRegion = regionTownIdsByRegion[sect.RegionId];
                var availableRecruitment = townIdsInRegion.Sum(townId => remainingRecruitmentPools[townId]);
                var wagePolicy = SectRecruitmentPolicyCatalog.Get(sect.RecruitmentWage);
                var wageTarget = wagePolicy.TargetRecruitsPerTurn;
                var targetRecruits = Math.Min(availableRecruitment, wageTarget);
                var affordableRecruits = (int)Math.Floor(sect.CurrentFunds / wagePolicy.WagePerRecruit);
                var recruitsGathered = Math.Min(targetRecruits, affordableRecruits);
                var wagesPaid = recruitsGathered * wagePolicy.WagePerRecruit;

                var recruitsStillNeeded = recruitsGathered;
                foreach (var townId in townIdsInRegion)
                {
                    if (recruitsStillNeeded == 0)
                    {
                        break;
                    }

                    var recruitsFromTown = Math.Min(recruitsStillNeeded, remainingRecruitmentPools[townId]);
                    if (recruitsFromTown == 0)
                    {
                        continue;
                    }

                    remainingRecruitmentPools[townId] -= recruitsFromTown;
                    recruitsLostByTown[townId] += recruitsFromTown;
                    recruitsStillNeeded -= recruitsFromTown;
                }

                recruitmentEvents.Add(new SectRecruitmentEvent(
                    SectId: sect.SectId,
                    SectName: sect.SectName,
                    RegionId: sect.RegionId,
                    RecruitmentWage: sect.RecruitmentWage,
                    RecruitsGathered: recruitsGathered,
                    WagesPaid: wagesPaid,
                    FundsRemaining: sect.CurrentFunds - wagesPaid,
                    OutcomeSummary: DescribeRecruitmentOutcome(availableRecruitment, wageTarget, targetRecruits, affordableRecruits, recruitsGathered)));
            }

            return recruitmentEvents.ToImmutableArray();
        }

        private static ImmutableArray<SectSimulationState> CreateInitialSectStates(
            IReadOnlyList<SectState> sects,
            IReadOnlyDictionary<string, IReadOnlyList<string>> regionTownIdsByRegion,
            IReadOnlyList<MortalTownSimulationState> towns)
        {
            var remainingPoolsByTown = towns.ToDictionary(town => town.TownId, town => town.RecruitmentPool);

            return sects
                .OrderBy(sect => sect.RegionId)
                .ThenBy(sect => sect.Id)
                .Select(sect =>
                {
                    var recruitablesFromRegion = regionTownIdsByRegion[sect.RegionId]
                        .Sum(townId => remainingPoolsByTown[townId]);

                    return new SectSimulationState(
                        SectId: sect.Id,
                        SectName: sect.Name,
                        RegionId: sect.RegionId,
                        CurrentFunds: sect.Funds,
                        CurrentPopulation: sect.Population,
                        Loyalty: sect.Loyalty,
                        IndustryPreference: sect.IndustryPreference,
                        RecruitmentWage: sect.RecruitmentWage,
                        CurrentOutput: sect.Output.ToImmutableArray(),
                        RecruitablesFromRegion: recruitablesFromRegion,
                        LastRecruitsGained: 0,
                        LastWagesPaid: 0m);
                })
                .ToImmutableArray();
        }

        private static ImmutableArray<SectSimulationState> BuildNextSectStates(
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyDictionary<string, IReadOnlyList<string>> regionTownIdsByRegion,
            IReadOnlyList<MortalTownSimulationState> towns,
            IReadOnlyDictionary<string, int> recruitsBySectId,
            IReadOnlyDictionary<string, decimal> wagesPaidBySectId)
        {
            var remainingPoolsByTown = towns.ToDictionary(town => town.TownId, town => town.RecruitmentPool);

            return sects
                .Select(sect =>
                {
                    var recruitablesFromRegion = regionTownIdsByRegion[sect.RegionId]
                        .Sum(townId => remainingPoolsByTown[townId]);

                    return sect with
                    {
                        CurrentFunds = sect.CurrentFunds - wagesPaidBySectId.GetValueOrDefault(sect.SectId),
                        CurrentPopulation = sect.CurrentPopulation + recruitsBySectId.GetValueOrDefault(sect.SectId),
                        RecruitablesFromRegion = recruitablesFromRegion,
                        LastRecruitsGained = recruitsBySectId.GetValueOrDefault(sect.SectId),
                        LastWagesPaid = wagesPaidBySectId.GetValueOrDefault(sect.SectId)
                    };
                })
                .ToImmutableArray();
        }

        private static string DescribeRecruitmentOutcome(
            int availableRecruitment,
            int wageTarget,
            int targetRecruits,
            int affordableRecruits,
            int recruitsGathered)
        {
            if (availableRecruitment == 0)
            {
                return "No recruitable mortals remained in the region.";
            }

            if (affordableRecruits == 0)
            {
                return "Funds were too low to pay even one recruit at the current wage.";
            }

            if (recruitsGathered < targetRecruits)
            {
                return "Recruitment stopped when sect funds ran short.";
            }

            if (availableRecruitment < wageTarget)
            {
                return "Regional recruitment supply limited this turn's hiring.";
            }

            return "Recruitment met the current wage-driven hiring target.";
        }

        private static NaturalTownResult ResolveNaturalChange(
            MortalTownState town,
            MortalTownSimulationState currentTown,
            TaxRatePolicy taxPolicy)
        {
            var currentPopulation = currentTown.CurrentPopulation;
            var foodBalance = CalculateFoodBalance(town, currentPopulation);

            if (foodBalance > 0)
            {
                var adjustedGrowthRate = Math.Max(0.001m, GrowthRateWhenSatisfied + taxPolicy.GrowthRateModifier);
                var naturalGrowth = Math.Max(1, (int)Math.Floor(currentPopulation * adjustedGrowthRate));
                var growthSupportedByFood = foodBalance / town.FoodConsumptionPerCapita;
                var actualGrowth = Math.Min(naturalGrowth, growthSupportedByFood);
                var populationAfterNaturalChange = currentPopulation + actualGrowth;

                return new NaturalTownResult(
                    TownId: town.Id,
                    TownName: town.Name,
                    PreviousPopulation: currentPopulation,
                    PopulationAfterNaturalChange: populationAfterNaturalChange,
                    RecruitmentPoolBeforeRecruitment: CalculateRecruitmentPool(populationAfterNaturalChange),
                    NaturalReason: DescribeNaturalReason(foodBalance, actualGrowth, taxPolicy));
            }

            if (foodBalance < 0)
            {
                var populationShortfall = (int)Math.Ceiling((decimal)(-foodBalance) / town.FoodConsumptionPerCapita);
                var adjustedDeclineRate = Math.Max(0.005m, DeclineRateWhenUnsatisfied + taxPolicy.DeclineRateModifier);
                var naturalDecline = Math.Max(1, (int)Math.Ceiling(currentPopulation * adjustedDeclineRate));
                var actualDecline = Math.Min(currentPopulation - MinimumPopulation, Math.Max(populationShortfall, naturalDecline));
                var populationAfterNaturalChange = Math.Max(MinimumPopulation, currentPopulation - actualDecline);

                return new NaturalTownResult(
                    TownId: town.Id,
                    TownName: town.Name,
                    PreviousPopulation: currentPopulation,
                    PopulationAfterNaturalChange: populationAfterNaturalChange,
                    RecruitmentPoolBeforeRecruitment: CalculateRecruitmentPool(populationAfterNaturalChange),
                    NaturalReason: DescribeNaturalReason(foodBalance, -actualDecline, taxPolicy));
            }

            return new NaturalTownResult(
                TownId: town.Id,
                TownName: town.Name,
                PreviousPopulation: currentPopulation,
                PopulationAfterNaturalChange: currentPopulation,
                RecruitmentPoolBeforeRecruitment: CalculateRecruitmentPool(currentPopulation),
                NaturalReason: DescribeNaturalReason(foodBalance, 0, taxPolicy));
        }

        private static int CalculateFoodBalance(MortalTownState town, int population)
            => town.FoodProduction - (population * town.FoodConsumptionPerCapita);

        private static int CalculateRecruitmentPool(int population)
            => population <= MinimumPopulation
                ? 0
                : (int)Math.Floor((population - MinimumPopulation) * RecruitablesRate);

        private static string DescribeNaturalReason(int foodBalance, int naturalChange, TaxRatePolicy taxPolicy)
        {
            var baselineReason = foodBalance < 0
                ? "Hardship"
                : naturalChange > 0
                    ? "Prosperity"
                    : "Stable";

            if (string.IsNullOrWhiteSpace(taxPolicy.ChangeReasonSuffix))
            {
                return baselineReason;
            }

            return $"{baselineReason}; {taxPolicy.ChangeReasonSuffix}";
        }

        private static string BuildChangeReason(string naturalReason, int recruitsLost)
            => recruitsLost > 0
                ? $"{naturalReason}; sect recruitment drew {recruitsLost}"
                : naturalReason;

        private sealed record NaturalTownResult(
            string TownId,
            string TownName,
            int PreviousPopulation,
            int PopulationAfterNaturalChange,
            int RecruitmentPoolBeforeRecruitment,
            string NaturalReason);
    }
}
