using System.Collections.Immutable;
using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultMortalIndustrySimulationService : IMortalIndustrySimulationService
    {
        private const decimal MinEfficiency = 0.7m;
        private const decimal MaxEfficiency = 1.3m;
        private const decimal BaselineAdministrationRating = 75m;

        public IReadOnlyList<MortalTownIndustryState> Initialize(
            RealmState foundation,
            IReadOnlyList<MinistryState> ministries)
        {
            var efficiency = CalculateGovernmentEfficiency(ministries);

            return foundation.Towns
                .Select(town => CalculateTownIndustryState(town, town.Population, efficiency))
                .ToImmutableArray();
        }

        public IndustryTurnResult Step(
            RealmState foundation,
            IReadOnlyList<MinistryState> ministries,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> currentStates)
        {
            var efficiency = CalculateGovernmentEfficiency(ministries);
            var currentPopulationByTownId = mortalRealmState.Towns
                .ToDictionary(t => t.TownId, t => t.CurrentPopulation);

            var nextStates = foundation.Towns
                .Select(town =>
                {
                    var currentPopulation = currentPopulationByTownId.GetValueOrDefault(town.Id, town.Population);
                    return CalculateTownIndustryState(town, currentPopulation, efficiency);
                })
                .ToImmutableArray();

            var townEvents = nextStates
                .Select(state => new TownIndustryEvent(
                    TownId: state.TownId,
                    TownName: state.TownName,
                    LaborForce: state.LaborForce,
                    GrossOutput: state.GrossOutput,
                    GovernmentEfficiency: state.GovernmentEfficiency,
                    NetOutput: state.NetOutput,
                    PurchasableSurplus: state.PurchasableSurplus))
                .ToImmutableArray();

            var report = new IndustryTurnReport(
                TownIndustryEvents: townEvents);

            return new IndustryTurnResult(nextStates, report);
        }

        private static MortalTownIndustryState CalculateTownIndustryState(
            MortalTownState town,
            int currentPopulation,
            decimal governmentEfficiency)
        {
            var laborForce = CalculateLaborForce(town, currentPopulation);
            var grossOutput = CalculateGrossOutput(town.BaseOutputPerWorker, laborForce);
            var netOutput = ApplyEfficiency(grossOutput, governmentEfficiency);
            var purchasableSurplus = CalculatePurchasableSurplus(netOutput);

            return new MortalTownIndustryState(
                TownId: town.Id,
                TownName: town.Name,
                LaborForce: laborForce,
                GrossOutput: grossOutput,
                GovernmentEfficiency: governmentEfficiency,
                NetOutput: netOutput,
                PurchasableSurplus: purchasableSurplus);
        }

        private static LaborForceDistribution CalculateLaborForce(MortalTownState town, int currentPopulation)
        {
            // Working population is total population minus the minimum population floor (100)
            var workingPopulation = Math.Max(0, currentPopulation - 100);

            // Distribute working population according to industry allocation percentages
            var agricultureShare = town.Industries
                .FirstOrDefault(i => i.Industry == MortalIndustryType.Agriculture)
                ?.WorkforceShare ?? 0;
            var handicraftsShare = town.Industries
                .FirstOrDefault(i => i.Industry == MortalIndustryType.Handicrafts)
                ?.WorkforceShare ?? 0;
            var commerceShare = town.Industries
                .FirstOrDefault(i => i.Industry == MortalIndustryType.Commerce)
                ?.WorkforceShare ?? 0;

            return new LaborForceDistribution(
                Agriculture: (int)Math.Floor(workingPopulation * agricultureShare / 100m),
                Handicrafts: (int)Math.Floor(workingPopulation * handicraftsShare / 100m),
                Commerce: (int)Math.Floor(workingPopulation * commerceShare / 100m));
        }

        private static IndustryOutput CalculateGrossOutput(
            IndustryBaseOutputRates baseRates,
            LaborForceDistribution laborForce)
        {
            return new IndustryOutput(
                AgricultureUnits: laborForce.Agriculture * baseRates.AgriculturePerWorker,
                HandicraftsUnits: laborForce.Handicrafts * baseRates.HandicraftsPerWorker,
                CommerceUnits: laborForce.Commerce * baseRates.CommercePerWorker);
        }

        private static IndustryOutput ApplyEfficiency(IndustryOutput gross, decimal efficiency)
        {
            return new IndustryOutput(
                AgricultureUnits: (int)Math.Floor(gross.AgricultureUnits * efficiency),
                HandicraftsUnits: (int)Math.Floor(gross.HandicraftsUnits * efficiency),
                CommerceUnits: (int)Math.Floor(gross.CommerceUnits * efficiency));
        }

        private static IndustryOutput CalculatePurchasableSurplus(IndustryOutput netOutput)
        {
            // For prototype: all net output is purchasable by sects
            // In later tasks, some portion may be needed for town maintenance
            return netOutput;
        }

        private static decimal CalculateGovernmentEfficiency(IReadOnlyList<MinistryState> ministries)
        {
            // Use Ministry of Revenue minister's administration rating as government efficiency
            var revenueMinistry = ministries.FirstOrDefault(m => m.Id == "ministry.revenue");
            if (revenueMinistry == null)
            {
                return 1.0m;
            }

            var administrationRating = revenueMinistry.Minister.Ratings.Administration;
            var efficiency = 1.0m + (administrationRating - BaselineAdministrationRating) / 250m;
            return Math.Clamp(efficiency, MinEfficiency, MaxEfficiency);
        }
    }
}
