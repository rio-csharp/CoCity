# Task 3.3 - Industry Policy System

## Objective
Introduce **Industry Policy System** so national governance decisions create realm-wide consequences that must be managed over time.

## Feature Increment
After this task, **Industry Policy System** should materially affect how the realm is governed, defended, or expanded.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Encourage a sect category: that category gains 20 percent output and 10 percent lower cost.
- Restrict a sect category: that category suffers 20 percent lower output and 10 percent higher cost.
- Ban a sect category: that category cannot construct related buildings.
- Policies have a cost.
- Policies affect sect loyalty.

## Code Design
- Introduce a clean policy-effect layer above the economic simulation so national decisions can be timed, revoked, and traced.
- Represent crises, conflict, defense, and rebellion as first-class simulation systems instead of special cases.
- Keep regional data and national data synchronized through a predictable turn pipeline with clear escalation rules.
- Express modifiers, prices, and policy effects through composable rule evaluators so balancing can happen without rewriting feature logic.
- Ensure all economic consequences can be traced through reports, logs, or explanatory UI labels.
- Prefer configuration-driven thresholds and weights over hardcoded branch logic whenever the feature will need tuning later.

## Dependencies
- Build on Task 3.2 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Industry Policy System** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The new governance layer creates clear pressure, trade-offs, or escalation paths at the realm level.
