namespace CoCity.Foundation.Services
{
    public interface IMinistryFrameworkService
    {
        RealmMinistryState Initialize(
            RealmState foundation,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState);

        RealmMinistryState Recalculate(
            RealmState foundation,
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState);

        MinistryFrameworkTurnResult Step(
            RealmState foundation,
            RealmMinistryState currentState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState);
    }

    public readonly record struct MinistryFrameworkTurnResult(
        RealmMinistryState NextState,
        MinistryTurnReport Report);
}
