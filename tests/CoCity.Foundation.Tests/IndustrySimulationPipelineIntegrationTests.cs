using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class IndustrySimulationPipelineIntegrationTests
{
    [Fact]
    public void Turn_pipeline_combines_population_simulation_with_industry_output_and_reports()
    {
        var foundationService = new SeedCoreDataFoundationService();
        var realmSimulationService = new DefaultMortalRealmSimulationService();
        var industrySimulationService = new DefaultMortalIndustrySimulationService();

        var foundation = foundationService.GetInitialState();
        var initialRealmState = realmSimulationService.Initialize(foundation);
        var initialIndustryStates = industrySimulationService.Initialize(foundation, foundation.Ministries);

        var turnResult = realmSimulationService.Step(foundation, initialRealmState);
        var industryResult = industrySimulationService.Step(
            foundation,
            foundation.Ministries,
            turnResult.NextState,
            initialIndustryStates);

        Assert.Equal(1, turnResult.NextState.TurnNumber);
        Assert.Equal(foundation.Towns.Count, turnResult.Report.TownEvents.Count);
        Assert.Equal(foundation.Towns.Count, industryResult.NextStates.Count);
        Assert.Equal(foundation.Towns.Count, industryResult.Report.TownIndustryEvents.Count);

        var azureFordTown = Assert.Single(turnResult.NextState.Towns, town => town.TownId == "town.azure-ford");
        var azureFordIndustry = Assert.Single(industryResult.NextStates, state => state.TownId == "town.azure-ford");
        var azureFordIndustryEvent = Assert.Single(
            industryResult.Report.TownIndustryEvents,
            townEvent => townEvent.TownId == "town.azure-ford");
        var azureRecruitment = Assert.Single(
            turnResult.Report.RecruitmentEvents,
            recruitmentEvent => recruitmentEvent.SectId == "sect.azure-talisman-academy");

        Assert.Equal(35, azureRecruitment.RecruitsGathered);
        Assert.Equal(18347, azureFordTown.CurrentPopulation);
        Assert.Equal(new LaborForceDistribution(8211, 5474, 4561), azureFordIndustry.LaborForce);
        Assert.Equal(new IndustryOutput(82110, 32844, 18244), azureFordIndustry.GrossOutput);
        Assert.Equal(azureFordIndustry.LaborForce, azureFordIndustryEvent.LaborForce);
        Assert.Equal(azureFordIndustry.GrossOutput, azureFordIndustryEvent.GrossOutput);
        Assert.Equal(azureFordIndustry.GovernmentEfficiency, azureFordIndustryEvent.GovernmentEfficiency);
        Assert.Equal(azureFordIndustry.NetOutput, azureFordIndustryEvent.NetOutput);
        Assert.Equal(azureFordIndustry.PurchasableSurplus, azureFordIndustryEvent.PurchasableSurplus);
    }
}
