# Task 1.8 - Foundational Building System

## Objective
Introduce **Foundational Building System** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Foundational Building System** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Sect buildings include gate halls, disciple quarters, warehouses, spirit gathering arrays, and alchemy rooms.
- Mortal buildings include farms, workshops, and markets.
- Buildings have construction cost, output, and upkeep cost.
- Sects can construct sect buildings.
- The mortal administration can construct mortal buildings.

## Scope Notes
- It is acceptable for this task to provide a narrow construction entry point that triggers foundational building construction without introducing the full player action framework planned for Task 1.12.
- Task 1.9 owns automatic sect construction, priority logic, quantity caps, and build time. Task 1.8 should stop at foundational definitions, runtime inventories, construction transactions, and turn-by-turn building effects.
- Building output may be applied as deterministic bonuses to existing sect or mortal production systems as long as those effects are visible in reports or panels.
- Construction and upkeep costs should have real financial consequences for sect funds or the treasury in this task.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Build screens around projections or view models that consume application services, not raw simulation internals.
- Keep navigation, filtering, and summary widgets aligned with player decisions so the UI explains cause and effect instead of only listing values.
- Treat readability, information hierarchy, and error states as part of the feature definition, not a later polish pass.

## Dependencies
- Build on Task 1.7 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Foundational Building System** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
