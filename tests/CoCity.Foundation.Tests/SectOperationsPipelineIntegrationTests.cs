using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class SectOperationsPipelineIntegrationTests
{
    [Fact]
    public void Turn_pipeline_applies_autonomous_sect_operations_after_recruitment_and_industry()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);

        var initialRealmState = realmService.Initialize(foundation);
        var initialIndustryStates = industryService.Initialize(foundation, foundation.Ministries);

        var realmResult = realmService.Step(foundation, initialRealmState);
        var industryResult = industryService.Step(
            foundation,
            foundation.Ministries,
            realmResult.NextState,
            initialIndustryStates);
        var sectOperationsResult = sectOperationsService.Step(
            foundation,
            realmResult.NextState,
            industryResult.NextStates);

        var baselineAzureIndustry = Assert.Single(industryResult.NextStates, state => state.TownId == "town.azure-ford");
        var azureSect = Assert.Single(sectOperationsResult.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureEvent = Assert.Single(sectOperationsResult.Report.SectEvents, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureTown = Assert.Single(sectOperationsResult.NextIndustryStates, state => state.TownId == "town.azure-ford");

        Assert.Equal(4345m, azureSect.CurrentFunds);
        Assert.Equal(1m, azureEvent.OutputFactor);
        Assert.Equal(120, azureEvent.PurchasedUnits);
        Assert.Equal(180m, azureEvent.UpkeepPaid);
        Assert.Equal(360m, azureEvent.InputPurchaseCost);
        Assert.Equal(baselineAzureIndustry.PurchasableSurplus.HandicraftsUnits - 120, azureTown.PurchasableSurplus.HandicraftsUnits);
    }
}
