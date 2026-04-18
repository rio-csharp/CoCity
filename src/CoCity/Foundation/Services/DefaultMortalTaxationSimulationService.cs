using System.Collections.Immutable;

namespace CoCity.Foundation.Services
{
    public sealed class DefaultMortalTaxationSimulationService : IMortalTaxationSimulationService
    {
        private const decimal PopulationTaxWeight = 0.05m;
        private const decimal AgricultureTaxWeight = 0.01m;
        private const decimal HandicraftsTaxWeight = 0.03m;
        private const decimal CommerceTaxWeight = 0.08m;

        public RealmTaxationState Initialize(
            RealmState foundation,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates)
            => BuildState(
                treasuryFunds: foundation.Treasury.Funds,
                lastCollectedRevenue: 0m,
                selectedTaxRate: TaxRateLevel.Standard,
                mortalRealmState: mortalRealmState,
                industryStates: industryStates);

        public RealmTaxationState SetTaxRate(
            RealmTaxationState currentState,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            TaxRateLevel taxRate)
            => BuildState(
                treasuryFunds: currentState.CurrentTreasuryFunds,
                lastCollectedRevenue: currentState.LastCollectedRevenue,
                selectedTaxRate: taxRate,
                mortalRealmState: mortalRealmState,
                industryStates: industryStates);

        public TaxationTurnResult Step(
            RealmTaxationState currentState,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates)
        {
            var nextState = BuildState(
                treasuryFunds: currentState.CurrentTreasuryFunds,
                lastCollectedRevenue: currentState.LastCollectedRevenue,
                selectedTaxRate: currentState.SelectedTaxRate,
                mortalRealmState: mortalRealmState,
                industryStates: industryStates);

            var collectedRevenue = nextState.ProjectedRevenue;
            var treasuryAfterCollection = currentState.CurrentTreasuryFunds + collectedRevenue;
            var finalizedState = nextState with
            {
                CurrentTreasuryFunds = treasuryAfterCollection,
                LastCollectedRevenue = collectedRevenue
            };

            var report = new TaxationTurnReport(
                TurnNumber: mortalRealmState.TurnNumber,
                SelectedTaxRate: finalizedState.SelectedTaxRate,
                TreasuryBeforeCollection: currentState.CurrentTreasuryFunds,
                CollectedRevenue: collectedRevenue,
                TreasuryAfterCollection: treasuryAfterCollection,
                TownEvents: finalizedState.Towns
                    .Select(town => new TownTaxationEvent(
                        TownId: town.TownId,
                        TownName: town.TownName,
                        GrossTaxBase: town.GrossTaxBase,
                        CollectedRevenue: town.CollectedRevenue,
                        StabilityDelta: town.StabilityDelta,
                        StabilitySummary: town.StabilitySummary))
                    .ToImmutableArray());

            return new TaxationTurnResult(
                NextState: finalizedState,
                Report: report);
        }

        private static RealmTaxationState BuildState(
            decimal treasuryFunds,
            decimal lastCollectedRevenue,
            TaxRateLevel selectedTaxRate,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates)
        {
            var policy = TaxationPolicyCatalog.Get(selectedTaxRate);
            var industryByTownId = industryStates.ToDictionary(state => state.TownId);

            var towns = mortalRealmState.Towns
                .Select(town =>
                {
                    if (!industryByTownId.TryGetValue(town.TownId, out var industryState))
                    {
                        throw new InvalidOperationException(
                            $"Missing industry state for town '{town.TownId}'. Taxation requires realm and industry simulation states to stay aligned.");
                    }

                    var grossTaxBase = RoundToSingleDecimal(
                        (town.CurrentPopulation * PopulationTaxWeight) +
                        (industryState.NetOutput.AgricultureUnits * AgricultureTaxWeight) +
                        (industryState.NetOutput.HandicraftsUnits * HandicraftsTaxWeight) +
                        (industryState.NetOutput.CommerceUnits * CommerceTaxWeight));

                    var collectedRevenue = RoundToSingleDecimal(grossTaxBase * policy.RevenueMultiplier);

                    return new TownTaxationState(
                        TownId: town.TownId,
                        TownName: town.TownName,
                        GrossTaxBase: grossTaxBase,
                        CollectedRevenue: collectedRevenue,
                        StabilityDelta: policy.StabilityDelta,
                        StabilitySummary: policy.StabilitySummary);
                })
                .ToImmutableArray();

            return new RealmTaxationState(
                CurrentTreasuryFunds: treasuryFunds,
                LastCollectedRevenue: lastCollectedRevenue,
                SelectedTaxRate: selectedTaxRate,
                ProjectedRevenue: towns.Sum(town => town.CollectedRevenue),
                Towns: towns);
        }

        private static decimal RoundToSingleDecimal(decimal value)
            => decimal.Floor(value * 10m) / 10m;
    }
}
