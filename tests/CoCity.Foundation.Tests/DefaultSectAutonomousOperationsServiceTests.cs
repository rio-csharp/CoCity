using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultSectAutonomousOperationsServiceTests
{
    [Fact]
    public void Step_consumes_upkeep_and_purchases_inputs_when_sect_can_fund_operations()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);

        var realmState = realmService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);

        var result = sectOperationsService.Step(foundation, realmState, industryStates);

        var azureSect = Assert.Single(result.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureEvent = Assert.Single(result.Report.SectEvents, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureTown = Assert.Single(result.NextIndustryStates, state => state.TownId == "town.azure-ford");

        Assert.Equal(4660m, azureSect.CurrentFunds);
        Assert.Equal(1m, azureEvent.OutputFactor);
        Assert.Equal(120, azureEvent.PurchasedUnits);
        Assert.Equal(180m, azureEvent.UpkeepPaid);
        Assert.Equal(360m, azureEvent.InputPurchaseCost);
        Assert.Equal(34154, azureTown.PurchasableSurplus.HandicraftsUnits);
        Assert.Equal(foundation.Sects.Single(sect => sect.Id == azureSect.SectId).Output, azureSect.CurrentOutput);
    }

    [Fact]
    public void Step_reduces_output_when_funds_cannot_cover_upkeep_and_inputs()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);

        var realmState = realmService.Initialize(foundation);
        var constrainedRealmState = realmState with
        {
            Sects = realmState.Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { CurrentFunds = 300m }
                    : sect)
                .ToArray()
        };
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);

        var result = sectOperationsService.Step(foundation, constrainedRealmState, industryStates);

        var azureSect = Assert.Single(result.NextSects, sect => sect.SectId == "sect.azure-talisman-academy");
        var azureEvent = Assert.Single(result.Report.SectEvents, sect => sect.SectId == "sect.azure-talisman-academy");

        Assert.Equal(0m, azureSect.CurrentFunds);
        Assert.Equal(40, azureEvent.PurchasedUnits);
        Assert.True(azureEvent.OutputFactor < 1m);
        Assert.All(azureSect.CurrentOutput, output => Assert.True(output.Amount < foundation.Sects.Single(sect => sect.Id == azureSect.SectId).Output.Single(item => item.Label == output.Label).Amount));
        Assert.Contains("Funds ran out", azureEvent.OperationSummary);
    }
}
