# Task 3.13 - Wartime Mobilization

## Objective
Introduce **Wartime Mobilization** so national governance decisions create realm-wide consequences that must be managed over time.

## Feature Increment
After this task, **Wartime Mobilization** should materially affect how the realm is governed, defended, or expanded.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The player can issue a wartime mobilization order.
- Mobilization increases taxes and resource requisitioning.
- Mobilization affects loyalty and stability.
- Mobilization has a time limit.
- Mobilization can be revoked.

## Code Design
- Introduce a clean policy-effect layer above the economic simulation so national decisions can be timed, revoked, and traced.
- Represent crises, conflict, defense, and rebellion as first-class simulation systems instead of special cases.
- Keep regional data and national data synchronized through a predictable turn pipeline with clear escalation rules.
- Keep buildable content, upgrade paths, upkeep costs, and unlock requirements in data definitions rather than embedding them inside UI flows.
- Model construction and deployment as time-based state transitions so the turn loop can display work-in-progress, interruptions, and completion.
- Expose infrastructure effects as modifiers on sectors, regions, or entities instead of bespoke one-off side effects.

## Dependencies
- Build on Task 3.12 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Wartime Mobilization** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The new governance layer creates clear pressure, trade-offs, or escalation paths at the realm level.
