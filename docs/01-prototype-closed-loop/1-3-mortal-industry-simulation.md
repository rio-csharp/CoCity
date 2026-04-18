# Task 1.3 - Mortal Industry Simulation

## Objective
Introduce **Mortal Industry Simulation** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Mortal Industry Simulation** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The mortal realm supports three industry types: agriculture, handicrafts, and commerce. ✅ Already modeled in 1.1 via `MortalIndustryType` enum and `IndustryAllocation` on `MortalTownState`.
- Each industry type produces outputs such as food, tools, or trade profit. **New — add per-industry base output rates and derived production calculation.**
- Industrial output requires labor population. **New — add labor force calculation linking population to industry workforce allocation.**
- Industrial output is affected by local government efficiency. **New — add government efficiency modifier sourced from the governing ministry's ratings.**
- Sects can purchase mortal industrial output. **New — add sect purchase interface and basic purchase flow.**

## Scope Boundaries
- This task establishes the **industry production simulation** for the mortal realm. It defines how labor population drives industry output and how government efficiency modulates that output, but does not yet implement taxation (Task 1.4) or sect autonomous purchasing decisions (Task 1.7).
- Deliver **mutable industry simulation state** that extends the mortal realm simulation, separate from the static foundation snapshot.
- Do **not** implement player policy input, sect wage-based recruitment, building construction effects, market pricing fluctuations, or event processing in this task.
- The simulation must be **deterministic**: given the same inputs, it produces the same outputs.

## Data Inventory
The implementation must extend the existing model with these additions:

### New Fields on `MortalTownState`
- `BaseOutputPerWorker` — baseline output per laborer for each industry type. Stored as a dictionary or separate fields per industry.

### New Simulation State (`MortalTownIndustryState`)
- Holds the mutable, turn-by-turn industry simulation state for all mortal towns.
- Tracks per-town: labor force by industry, gross output by industry, government efficiency modifier, net output after efficiency, purchasable surplus by industry.
- Reports side effects (output changes, surplus available for purchase) for UI presentation.

### Industry Output Model
Each industry type has:
- **Agriculture**: produces food units. Base rate ~8-12 food per agricultural worker.
- **Handicrafts**: produces tool/craft units. Base rate ~4-6 tools per handicraft worker.
- **Commerce**: produces trade profit (taels). Base rate ~3-5 taels per commerce worker.

### Government Efficiency Modifier
- Calculated from the Ministry of Revenue's handling standard and the minister's administration rating.
- Range: 0.7 (low efficiency) to 1.3 (high efficiency).
- Formula: `1.0 + (MinisterAdministrationRating - 75) / 250`, clamped to [0.7, 1.3].

### Sect Purchase Interface (`SectPurchaseRequest`)
- Sects can submit purchase requests for mortal industrial output.
- Purchase reduces town's purchasable surplus.
- Purchase amount is deducted from sect funds.

## Implementation Notes
- Use a **separate industry simulation state object** that wraps the foundation data, so the foundation snapshot remains immutable and reusable.
- Keep the industry simulation **step function** pure and deterministic: `TownInput × GovernmentEfficiency → IndustryOutput`.
- The turn report should enumerate output by industry type so the UI can surface these as readable production summaries.
- Add the industry simulation logic to the existing turn step alongside natural change and recruitment.
- Seed data should provide realistic base output rates per worker for each industry type in each town.
- The basic sect purchase flow may be implemented as a domain/service interface plus automated tests; do not add a dedicated player purchase UI in this task.

## Proposed Code Structure
- **Domain model layer**: `MortalTownIndustryState`, `IndustryOutput`, `SectPurchaseRequest`, `IndustryTurnReport` records.
- **Industry service**: `IMortalIndustrySimulationService` interface and `DefaultMortalIndustrySimulationService` implementation with the step function.
- **Foundation extension**: `MortalTownState` gains `BaseOutputPerWorkerByIndustry` or equivalent fields (added to seed data with realistic values).
- **Turn integration**: extend the existing `IMortalRealmSimulationService.Step()` to include industry calculation as a phase.
- **ViewModel layer**: `MainPageViewModel` extends to display industry output per town and purchasable surplus.
- **App composition**: register the industry service in DI alongside the foundation and simulation services.

## Non-Goals
- No player policy input affecting industry allocation.
- No sect autonomous purchasing decisions beyond basic interface.
- No market pricing or trade simulation.
- No save/load workflow.
- No UI beyond showing current industry state and output summaries.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Express modifiers, prices, and policy effects through composable rule evaluators so balancing can happen without rewriting feature logic.
- Ensure all economic consequences can be traced through reports, logs, or explanatory UI labels.
- Prefer configuration-driven thresholds and weights over hardcoded branch logic whenever the feature will need tuning later.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 1.2 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Mortal Industry Simulation** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
- The UI shows labor force by industry, gross output by industry, government efficiency modifier, and net output for each town.
- The player can see which industrial outputs are available for sect purchase.
