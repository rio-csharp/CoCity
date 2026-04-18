using CoCity.Foundation.Services;

namespace CoCity.Foundation.Tests;

public sealed class DefaultMortalRealmSimulationServiceSectStateTests
{
    [Fact]
    public void Initialize_seeds_runtime_sect_state_with_loyalty_and_empty_industry_preference()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var simulationService = new DefaultMortalRealmSimulationService();

        var initialState = simulationService.Initialize(foundation);

        var foundationSect = Assert.Single(foundation.Sects, sect => sect.Id == "sect.azure-talisman-academy");
        var simulatedSect = Assert.Single(initialState.Sects, sect => sect.SectId == foundationSect.Id);

        Assert.Equal(foundationSect.RegionId, simulatedSect.RegionId);
        Assert.Equal(foundationSect.Funds, simulatedSect.CurrentFunds);
        Assert.Equal(foundationSect.Population, simulatedSect.CurrentPopulation);
        Assert.Equal(foundationSect.Loyalty, simulatedSect.Loyalty);
        Assert.Null(simulatedSect.IndustryPreference);
        Assert.Equal(foundationSect.RecruitmentWage, simulatedSect.RecruitmentWage);
        Assert.Equal(foundationSect.Output, simulatedSect.CurrentOutput);
        Assert.True(simulatedSect.RecruitablesFromRegion > 0);
        Assert.Equal(0, simulatedSect.LastRecruitsGained);
        Assert.Equal(0m, simulatedSect.LastWagesPaid);
    }

    [Fact]
    public void Initialize_orders_runtime_sects_by_region_then_id()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var simulationService = new DefaultMortalRealmSimulationService();

        var initialState = simulationService.Initialize(foundation);

        Assert.Equal(
            [
                "sect.azure-talisman-academy",
                "sect.verdant-crucible-sect",
                "sect.iron-peak-hall"
            ],
            initialState.Sects.Select(sect => sect.SectId).ToArray());
    }

    [Fact]
    public void Step_applies_recruitment_results_to_runtime_sect_population_and_wage_costs()
    {
        var foundation = new SeedCoreDataFoundationService().GetInitialState();
        var simulationService = new DefaultMortalRealmSimulationService();
        var initialState = simulationService.Initialize(foundation);

        var result = simulationService.Step(foundation, initialState);

        var recruitmentEvent = Assert.Single(result.Report.RecruitmentEvents, item => item.SectId == "sect.azure-talisman-academy");
        var initialSect = Assert.Single(initialState.Sects, sect => sect.SectId == recruitmentEvent.SectId);
        var nextSect = Assert.Single(result.NextState.Sects, sect => sect.SectId == recruitmentEvent.SectId);

        Assert.Equal(initialSect.CurrentPopulation + recruitmentEvent.RecruitsGathered, nextSect.CurrentPopulation);
        Assert.Equal(initialSect.CurrentFunds - recruitmentEvent.WagesPaid, nextSect.CurrentFunds);
        Assert.Equal(initialSect.Loyalty, nextSect.Loyalty);
        Assert.Equal(initialSect.IndustryPreference, nextSect.IndustryPreference);
        Assert.Equal(initialSect.RecruitmentWage, nextSect.RecruitmentWage);
        Assert.Equal(initialSect.CurrentOutput, nextSect.CurrentOutput);
        Assert.Equal(recruitmentEvent.RecruitsGathered, nextSect.LastRecruitsGained);
        Assert.Equal(recruitmentEvent.WagesPaid, nextSect.LastWagesPaid);
    }
}
