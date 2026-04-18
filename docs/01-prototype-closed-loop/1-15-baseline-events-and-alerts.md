# Task 1.15 - Baseline Events and Alerts

## Objective
Introduce **Baseline Events and Alerts** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Baseline Events and Alerts** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Handle sect expansion requests through the Ministry of Personnel.
- Show warnings when mortal population declines.
- Show warnings when mortal stability falls.
- Show warnings when the national treasury runs low.
- Show warnings when sect loyalty declines.

## Scope Notes
- Reuse the current turn pipeline, ministry state, taxation state, and sect runtime state to shape alerts instead of creating a second simulation path.
- Introduce a unified notification model for active alerts and recent events so the overview can explain warnings consistently.
- Keep the work focused on baseline warnings and recent-history surfacing; broader event chains and advanced incident systems stay out of scope.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Represent triggers, preconditions, payloads, and consequences as explicit event objects so chains can be inspected and extended safely.
- Decide which events are deterministic outcomes versus probability-driven incidents before wiring them into the turn loop.
- Route alerts and event history through one notification pipeline so the player receives consistent explanations across the game.

## Dependencies
- Build on Task 1.14 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Baseline Events and Alerts** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
