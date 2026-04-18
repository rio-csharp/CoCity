using CoCity.Foundation;
using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultMortalIndustrySimulationServiceTests
{
    private static readonly SeedCoreDataFoundationService FoundationService = new();
    private static readonly DefaultMortalIndustrySimulationService IndustryService = new();
    private static readonly DefaultMortalRealmSimulationService RealmSimulationService = new();

    [Fact]
    public void Initialize_calculates_seeded_labor_output_and_efficiency()
    {
        var foundation = FoundationService.GetInitialState();

        var states = IndustryService.Initialize(foundation, foundation.Ministries);
        var azureFord = Assert.Single(states, state => state.TownId == "town.azure-ford");

        Assert.Equal(new LaborForceDistribution(8145, 5430, 4525), azureFord.LaborForce);
        Assert.Equal(new IndustryOutput(81450, 32580, 18100), azureFord.GrossOutput);
        Assert.Equal(1.052m, azureFord.GovernmentEfficiency);
        Assert.Equal(new IndustryOutput(85685, 34274, 19041), azureFord.NetOutput);
        Assert.Equal(azureFord.NetOutput, azureFord.PurchasableSurplus);
    }

    [Fact]
    public void Step_uses_current_population_from_realm_state()
    {
        var foundation = FoundationService.GetInitialState();
        var currentRealmState = RealmSimulationService.Initialize(foundation);
        var currentIndustryStates = IndustryService.Initialize(foundation, foundation.Ministries);

        var adjustedTowns = currentRealmState.Towns
            .Select(town => town.TownId == "town.azure-ford"
                ? town with { CurrentPopulation = 20000 }
                : town)
            .ToArray();

        var adjustedRealmState = currentRealmState with { Towns = adjustedTowns };

        var result = IndustryService.Step(
            foundation,
            foundation.Ministries,
            adjustedRealmState,
            currentIndustryStates);

        var azureFord = Assert.Single(result.NextStates, state => state.TownId == "town.azure-ford");

        Assert.Equal(new LaborForceDistribution(8955, 5970, 4975), azureFord.LaborForce);
        Assert.Equal(new IndustryOutput(89550, 35820, 19900), azureFord.GrossOutput);
        Assert.Equal(new IndustryOutput(94206, 37682, 20934), azureFord.NetOutput);
    }

    [Fact]
    public void ProcessPurchases_reduces_surplus_and_sect_funds_for_filled_request()
    {
        var foundation = FoundationService.GetInitialState();
        var currentStates = IndustryService.Initialize(foundation, foundation.Ministries);

        var result = IndustryService.ProcessPurchases(
            currentStates,
            foundation.Sects,
            [
                new SectPurchaseRequest(
                    SectId: "sect.azure-talisman-academy",
                    TownId: "town.azure-ford",
                    Industry: MortalIndustryType.Agriculture,
                    Quantity: 1000,
                    UnitPrice: 2m)
            ]);

        var azureFord = Assert.Single(result.NextStates, state => state.TownId == "town.azure-ford");
        var receipt = Assert.Single(result.Report.Receipts);
        var settlement = Assert.Single(result.Settlements, item => item.SectId == "sect.azure-talisman-academy");

        Assert.Equal(84685, azureFord.PurchasableSurplus.AgricultureUnits);
        Assert.Equal(34274, azureFord.PurchasableSurplus.HandicraftsUnits);
        Assert.Equal(19041, azureFord.PurchasableSurplus.CommerceUnits);

        Assert.Equal(1000, receipt.PurchasedQuantity);
        Assert.Equal(2000m, receipt.FundsSpent);
        Assert.Equal(3200m, receipt.FundsRemaining);
        Assert.Equal("Filled", receipt.Resolution);
        Assert.Equal(2000m, settlement.FundsSpent);
        Assert.Equal(3200m, settlement.FundsRemaining);
    }

    [Fact]
    public void ProcessPurchases_caps_purchase_by_affordable_quantity()
    {
        var foundation = FoundationService.GetInitialState();
        var currentStates = IndustryService.Initialize(foundation, foundation.Ministries);

        var result = IndustryService.ProcessPurchases(
            currentStates,
            foundation.Sects,
            [
                new SectPurchaseRequest(
                    SectId: "sect.azure-talisman-academy",
                    TownId: "town.azure-ford",
                    Industry: MortalIndustryType.Handicrafts,
                    Quantity: 5000,
                    UnitPrice: 10m)
            ]);

        var azureFord = Assert.Single(result.NextStates, state => state.TownId == "town.azure-ford");
        var receipt = Assert.Single(result.Report.Receipts);
        var settlement = Assert.Single(result.Settlements, item => item.SectId == "sect.azure-talisman-academy");

        Assert.Equal(33754, azureFord.PurchasableSurplus.HandicraftsUnits);
        Assert.Equal(520, receipt.PurchasedQuantity);
        Assert.Equal(5200m, receipt.FundsSpent);
        Assert.Equal(0m, receipt.FundsRemaining);
        Assert.Equal("Partially filled", receipt.Resolution);
        Assert.Equal(5200m, settlement.FundsSpent);
        Assert.Equal(0m, settlement.FundsRemaining);
    }

    [Fact]
    public void ProcessPurchases_caps_purchase_by_available_surplus()
    {
        var foundation = FoundationService.GetInitialState();
        var currentStates = IndustryService.Initialize(foundation, foundation.Ministries);

        var result = IndustryService.ProcessPurchases(
            currentStates,
            foundation.Sects,
            [
                new SectPurchaseRequest(
                    SectId: "sect.verdant-crucible-sect",
                    TownId: "town.azure-ford",
                    Industry: MortalIndustryType.Commerce,
                    Quantity: 20000,
                    UnitPrice: 0.1m)
            ]);

        var azureFord = Assert.Single(result.NextStates, state => state.TownId == "town.azure-ford");
        var receipt = Assert.Single(result.Report.Receipts);
        var settlement = Assert.Single(result.Settlements, item => item.SectId == "sect.verdant-crucible-sect");

        Assert.Equal(0, azureFord.PurchasableSurplus.CommerceUnits);
        Assert.Equal(19041, receipt.PurchasedQuantity);
        Assert.Equal(1904.1m, receipt.FundsSpent);
        Assert.Equal(4225.9m, receipt.FundsRemaining);
        Assert.Equal("Partially filled", receipt.Resolution);
        Assert.Equal(1904.1m, settlement.FundsSpent);
        Assert.Equal(4225.9m, settlement.FundsRemaining);
    }

    [Fact]
    public void ProcessPurchases_applies_prior_spend_when_same_sect_submits_multiple_requests()
    {
        var foundation = FoundationService.GetInitialState();
        var currentStates = IndustryService.Initialize(foundation, foundation.Ministries);

        var result = IndustryService.ProcessPurchases(
            currentStates,
            foundation.Sects,
            [
                new SectPurchaseRequest(
                    SectId: "sect.azure-talisman-academy",
                    TownId: "town.azure-ford",
                    Industry: MortalIndustryType.Commerce,
                    Quantity: 1000,
                    UnitPrice: 2m),
                new SectPurchaseRequest(
                    SectId: "sect.azure-talisman-academy",
                    TownId: "town.azure-ford",
                    Industry: MortalIndustryType.Handicrafts,
                    Quantity: 1000,
                    UnitPrice: 5m)
            ]);

        Assert.Collection(
            result.Report.Receipts,
            first =>
            {
                Assert.Equal(1000, first.PurchasedQuantity);
                Assert.Equal(2000m, first.FundsSpent);
                Assert.Equal(3200m, first.FundsRemaining);
            },
            second =>
            {
                Assert.Equal(640, second.PurchasedQuantity);
                Assert.Equal(3200m, second.FundsSpent);
                Assert.Equal(0m, second.FundsRemaining);
                Assert.Equal("Partially filled", second.Resolution);
            });

        var settlement = Assert.Single(result.Settlements, item => item.SectId == "sect.azure-talisman-academy");
        Assert.Equal(5200m, settlement.FundsSpent);
        Assert.Equal(0m, settlement.FundsRemaining);
    }
}
