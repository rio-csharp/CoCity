namespace CoCity.Foundation.Services
{
    public interface IPlayerActionService
    {
        RealmMinistryState CycleMinistryAuthority(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            string ministryId);

        RealmMinistryState CycleMinistryStandard(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            string ministryId);

        PlayerActionResolutionResult ApproveEscalation(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            string ministryId,
            string caseId);

        PlayerActionResolutionResult RejectEscalation(
            RealmState foundation,
            RealmMinistryState ministryState,
            MortalRealmState mortalRealmState,
            RealmBuildingState buildingState,
            RealmTaxationState taxationState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            string ministryId,
            string caseId);
    }

    public readonly record struct PlayerActionResolutionResult(
        RealmMinistryState NextMinistryState,
        MortalRealmState NextRealmState,
        RealmBuildingState NextBuildingState,
        RealmTaxationState NextTaxationState,
        string Summary);
}
