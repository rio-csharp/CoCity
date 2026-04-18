using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class MinistryFrameworkPipelineIntegrationTests
{
    [Fact]
    public void Turn_pipeline_surfaces_ministry_reports_after_building_and_tax_updates()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var ministryService = new DefaultMinistryFrameworkService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var realmResult = realmService.Step(foundation, realmState, taxationState.SelectedTaxRate);
        var industryResult = industryService.Step(foundation, foundation.Ministries, realmResult.NextState, industryStates);
        var sectOpsResult = sectOperationsService.Step(foundation, realmResult.NextState, industryResult.NextStates);
        var buildingTurnResult = buildingService.ApplyTurn(
            foundation,
            buildingState,
            sectOpsResult.NextSects,
            sectOpsResult.NextIndustryStates,
            taxationState.CurrentTreasuryFunds);
        var taxationResult = taxationService.Step(
            taxationState with { CurrentTreasuryFunds = buildingTurnResult.NextTreasuryFunds },
            realmResult.NextState with { Sects = buildingTurnResult.NextSects },
            buildingTurnResult.NextIndustryStates);
        var ministryResult = ministryService.Step(
            foundation,
            ministryState,
            realmResult.NextState with { Sects = buildingTurnResult.NextSects },
            buildingTurnResult.NextState,
            taxationResult.NextState);

        Assert.NotEmpty(ministryResult.Report.MinistryEvents);
        Assert.Contains(ministryResult.Report.MinistryEvents, evt => evt.MinistryId == "ministry.personnel" && evt.ActiveCases > 0);
        Assert.Contains(ministryResult.Report.MinistryEvents, evt => evt.MinistryId == "ministry.revenue" && evt.ProcessedCases == foundation.Towns.Count);
    }
}
