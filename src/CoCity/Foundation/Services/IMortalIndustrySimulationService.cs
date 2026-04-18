namespace CoCity.Foundation.Services
{
    /// <summary>
    /// Calculates mortal industry output based on labor force, government efficiency,
    /// and base output rates per worker.
    /// </summary>
    public interface IMortalIndustrySimulationService
    {
        /// <summary>Initializes industry simulation state for all towns at turn 0.</summary>
        IReadOnlyList<MortalTownIndustryState> Initialize(
            RealmState foundation,
            IReadOnlyList<MinistryState> ministries);

        /// <summary>
        /// Advances the industry simulation by one turn.
        /// Returns the next industry states and a report of industry events.
        /// Uses current population from mortalRealmState to calculate labor force.
        /// </summary>
        IndustryTurnResult Step(
            RealmState foundation,
            IReadOnlyList<MinistryState> ministries,
            MortalRealmState mortalRealmState,
            IReadOnlyList<MortalTownIndustryState> currentStates);

        /// <summary>
        /// Applies explicit sect purchase requests against currently purchasable industrial surplus.
        /// This is intentionally request-driven and does not introduce autonomous sect behavior.
        /// The purchase report exposes financial consequences for the caller to apply to sect state.
        /// </summary>
        IndustryPurchaseResult ProcessPurchases(
            IReadOnlyList<MortalTownIndustryState> currentStates,
            IReadOnlyList<SectState> sects,
            IReadOnlyList<SectPurchaseRequest> requests);
    }

    public readonly record struct IndustryTurnResult(
        IReadOnlyList<MortalTownIndustryState> NextStates,
        IndustryTurnReport Report);

    public readonly record struct IndustryPurchaseResult(
        IReadOnlyList<MortalTownIndustryState> NextStates,
        IReadOnlyList<SectPurchaseSettlement> Settlements,
        SectPurchaseReport Report);
}
