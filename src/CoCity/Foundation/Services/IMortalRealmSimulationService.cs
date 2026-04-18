using CoCity.Foundation;

namespace CoCity.Foundation.Services
{
    /// <summary>
    /// Encapsulates the mortal-realm simulation step function.
    /// Given a foundation state and current simulation state, produces the next
    /// simulation state and a human-readable turn report.
    /// </summary>
    public interface IMortalRealmSimulationService
    {
        /// <summary>Initializes simulation state from the foundation data at turn 0.</summary>
        MortalRealmState Initialize(RealmState foundation);

        /// <summary>Advances the simulation by one turn. Deterministic: same inputs → same outputs.</summary>
        TurnResult Step(RealmState foundation, MortalRealmState currentState, TaxRateLevel taxRate = TaxRateLevel.Standard);
    }

    /// <summary>Return value of Step(): next state + report of what happened.</summary>
    public sealed record TurnResult(
        MortalRealmState NextState,
        TurnReport Report);
}
