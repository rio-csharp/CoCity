# Task 3.18 - Expanded Ministry Responsibilities

## Objective
Introduce **Expanded Ministry Responsibilities** so national governance decisions create realm-wide consequences that must be managed over time.

## Feature Increment
After this task, **Expanded Ministry Responsibilities** should materially affect how the realm is governed, defended, or expanded.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The Ministry of Personnel can mediate sect conflicts.
- The Ministry of Revenue can manage spiritual vein investment.
- The Ministry of Rites can manage sect diplomacy.
- The Ministry of War can manage military mobilization.
- The Ministry of Works can manage defensive construction.

## Code Design
- Introduce a clean policy-effect layer above the economic simulation so national decisions can be timed, revoked, and traced.
- Represent crises, conflict, defense, and rebellion as first-class simulation systems instead of special cases.
- Keep regional data and national data synchronized through a predictable turn pipeline with clear escalation rules.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 3.17 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Expanded Ministry Responsibilities** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The new governance layer creates clear pressure, trade-offs, or escalation paths at the realm level.
