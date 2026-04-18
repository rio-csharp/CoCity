namespace CoCity.Foundation.Services
{
    public interface ISectAutonomousOperationsService
    {
        SectOperationsTurnResult Step(
            RealmState foundation,
            MortalRealmState realmState,
            IReadOnlyList<MortalTownIndustryState> currentIndustryStates);
    }

    public readonly record struct SectOperationsTurnResult(
        IReadOnlyList<SectSimulationState> NextSects,
        IReadOnlyList<MortalTownIndustryState> NextIndustryStates,
        SectOperationsTurnReport Report);
}
