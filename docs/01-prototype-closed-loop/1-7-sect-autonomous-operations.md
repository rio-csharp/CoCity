# Task 1.7 - Sect Autonomous Operations

## Objective
Introduce **Sect Autonomous Operations** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Sect Autonomous Operations** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Sects consume resources each turn to sustain operations.
- Sects produce baseline resources each turn.
- Sects must purchase raw materials from the mortal realm.
- If a sect runs short of funds, its output declines.

## Scope Notes
- Task 1.3 already introduced a request-driven sect purchase interface, and Task 1.6 established sect runtime funds. This task should wire those foundations into an autonomous sect operations step rather than replacing them with a new purchasing system.
- It is acceptable to define a narrow seeded operations profile per sect for upkeep cost, required raw material type, and required raw material quantity, as long as these rules stay deterministic and visible in reports or dashboards.
- Baseline sect output may continue to come from seeded sect definitions, but the runtime sect output shown to the player should now reflect whether upkeep and raw-material requirements were met this turn.
- Do not implement building construction, expansion logic, ministry-driven optimization, or the broader building content planned for Task 1.8.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 1.6 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Sect Autonomous Operations** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
