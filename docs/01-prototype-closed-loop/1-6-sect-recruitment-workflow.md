# Task 1.6 - Sect Recruitment Workflow

## Objective
Introduce **Sect Recruitment Workflow** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Sect Recruitment Workflow** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Sects can recruit population from the mortal realm.
- Recruitment requires wage payments.
- Recruitment reduces mortal population.
- Sects offering higher wages recruit more effectively.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Build screens around projections or view models that consume application services, not raw simulation internals.
- Keep navigation, filtering, and summary widgets aligned with player decisions so the UI explains cause and effect instead of only listing values.
- Treat readability, information hierarchy, and error states as part of the feature definition, not a later polish pass.

## Dependencies
- Build on Task 1.5 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Sect Recruitment Workflow** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
