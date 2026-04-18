namespace CoCity.Foundation.Services
{
    public interface IBuildingSystemService
    {
        RealmBuildingState Initialize(RealmState foundation);

        BuildingConstructionResult ConstructNextSectBuildings(
            RealmBuildingState currentState,
            IReadOnlyList<SectSimulationState> sects,
            decimal currentTreasuryFunds);

        BuildingConstructionResult ConstructNextTownBuildings(
            RealmBuildingState currentState,
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyList<MortalTownSimulationState> towns,
            decimal currentTreasuryFunds);

        BuildingTurnResult ApplyTurn(
            RealmState foundation,
            RealmBuildingState currentState,
            IReadOnlyList<SectSimulationState> sects,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            decimal currentTreasuryFunds);
    }

    public readonly record struct BuildingConstructionResult(
        RealmBuildingState NextState,
        IReadOnlyList<SectSimulationState> NextSects,
        decimal NextTreasuryFunds,
        BuildingReport Report);

    public readonly record struct BuildingTurnResult(
        RealmBuildingState NextState,
        IReadOnlyList<SectSimulationState> NextSects,
        IReadOnlyList<MortalTownIndustryState> NextIndustryStates,
        decimal NextTreasuryFunds,
        BuildingReport Report);
}
