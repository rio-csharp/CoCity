# Task 1.14 - Baseline User Interface

## Objective
Introduce **Baseline User Interface** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Baseline User Interface** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Provide a national overview panel for sect lists, mortal status, the treasury, and ministry reports.
- Provide a sect detail panel for funds, population, buildings, and output.
- Provide a mortal realm panel for population, industries, output, and stability.
- Provide a ministry panel for officials, authority, and handling standards.
- Provide a turn-advance button and turn counter display.

## Scope Notes
- Reorganize the existing dashboard surfaces into clearer baseline panels instead of inventing a separate UI path or replacing the underlying simulation services.
- Keep the player-facing controls for taxation, construction, ministry focus, and escalation decisions visible in the baseline interface.
- Preserve current reports and event visibility, but defer dedicated alert curation and event-system consolidation to Task 1.15.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Build screens around projections or view models that consume application services, not raw simulation internals.
- Keep navigation, filtering, and summary widgets aligned with player decisions so the UI explains cause and effect instead of only listing values.
- Treat readability, information hierarchy, and error states as part of the feature definition, not a later polish pass.

## Dependencies
- Build on Task 1.13 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Baseline User Interface** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
