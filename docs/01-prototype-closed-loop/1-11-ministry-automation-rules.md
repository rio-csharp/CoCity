# Task 1.11 - Ministry Automation Rules

## Objective
Introduce **Ministry Automation Rules** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Ministry Automation Rules** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The Ministry of Personnel automatically handles sect expansion requests according to player-defined standards.
- The Ministry of Revenue automatically collects taxes.
- Department processing has a success rate.
- Failed cases or cases beyond ministry authority are escalated to the player.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 1.10 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Ministry Automation Rules** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
