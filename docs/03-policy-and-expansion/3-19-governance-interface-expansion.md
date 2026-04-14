# Task 3.19 - Governance Interface Expansion

## Objective
Introduce **Governance Interface Expansion** so national governance decisions create realm-wide consequences that must be managed over time.

## Feature Increment
After this task, **Governance Interface Expansion** should materially affect how the realm is governed, defended, or expanded.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Add a national policy panel showing active policies and effects.
- Add a regional governance panel showing each region and its development state.
- Add a sect relationship view showing connections between sects.
- Add a threat warning panel showing current threats and defensive readiness.
- Add a military panel showing armed strength and mobilization state.

## Code Design
- Introduce a clean policy-effect layer above the economic simulation so national decisions can be timed, revoked, and traced.
- Represent crises, conflict, defense, and rebellion as first-class simulation systems instead of special cases.
- Keep regional data and national data synchronized through a predictable turn pipeline with clear escalation rules.
- Build screens around projections or view models that consume application services, not raw simulation internals.
- Keep navigation, filtering, and summary widgets aligned with player decisions so the UI explains cause and effect instead of only listing values.
- Treat readability, information hierarchy, and error states as part of the feature definition, not a later polish pass.

## Dependencies
- Build on Task 3.18 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Governance Interface Expansion** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The new governance layer creates clear pressure, trade-offs, or escalation paths at the realm level.
