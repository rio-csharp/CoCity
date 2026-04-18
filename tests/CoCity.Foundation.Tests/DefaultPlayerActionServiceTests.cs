using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultPlayerActionServiceTests
{
    [Fact]
    public void CycleMinistryAuthority_rotates_to_next_authority_profile()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var playerActionService = new DefaultPlayerActionService(ministryService, taxationService);

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var result = playerActionService.CycleMinistryAuthority(
            foundation,
            ministryState,
            realmState,
            buildingState,
            taxationState,
            "ministry.personnel");

        var personnel = Assert.Single(result.Ministries, ministry => ministry.MinistryId == "ministry.personnel");
        Assert.Equal("Broad delegation", personnel.Authority.DelegationLevel);
    }

    [Fact]
    public void ApproveEscalation_marks_sensitive_expansion_as_approved()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var playerActionService = new DefaultPlayerActionService(ministryService, taxationService);

        var realmState = realmService.Initialize(foundation) with
        {
            Sects = realmService.Initialize(foundation).Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { CurrentPopulation = 205 }
                    : sect)
                .ToArray()
        };
        var baselineBuildingState = buildingService.Initialize(foundation);
        var buildingState = new RealmBuildingState(
            Sects: [
                new SectBuildingInventoryState(
                    "sect.azure-talisman-academy",
                    [],
                    new SectBuildingProject(SectBuildingType.SpiritGatheringArray, 2)),
                .. baselineBuildingState.Sects.Where(item => item.SectId != "sect.azure-talisman-academy")
            ],
            Towns: baselineBuildingState.Towns);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);
        var personnel = Assert.Single(ministryState.Ministries, ministry => ministry.MinistryId == "ministry.personnel");
        var escalation = Assert.Single(personnel.PendingEscalations);

        var result = playerActionService.ApproveEscalation(
            foundation,
            ministryState,
            realmState,
            buildingState,
            taxationState,
            industryStates,
            "ministry.personnel",
            escalation.CaseId);

        var updatedPersonnel = Assert.Single(result.NextMinistryState.Ministries, ministry => ministry.MinistryId == "ministry.personnel");
        Assert.Contains(updatedPersonnel.ApprovedCases, item => item.CaseId == escalation.CaseId);
        Assert.Empty(updatedPersonnel.PendingEscalations);
    }

    [Fact]
    public void RejectEscalation_cancels_sensitive_expansion_project()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var playerActionService = new DefaultPlayerActionService(ministryService, taxationService);

        var realmState = realmService.Initialize(foundation) with
        {
            Sects = realmService.Initialize(foundation).Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { CurrentPopulation = 205 }
                    : sect)
                .ToArray()
        };
        var baselineBuildingState = buildingService.Initialize(foundation);
        var buildingState = new RealmBuildingState(
            Sects: [
                new SectBuildingInventoryState(
                    "sect.azure-talisman-academy",
                    [],
                    new SectBuildingProject(SectBuildingType.SpiritGatheringArray, 2)),
                .. baselineBuildingState.Sects.Where(item => item.SectId != "sect.azure-talisman-academy")
            ],
            Towns: baselineBuildingState.Towns);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);
        var personnel = Assert.Single(ministryState.Ministries, ministry => ministry.MinistryId == "ministry.personnel");
        var escalation = Assert.Single(personnel.PendingEscalations);

        var result = playerActionService.RejectEscalation(
            foundation,
            ministryState,
            realmState,
            buildingState,
            taxationState,
            industryStates,
            "ministry.personnel",
            escalation.CaseId);

        var updatedInventory = Assert.Single(result.NextBuildingState.Sects, item => item.SectId == "sect.azure-talisman-academy");
        var updatedPersonnel = Assert.Single(result.NextMinistryState.Ministries, ministry => ministry.MinistryId == "ministry.personnel");
        Assert.Null(updatedInventory.ActiveProject);
        Assert.Empty(updatedPersonnel.PendingEscalations);
    }

    [Fact]
    public void Approved_expansion_decision_does_not_suppress_later_different_project()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var playerActionService = new DefaultPlayerActionService(ministryService, taxationService);

        var realmState = realmService.Initialize(foundation) with
        {
            Sects = realmService.Initialize(foundation).Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { CurrentPopulation = 205 }
                    : sect)
                .ToArray()
        };
        var baselineBuildingState = buildingService.Initialize(foundation);
        var firstBuildingState = new RealmBuildingState(
            Sects: [
                new SectBuildingInventoryState(
                    "sect.azure-talisman-academy",
                    [],
                    new SectBuildingProject(SectBuildingType.SpiritGatheringArray, 2)),
                .. baselineBuildingState.Sects.Where(item => item.SectId != "sect.azure-talisman-academy")
            ],
            Towns: baselineBuildingState.Towns);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, firstBuildingState, taxationState);
        var firstEscalation = Assert.Single(
            Assert.Single(ministryState.Ministries, ministry => ministry.MinistryId == "ministry.personnel").PendingEscalations);

        var approvedResult = playerActionService.ApproveEscalation(
            foundation,
            ministryState,
            realmState,
            firstBuildingState,
            taxationState,
            industryStates,
            "ministry.personnel",
            firstEscalation.CaseId);

        var secondBuildingState = new RealmBuildingState(
            Sects: [
                new SectBuildingInventoryState(
                    "sect.azure-talisman-academy",
                    [],
                    new SectBuildingProject(SectBuildingType.AlchemyRoom, 2)),
                .. baselineBuildingState.Sects.Where(item => item.SectId != "sect.azure-talisman-academy")
            ],
            Towns: baselineBuildingState.Towns);

        var recalculated = ministryService.Recalculate(
            foundation,
            approvedResult.NextMinistryState,
            realmState,
            secondBuildingState,
            taxationState);
        var personnel = Assert.Single(recalculated.Ministries, ministry => ministry.MinistryId == "ministry.personnel");

        Assert.Contains(personnel.PendingEscalations, item => item.CaseId.EndsWith(SectBuildingType.AlchemyRoom.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public void RejectEscalation_lowers_tax_rate_for_revenue_case()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var playerActionService = new DefaultPlayerActionService(ministryService, taxationService);

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.SetTaxRate(
            taxationService.Initialize(foundation, realmState, industryStates),
            realmState,
            industryStates,
            TaxRateLevel.Heavy);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);
        var revenue = Assert.Single(ministryState.Ministries, ministry => ministry.MinistryId == "ministry.revenue");
        var escalation = revenue.PendingEscalations[0];

        var result = playerActionService.RejectEscalation(
            foundation,
            ministryState,
            realmState,
            buildingState,
            taxationState,
            industryStates,
            "ministry.revenue",
            escalation.CaseId);

        var updatedRevenue = Assert.Single(result.NextMinistryState.Ministries, ministry => ministry.MinistryId == "ministry.revenue");
        Assert.Equal(TaxRateLevel.Standard, result.NextTaxationState.SelectedTaxRate);
        Assert.True(updatedRevenue.PendingEscalations.Count < revenue.PendingEscalations.Count);
    }
}
