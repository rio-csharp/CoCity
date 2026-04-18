# Task 1.9 - Sect Auto-Construction Logic

## Objective
Introduce **Sect Auto-Construction Logic** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Sect Auto-Construction Logic** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Sects automatically construct buildings when they have sufficient funds.
- Sects prioritize foundational buildings.
- Buildings have quantity caps.
- Building construction takes time to complete.

## Scope Notes
- Build directly on the foundational building definitions and runtime inventories from Task 1.8 rather than replacing that system.
- It is acceptable for sect auto-construction to use a deterministic build priority order and fixed construction durations in this task, as long as progress and completions are visible to the player.
- Quantity caps only need to cover sect buildings in this task; mortal building expansion rules can remain manual until later work makes them relevant.
- Do not introduce ministry approval workflows, broader player action routing, or non-sect construction automation in this task.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 1.8 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Sect Auto-Construction Logic** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
