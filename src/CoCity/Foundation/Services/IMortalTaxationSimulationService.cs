namespace CoCity.Foundation.Services
{
    public interface IMortalTaxationSimulationService
    {
        RealmTaxationState Initialize(
            RealmState foundation,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates);

        RealmTaxationState SetTaxRate(
            RealmTaxationState currentState,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates,
            TaxRateLevel taxRate);

        TaxationTurnResult Step(
            RealmTaxationState currentState,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> industryStates);
    }

    public readonly record struct TaxationTurnResult(
        RealmTaxationState NextState,
        TaxationTurnReport Report);
}
