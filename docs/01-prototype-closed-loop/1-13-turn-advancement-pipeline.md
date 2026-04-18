# Task 1.13 - Turn Advancement Pipeline

## Objective
Introduce **Turn Advancement Pipeline** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Turn Advancement Pipeline** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The game advances in discrete turns.
- Each turn resolves mortal production, sect operations, ministry processing, and tax settlement in order.
- Each turn produces ministry reports.
- The player can review what changed each turn.

## Scope Notes
- Extract the existing prototype turn logic into an explicit pipeline service instead of changing gameplay balance or introducing a second orchestration path.
- It is acceptable for the pipeline to preserve the current prototype ordering of realm simulation, industry, sect operations, buildings, taxation, and ministry processing, as long as that order becomes explicit and reportable.
- Keep manual player actions and non-turn policy changes outside this pipeline service; it should own turn advancement only.
- Surface a consolidated turn result/report object that later UI and alert tasks can consume without duplicating orchestration logic.
- The app now uses the pipeline service as the single orchestration path for `Advance Turn`, while keeping construction, tax tuning, and ministry player actions on their existing non-turn paths.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 1.12 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Turn Advancement Pipeline** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
