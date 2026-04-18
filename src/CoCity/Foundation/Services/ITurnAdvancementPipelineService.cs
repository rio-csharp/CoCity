namespace CoCity.Foundation.Services
{
    public interface ITurnAdvancementPipelineService
    {
        TurnAdvancementResult Advance(
            RealmState foundation,
            TurnAdvancementState currentState);
    }

    public sealed record TurnAdvancementState(
        MortalRealmState RealmState,
        RealmBuildingState BuildingState,
        IReadOnlyList<MortalTownIndustryState> IndustryStates,
        RealmTaxationState TaxationState,
        RealmMinistryState MinistryState);

    public sealed record TurnAdvancementReport(
        TurnReport RealmReport,
        IndustryTurnReport IndustryReport,
        SectOperationsTurnReport SectOperationsReport,
        BuildingReport BuildingReport,
        TaxationTurnReport TaxationReport,
        MinistryTurnReport MinistryReport);

    public sealed record TurnAdvancementResult(
        TurnAdvancementState NextState,
        TurnAdvancementReport Report);
}
