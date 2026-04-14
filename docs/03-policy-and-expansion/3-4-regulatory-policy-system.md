# Task 3.4 - Regulatory Policy System

## Objective
Introduce **Regulatory Policy System** so national governance decisions create realm-wide consequences that must be managed over time.

## Feature Increment
After this task, **Regulatory Policy System** should materially affect how the realm is governed, defended, or expanded.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Increase oversight: government corruption rate drops by 50 percent, but the policy is expensive.
- Restrict private armies: sects cannot build defensive structures.
- Crack down on tax evasion: sects cannot evade taxes, but loyalty drops by 10.
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
- Build on Task 3.3 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Regulatory Policy System** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The new governance layer creates clear pressure, trade-offs, or escalation paths at the realm level.
