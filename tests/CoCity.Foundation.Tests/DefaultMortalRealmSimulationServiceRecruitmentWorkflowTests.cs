using System.Collections.Immutable;
using CoCity.Foundation;
using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultMortalRealmSimulationServiceRecruitmentWorkflowTests
{
    [Fact]
    public void Higher_wage_recruits_more_effectively_when_other_inputs_match()
    {
        var foundation = CreateSameRegionRecruitmentFoundation();
        var simulationService = new DefaultMortalRealmSimulationService();
        var initialState = simulationService.Initialize(foundation);

        var result = simulationService.Step(foundation, initialState);

        var frugalEvent = Assert.Single(result.Report.RecruitmentEvents, item => item.SectId == "sect.azure-frugal");
        var generousEvent = Assert.Single(result.Report.RecruitmentEvents, item => item.SectId == "sect.azure-generous");

        Assert.True(generousEvent.RecruitsGathered > frugalEvent.RecruitsGathered);
        Assert.True(generousEvent.WagesPaid > frugalEvent.WagesPaid);
        Assert.True(generousEvent.FundsRemaining < frugalEvent.FundsRemaining);
    }

    [Fact]
    public void Recruitment_stops_when_sect_cannot_afford_current_wage()
    {
        var foundation = CreateSameRegionRecruitmentFoundation() with
        {
            Sects = ImmutableArray.Create(
                new SectState(
                    Id: "sect.azure-broke",
                    RegionId: "region.azure-river",
                    Name: "Azure Broke Sect",
                    Funds: 5,
                    Population: 100,
                    Loyalty: 70,
                    IndustryPreference: null,
                    RecruitmentWage: RecruitmentWageLevel.Generous,
                    Output: ImmutableArray.Create(new OutputMetric("Trial talismans", 12, "seals"))))
        };

        var simulationService = new DefaultMortalRealmSimulationService();
        var initialState = simulationService.Initialize(foundation);

        var result = simulationService.Step(foundation, initialState);

        var recruitmentEvent = Assert.Single(result.Report.RecruitmentEvents);
        var nextSect = Assert.Single(result.NextState.Sects);

        Assert.Equal(0, recruitmentEvent.RecruitsGathered);
        Assert.Equal(0m, recruitmentEvent.WagesPaid);
        Assert.Equal(initialState.Sects[0].CurrentFunds, recruitmentEvent.FundsRemaining);
        Assert.Equal(initialState.Sects[0].CurrentPopulation, nextSect.CurrentPopulation);
        Assert.Contains("too low to pay even one recruit", recruitmentEvent.OutcomeSummary);
    }

    private static RealmState CreateSameRegionRecruitmentFoundation()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var azureSect = Assert.Single(foundation.Sects, sect => sect.Id == "sect.azure-talisman-academy");

        return foundation with
        {
            Sects = ImmutableArray.Create(
                new SectState(
                    Id: "sect.azure-frugal",
                    RegionId: "region.azure-river",
                    Name: "Azure Frugal Sect",
                    Funds: 5000,
                    Population: 100,
                    Loyalty: 70,
                    IndustryPreference: null,
                    RecruitmentWage: RecruitmentWageLevel.Frugal,
                    Output: azureSect.Output.ToImmutableArray()),
                new SectState(
                    Id: "sect.azure-generous",
                    RegionId: "region.azure-river",
                    Name: "Azure Generous Sect",
                    Funds: 5000,
                    Population: 100,
                    Loyalty: 70,
                    IndustryPreference: null,
                    RecruitmentWage: RecruitmentWageLevel.Generous,
                    Output: azureSect.Output.ToImmutableArray())),
            Regions = foundation.Regions
                .Select(region => region.Id == "region.azure-river"
                    ? region with { SectIds = ImmutableArray.Create("sect.azure-frugal", "sect.azure-generous") }
                    : region)
                .ToImmutableArray()
        };
    }
}
