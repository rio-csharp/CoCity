using CoCity.Foundation;
using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultMortalTaxationSimulationServiceTests
{
    private static readonly SeedCoreDataFoundationService FoundationService = new();
    private static readonly DefaultMortalRealmSimulationService RealmSimulationService = new();
    private static readonly DefaultMortalIndustrySimulationService IndustrySimulationService = new();
    private static readonly DefaultMortalTaxationSimulationService TaxationSimulationService = new();

    [Fact]
    public void Initialize_starts_with_standard_rate_and_projected_revenue()
    {
        var foundation = FoundationService.GetInitialState();
        var realmState = RealmSimulationService.Initialize(foundation);
        var industryState = IndustrySimulationService.Initialize(foundation, foundation.Ministries);

        var taxationState = TaxationSimulationService.Initialize(foundation, realmState, industryState);

        Assert.Equal(TaxRateLevel.Standard, taxationState.SelectedTaxRate);
        Assert.Equal(foundation.Treasury.Funds, taxationState.CurrentTreasuryFunds);
        Assert.Equal(0m, taxationState.LastCollectedRevenue);
        Assert.Equal(foundation.Towns.Count, taxationState.Towns.Count);
        Assert.True(taxationState.ProjectedRevenue > 0m);
    }

    [Fact]
    public void SetTaxRate_recalculates_projection_and_stability_effect()
    {
        var foundation = FoundationService.GetInitialState();
        var realmState = RealmSimulationService.Initialize(foundation);
        var industryState = IndustrySimulationService.Initialize(foundation, foundation.Ministries);
        var currentState = TaxationSimulationService.Initialize(foundation, realmState, industryState);

        var lightState = TaxationSimulationService.SetTaxRate(
            currentState,
            realmState,
            industryState,
            TaxRateLevel.Light);
        var heavyState = TaxationSimulationService.SetTaxRate(
            currentState,
            realmState,
            industryState,
            TaxRateLevel.Heavy);

        Assert.True(heavyState.ProjectedRevenue > currentState.ProjectedRevenue);
        Assert.True(lightState.ProjectedRevenue < currentState.ProjectedRevenue);

        var lightTown = Assert.Single(lightState.Towns, town => town.TownId == "town.azure-ford");
        var heavyTown = Assert.Single(heavyState.Towns, town => town.TownId == "town.azure-ford");

        Assert.True(lightTown.StabilityDelta > 0);
        Assert.True(heavyTown.StabilityDelta < 0);
    }

    [Fact]
    public void Step_collects_projected_revenue_into_treasury()
    {
        var foundation = FoundationService.GetInitialState();
        var realmState = RealmSimulationService.Initialize(foundation);
        var industryState = IndustrySimulationService.Initialize(foundation, foundation.Ministries);
        var currentState = TaxationSimulationService.Initialize(foundation, realmState, industryState);

        var result = TaxationSimulationService.Step(currentState, realmState, industryState);

        Assert.Equal(currentState.ProjectedRevenue, result.Report.CollectedRevenue);
        Assert.Equal(currentState.CurrentTreasuryFunds, result.Report.TreasuryBeforeCollection);
        Assert.Equal(currentState.CurrentTreasuryFunds + currentState.ProjectedRevenue, result.Report.TreasuryAfterCollection);
        Assert.Equal(result.Report.TreasuryAfterCollection, result.NextState.CurrentTreasuryFunds);
        Assert.Equal(result.Report.CollectedRevenue, result.NextState.LastCollectedRevenue);
    }

    [Fact]
    public void Step_accumulates_treasury_across_multiple_turns()
    {
        var foundation = FoundationService.GetInitialState();
        var realmState = RealmSimulationService.Initialize(foundation);
        var industryState = IndustrySimulationService.Initialize(foundation, foundation.Ministries);
        var taxationState = TaxationSimulationService.Initialize(foundation, realmState, industryState);

        var firstRealmTurn = RealmSimulationService.Step(foundation, realmState, taxationState.SelectedTaxRate);
        var firstIndustryTurn = IndustrySimulationService.Step(
            foundation,
            foundation.Ministries,
            firstRealmTurn.NextState,
            industryState);
        var firstTaxTurn = TaxationSimulationService.Step(
            taxationState,
            firstRealmTurn.NextState,
            firstIndustryTurn.NextStates);

        var secondRealmTurn = RealmSimulationService.Step(
            foundation,
            firstRealmTurn.NextState,
            firstTaxTurn.NextState.SelectedTaxRate);
        var secondIndustryTurn = IndustrySimulationService.Step(
            foundation,
            foundation.Ministries,
            secondRealmTurn.NextState,
            firstIndustryTurn.NextStates);
        var secondTaxTurn = TaxationSimulationService.Step(
            firstTaxTurn.NextState,
            secondRealmTurn.NextState,
            secondIndustryTurn.NextStates);

        Assert.Equal(
            foundation.Treasury.Funds + firstTaxTurn.Report.CollectedRevenue + secondTaxTurn.Report.CollectedRevenue,
            secondTaxTurn.NextState.CurrentTreasuryFunds);
    }
}
