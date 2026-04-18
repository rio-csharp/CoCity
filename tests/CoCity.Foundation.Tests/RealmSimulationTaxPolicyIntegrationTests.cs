using CoCity.Foundation;
using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class RealmSimulationTaxPolicyIntegrationTests
{
    [Fact]
    public void Light_tax_supports_population_better_than_heavy_tax()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var simulationService = new DefaultMortalRealmSimulationService();
        var initialState = simulationService.Initialize(foundation);

        var lightResult = simulationService.Step(foundation, initialState, TaxRateLevel.Light);
        var heavyResult = simulationService.Step(foundation, initialState, TaxRateLevel.Heavy);

        var lightTown = Assert.Single(lightResult.NextState.Towns, town => town.TownId == "town.azure-ford");
        var heavyTown = Assert.Single(heavyResult.NextState.Towns, town => town.TownId == "town.azure-ford");

        Assert.True(lightTown.CurrentPopulation > heavyTown.CurrentPopulation);
        Assert.Contains("tax relief", lightTown.ChangeReason);
        Assert.Contains("tax strain", heavyTown.ChangeReason);
    }
}
