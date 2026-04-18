using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultBuildingSystemServiceTests
{
    [Fact]
    public void ConstructNextSectBuildings_spends_sect_funds_and_adds_gate_halls()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);

        var result = buildingService.ConstructNextSectBuildings(
            buildingState,
            realmState.Sects,
            currentTreasuryFunds: foundation.Treasury.Funds);

        var azureSect = Assert.Single(result.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureInventory = Assert.Single(result.NextState.Sects, item => item.SectId == "sect.azure-talisman-academy");

        Assert.Equal(4600m, azureSect.CurrentFunds);
        Assert.Contains(azureInventory.Buildings, item => item.Building == SectBuildingType.GateHall && item.Quantity == 1);
        Assert.NotEmpty(result.Report.ConstructionEvents);
    }

    [Fact]
    public void ApplyTurn_applies_built_building_effects_to_sects_and_towns()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
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
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);

        var result = buildingService.ApplyTurn(
            foundation,
            townConstruction.NextState,
            townConstruction.NextSects,
            industryStates,
            townConstruction.NextTreasuryFunds);

        var azureSect = Assert.Single(result.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureTown = Assert.Single(result.NextIndustryStates, state => state.TownId == "town.azure-ford");
        var baselineAzureSect = Assert.Single(foundation.Sects, sect => sect.Id == azureSect.SectId);
        var baselineAzureTown = Assert.Single(industryStates, state => state.TownId == azureTown.TownId);

        Assert.Equal(4565m, azureSect.CurrentFunds);
        Assert.True(azureSect.CurrentOutput[0].Amount > baselineAzureSect.Output[0].Amount);
        Assert.True(azureTown.NetOutput.AgricultureUnits > baselineAzureTown.NetOutput.AgricultureUnits);
        Assert.True(result.NextTreasuryFunds < townConstruction.NextTreasuryFunds);
        Assert.NotEmpty(result.Report.OperationEvents);
    }

    [Fact]
    public void ApplyTurn_preserves_existing_sect_output_scaling_before_building_bonus()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var sectConstruction = buildingService.ConstructNextSectBuildings(
            buildingState,
            realmState.Sects,
            currentTreasuryFunds: foundation.Treasury.Funds);

        var reducedRealmState = sectConstruction.NextSects
            .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                ? sect with
                {
                    CurrentOutput = sect.CurrentOutput
                        .Select(output => output with { Amount = Math.Round(output.Amount * 0.5m, 1, MidpointRounding.AwayFromZero) })
                        .ToArray()
                }
                : sect)
            .ToArray();

        var result = buildingService.ApplyTurn(
            foundation,
            sectConstruction.NextState,
            reducedRealmState,
            [],
            sectConstruction.NextTreasuryFunds);

        var azureSect = Assert.Single(result.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");

        Assert.Equal(30.5m, azureSect.CurrentOutput[0].Amount);
        Assert.Equal(9.5m, azureSect.CurrentOutput[1].Amount);
    }
}
