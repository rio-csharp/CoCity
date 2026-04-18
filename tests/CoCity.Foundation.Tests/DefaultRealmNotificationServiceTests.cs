using System.Collections.Immutable;
using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultRealmNotificationServiceTests
{
    [Fact]
    public void Build_returns_active_alerts_for_expansion_requests_stability_pressure_and_low_treasury()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var realmService = new DefaultMortalRealmSimulationService();
        var industryService = new DefaultMortalIndustrySimulationService();
        var buildingService = new DefaultBuildingSystemService();
        var taxationService = new DefaultMortalTaxationSimulationService();
        var ministryService = new DefaultMinistryFrameworkService();
        var notificationService = new DefaultRealmNotificationService();

        var realmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, realmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, realmState, buildingState, taxationState);

        var personnelMinistry = ministryState.Ministries.Single(ministry => ministry.MinistryId == "ministry.personnel");
        var escalatedMinistryState = ministryState with
        {
            Ministries = ministryState.Ministries
                .Select(ministry => ministry.MinistryId == personnelMinistry.MinistryId
                    ? personnelMinistry with
                    {
                        PendingEscalations = personnelMinistry.PendingEscalations
                            .Concat([
                                new MinistryEscalationState(
                                    CaseId: "sect.azure-talisman-academy:expansion:SpiritGatheringArray",
                                    CaseType: MinistryCaseType.SectApplication,
                                    SubjectId: "sect.azure-talisman-academy",
                                    SubjectName: "Azure Talisman Academy",
                                    Reason: "Ministry of Personnel requires a ruling on a new cultivation facility.")
                            ])
                            .ToImmutableArray()
                    }
                    : ministry)
                .ToImmutableArray()
        };
        var pressuredTaxationState = taxationState with
        {
            CurrentTreasuryFunds = 20000m,
            Towns = taxationState.Towns
                .Select(town => town.TownId == "town.azure-ford"
                    ? town with
                    {
                        StabilityDelta = -2,
                        StabilitySummary = "Tax pressure is provoking local unrest."
                    }
                    : town)
                .ToImmutableArray()
        };

        var result = notificationService.Build(new RealmNotificationContext(
            Foundation: foundation,
            CurrentRealmState: realmState,
            TaxationState: pressuredTaxationState,
            MinistryState: escalatedMinistryState,
            TurnReport: null));

        Assert.Contains(result.Alerts, item => item.Category == RealmNotificationCategory.ExpansionRequest);
        Assert.Contains(result.Alerts, item => item.Category == RealmNotificationCategory.Stability);
        Assert.Contains(result.Alerts, item => item.Category == RealmNotificationCategory.Treasury);
        Assert.Equal("Alerts: 3 | Recent events: 0", result.Summary);
    }

    [Fact]
    public void Build_returns_recent_events_for_population_loyalty_and_ministry_updates()
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
        var notificationService = new DefaultRealmNotificationService();

        var previousRealmState = realmService.Initialize(foundation);
        var buildingState = buildingService.Initialize(foundation);
        var industryStates = industryService.Initialize(foundation, foundation.Ministries);
        var taxationState = taxationService.Initialize(foundation, previousRealmState, industryStates);
        var ministryState = ministryService.Initialize(foundation, previousRealmState, buildingState, taxationState);
        var pipelineResult = pipelineService.Advance(
            foundation,
            new TurnAdvancementState(
                previousRealmState,
                buildingState,
                industryStates,
                taxationState,
                ministryState));

        var currentRealmState = pipelineResult.NextState.RealmState with
        {
            Towns = pipelineResult.NextState.RealmState.Towns
                .Select(town => town.TownId == "town.azure-ford"
                    ? town with { CurrentPopulation = town.CurrentPopulation - 120 }
                    : town)
                .ToImmutableArray(),
            Sects = pipelineResult.NextState.RealmState.Sects
                .Select(sect => sect.SectId == "sect.azure-talisman-academy"
                    ? sect with { Loyalty = sect.Loyalty - 4 }
                    : sect)
                .ToImmutableArray()
        };

        var result = notificationService.Build(new RealmNotificationContext(
            Foundation: foundation,
            CurrentRealmState: currentRealmState,
            TaxationState: pipelineResult.NextState.TaxationState,
            MinistryState: pipelineResult.NextState.MinistryState,
            TurnReport: pipelineResult.Report,
            PreviousRealmState: previousRealmState));

        Assert.Contains(result.RecentEvents, item => item.Category == RealmNotificationCategory.Population);
        Assert.Contains(result.RecentEvents, item => item.Category == RealmNotificationCategory.Loyalty);
        Assert.Contains(result.RecentEvents, item => item.Category == RealmNotificationCategory.Ministry);
    }
}
