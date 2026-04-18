using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class BuildingSystemPipelineIntegrationTests
{
    [Fact]
    public void Turn_pipeline_keeps_building_effects_visible_after_construction()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);
        var buildingService = new DefaultBuildingSystemService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var sectConstruction = buildingService.ConstructNextSectBuildings(
            buildingState,
            realmState.Sects,
            currentTreasuryFunds: foundation.Treasury.Funds);
        var townConstruction = buildingService.ConstructNextTownBuildings(
            sectConstruction.NextState,
            sectConstruction.NextSects,
            realmState.Towns,
            sectConstruction.NextTreasuryFunds);

        var realmResult = realmService.Step(foundation, realmState);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var industryResult = industryService.Step(
            foundation,
            foundation.Ministries,
            realmResult.NextState,
            industryStates);
        var sectOpsResult = sectOperationsService.Step(
            foundation,
            realmResult.NextState with { Sects = townConstruction.NextSects },
            industryResult.NextStates);
        var buildingTurnResult = buildingService.ApplyTurn(
            foundation,
            townConstruction.NextState,
            sectOpsResult.NextSects,
            sectOpsResult.NextIndustryStates,
            townConstruction.NextTreasuryFunds);

        var azureSect = Assert.Single(buildingTurnResult.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureTown = Assert.Single(buildingTurnResult.NextIndustryStates, state => state.TownId == "town.azure-ford");

        Assert.True(azureSect.CurrentOutput[0].Amount > foundation.Sects.Single(sect => sect.Id == azureSect.SectId).Output[0].Amount);
        Assert.True(azureTown.NetOutput.AgricultureUnits > industryResult.NextStates.Single(state => state.TownId == "town.azure-ford").NetOutput.AgricultureUnits);
        Assert.True(buildingTurnResult.NextTreasuryFunds < townConstruction.NextTreasuryFunds);
    }
}
