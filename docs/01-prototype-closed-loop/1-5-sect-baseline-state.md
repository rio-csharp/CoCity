# Task 1.5 - Sect Baseline State

## Objective
Introduce **Sect Baseline State** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Sect Baseline State** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Sects track funding, population, and output.
- Each sect is assigned to a region.
- Each sect has a loyalty value.
- Each sect has an industry preference field, initially empty.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Separate static content definitions from mutable campaign state so later saves, balancing passes, and content growth remain manageable.
- Use stable identifiers and explicit relationships instead of direct object graphs to avoid tight coupling between simulation, UI, and persistence.
- Document which fields are authoritative, derived, cached, or presentation-only before implementation begins.

## Dependencies
- Build on Task 1.4 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Sect Baseline State** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
