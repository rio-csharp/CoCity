using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class TurnAdvancementPipelineServiceTests
{
    [Fact]
    public void Advance_resolves_existing_turn_flow_and_returns_unified_report()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var pipelineService = new DefaultTurnAdvancementPipelineService(
            realmService,
            industryService,
            sectOperationsService,
            buildingService,
            taxationService,
            ministryService);

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var result = pipelineService.Advance(
            foundation,
            new TurnAdvancementState(
                realmState,
                buildingState,
                industryStates,
                taxationState,
                ministryState));

        Assert.Equal(1, result.NextState.RealmState.TurnNumber);
        Assert.NotEmpty(result.Report.RealmReport.TownEvents);
        Assert.NotEmpty(result.Report.IndustryReport.TownIndustryEvents);
        Assert.NotEmpty(result.Report.TaxationReport.TownEvents);
        Assert.NotEmpty(result.Report.MinistryReport.MinistryEvents);
    }

    [Fact]
    public void Advance_is_deterministic_for_same_input_state()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var sectOperationsService = new DefaultSectAutonomousOperationsService(industryService);
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var pipelineService = new DefaultTurnAdvancementPipelineService(
            realmService,
            industryService,
            sectOperationsService,
            buildingService,
            taxationService,
            ministryService);

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);
        var currentState = new TurnAdvancementState(
            realmState,
            buildingState,
            industryStates,
            taxationState,
            ministryState);

        var first = pipelineService.Advance(foundation, currentState);
        var second = pipelineService.Advance(foundation, currentState);

        Assert.Equivalent(first.NextState, second.NextState, strict: true);
        Assert.Equivalent(first.Report, second.Report, strict: true);
    }
}
