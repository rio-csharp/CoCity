# Task 1.2 - Mortal Realm Baseline Simulation

## Objective
Introduce **Mortal Realm Baseline Simulation** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Mortal Realm Baseline Simulation** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Each region contains mortal settlements. ✅ Already modeled in 1.1 via `TownIds` on `RegionState`.
- Each mortal settlement has a tracked population. ✅ Already modeled in 1.1 via `Population` on `MortalTownState`.
- Mortal population consumes basic living resources. **New — add per-cycle consumption to seed data and simulation logic.**
- Mortal population can grow or decline naturally. **New — add natural growth/decline rules based on consumption satisfaction.**
- Mortal population can be recruited by sects. **New — add recruitment interface and basic flow.**

## Scope Boundaries
- This task establishes the **simulation behavior** for the mortal realm. It defines how population changes each turn and how sects can recruit from it, but does not yet implement industry processing (Task 1.3) or taxation (Task 1.4).
- Deliver a **mutable simulation state** that can be advanced turn-by-turn, separate from the static foundation snapshot.
- Do **not** implement player policy input, ministry automation, building construction, market behavior, or event processing in this task.
- The simulation must be **deterministic**: given the same inputs, it produces the same outputs.

## Data Inventory
The implementation must extend the existing model with these additions:

### New Fields on `MortalTownState`
- `FoodConsumptionPerCapita` — basic living resource consumption rate per person per cycle.
- `FoodProduction` — current cycle food output from local industry.
- `RecruitmentPool` — number of mortals available for sect recruitment (derived from population after natural change).

### New Simulation State (`MortalRealmState`)
- Holds the mutable, turn-by-turn simulation state for all mortal towns.
- Tracks per-town: current population, food balance, recruitment pool, last turn's change summary.
- Reports side effects (growth, decline, recruitment events) for UI presentation.

### New Sect State Extension
- `RecruitablesFromRegion` — tracks how many mortals each sect can recruit from each region.

## Implementation Notes
- Use a **separate simulation state object** that wraps the foundation data, so the foundation snapshot remains immutable and reusable.
- Keep the simulation **step function** pure and deterministic: `MortalRealmState × TurnInput → MortalRealmState × TurnReport`.
- The turn report should enumerate every town that grew, declined, or lost recruits so the UI can surface these as readable events.
- Add the simulation service to DI alongside the foundation service.

## Proposed Code Structure
- **Domain model layer**: `MortalRealmState`, `MortalTownSimulationState`, `TurnReport`, `TurnInput` records.
- **Simulation service**: `IMortalRealmSimulationService` interface and `DefaultMortalRealmSimulationService` implementation with the step function.
- **Foundation extension**: `MortalTownState` gains `FoodConsumptionPerCapita` and `FoodProduction` fields (added to seed data with realistic values).
- **ViewModel layer**: `MainPageViewModel` gains a "Advance Turn" command that invokes the simulation and updates display cards.
- **App composition**: register the simulation service in DI and extend `MainPageViewModel` to hold simulation state.

## Non-Goals
- No player policy input affecting population.
- No sect autonomous behavior beyond basic recruitment.
- No save/load workflow.
- No UI beyond showing current simulation state and a turn-advance button.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 1.1 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Mortal Realm Baseline Simulation** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
- The UI shows population, food balance, and recruitment state for each town.
- The player can advance a turn and see population grow/decline based on food balance, and see sect recruitment affect available pools.
