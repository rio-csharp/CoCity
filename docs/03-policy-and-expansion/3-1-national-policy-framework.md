# Task 3.1 - National Policy Framework

## Objective
Introduce **National Policy Framework** so national governance decisions create realm-wide consequences that must be managed over time.

## Feature Increment
After this task, **National Policy Framework** should materially affect how the realm is governed, defended, or expanded.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The player can issue nationwide policies.
- Policies have activation timing and duration.
- Policies have costs paid from the treasury.
- Policies have effects on sects and the mortal realm.
- Policies can be revoked.

## Code Design
- Introduce a clean policy-effect layer above the economic simulation so national decisions can be timed, revoked, and traced.
- Represent crises, conflict, defense, and rebellion as first-class simulation systems instead of special cases.
- Keep regional data and national data synchronized through a predictable turn pipeline with clear escalation rules.
- Express modifiers, prices, and policy effects through composable rule evaluators so balancing can happen without rewriting feature logic.
- Ensure all economic consequences can be traced through reports, logs, or explanatory UI labels.
- Prefer configuration-driven thresholds and weights over hardcoded branch logic whenever the feature will need tuning later.

## Dependencies
- Depends on the Phase 2 acceptance state: a living economic ecosystem with sect specialization, market feedback, and event-driven consequences.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **National Policy Framework** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The new governance layer creates clear pressure, trade-offs, or escalation paths at the realm level.
