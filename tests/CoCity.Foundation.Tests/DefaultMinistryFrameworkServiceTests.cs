using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultMinistryFrameworkServiceTests
{
    [Fact]
    public void Initialize_builds_runtime_ministry_state_with_seeded_staff_and_cases()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);

        var result = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var personnel = Assert.Single(result.Ministries, ministry => ministry.MinistryId == "ministry.personnel");
        var revenue = Assert.Single(result.Ministries, ministry => ministry.MinistryId == "ministry.revenue");
        var rites = Assert.Single(result.Ministries, ministry => ministry.MinistryId == "ministry.rites");

        Assert.Equal(3, result.Ministries.Count);
        Assert.True(personnel.HandlingCapacity > 0m);
        Assert.True(revenue.AutomationSuccessRate > 0m);
        Assert.Equal(foundation.Towns.Count, revenue.ActiveCaseCount);
        Assert.NotEmpty(rites.ActiveCases);
    }

    [Fact]
    public void Step_automates_routine_personnel_cases_without_escalation()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var sectConstruction = buildingService.ConstructNextSectBuildings(
            buildingState,
            realmState.Sects,
            currentTreasuryFunds: foundation.Treasury.Funds);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(
            foundation,
            realmState with { Sects = sectConstruction.NextSects },
            industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var result = ministryService.Step(
            foundation,
            ministryState,
            realmState with { Sects = sectConstruction.NextSects },
            sectConstruction.NextState,
            taxationState);

        var personnel = Assert.Single(result.NextState.Ministries, ministry => ministry.MinistryId == "ministry.personnel");

        Assert.NotEmpty(personnel.ActiveCases);
        Assert.True(personnel.ProcessedCaseCount > 0);
        Assert.Equal(0, personnel.EscalatedCaseCount);
        Assert.All(personnel.ActiveCases, item => Assert.Equal(MinistryCaseType.SectApplication, item.CaseType));
        Assert.Contains(personnel.ActiveCases, item => item.Summary.Contains("expansion", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Step_escalates_revenue_cases_when_harsh_tax_policy_is_active()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.SetTaxRate(
            taxationService.Initialize(foundation, realmState, industryStates),
            realmState,
            industryStates,
            TaxRateLevel.Heavy);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var result = ministryService.Step(foundation, ministryState, realmState, buildingState, taxationState);
        var revenue = Assert.Single(result.NextState.Ministries, ministry => ministry.MinistryId == "ministry.revenue");

        Assert.Equal(revenue.ActiveCaseCount, revenue.EscalatedCaseCount);
        Assert.Equal(0, revenue.ProcessedCaseCount);
        Assert.NotEmpty(revenue.PendingEscalations);
        Assert.Contains(result.Report.MinistryEvents, evt => evt.MinistryId == "ministry.revenue" && evt.EscalatedCases > 0);
    }

    [Fact]
    public void Step_escalates_rites_cases_when_loyalty_is_low()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var adjustedRealmState = realmState with
        {
            Sects = realmState.Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { Loyalty = 75 }
                    : sect)
                .ToArray()
        };
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, adjustedRealmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, adjustedRealmState, buildingState, taxationState);

        var result = ministryService.Step(foundation, ministryState, adjustedRealmState, buildingState, taxationState);
        var rites = Assert.Single(result.NextState.Ministries, ministry => ministry.MinistryId == "ministry.rites");

        Assert.Contains(rites.PendingEscalations, item => item.SubjectId == "sect.azure-talisman-academy");
    }

    [Fact]
    public void Step_escalates_personnel_cases_for_sensitive_expansion_projects()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var adjustedRealmState = realmState with
        {
            Sects = realmState.Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { CurrentPopulation = 205 }
                    : sect)
                .ToArray()
        };
        var buildingState = new RealmBuildingState(
            Sects: [
                new SectBuildingInventoryState(
                    "sect.azure-talisman-academy",
                    [],
                    new SectBuildingProject(SectBuildingType.SpiritGatheringArray, 2)),
                .. buildingService.Initialize(foundation).Sects.Where(item => item.SectId != "sect.azure-talisman-academy")
            ],
            Towns: buildingService.Initialize(foundation).Towns);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, adjustedRealmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, adjustedRealmState, buildingState, taxationState);

        var result = ministryService.Step(foundation, ministryState, adjustedRealmState, buildingState, taxationState);
        var personnel = Assert.Single(result.NextState.Ministries, ministry => ministry.MinistryId == "ministry.personnel");

        Assert.Contains(personnel.PendingEscalations, item => item.SubjectId == "sect.azure-talisman-academy");
    }

    [Fact]
    public void Step_automates_revenue_cases_under_standard_policy()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var result = ministryService.Step(foundation, ministryState, realmState, buildingState, taxationState);
        var revenue = Assert.Single(result.NextState.Ministries, ministry => ministry.MinistryId == "ministry.revenue");

        Assert.Equal(foundation.Towns.Count, revenue.ProcessedCaseCount);
        Assert.Equal(0, revenue.EscalatedCaseCount);
    }
}
