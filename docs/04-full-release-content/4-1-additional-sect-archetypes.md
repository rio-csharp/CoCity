# Task 4.1 - Additional Sect Archetypes

## Objective
Introduce **Additional Sect Archetypes** so the project evolves from a complete simulation into a polished, replayable, release-ready strategy game.

## Feature Increment
After this task, **Additional Sect Archetypes** should improve depth, readability, replay value, or release completeness in a player-visible way.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Add body cultivation sects that produce body cultivator disciples and require pills and cultivation resources.
- Add beast-taming sects that produce beast-handling disciples and require spirit beasts and feed.
- Add mining sects that produce ore and require tools and labor.
- Add commercial sects that produce trade profit and require capital.
- Add special side-path sects such as heretical or hidden sects with unique mechanics.

## Code Design
- Preserve the earlier simulation contracts while layering content, UX, onboarding, and release-readiness around them.
- Use data-driven content definitions where possible so sect types, events, buildings, and achievements scale without hardcoding.
- Treat performance, saves, presentation, and release operations as production features, not late afterthoughts.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Depends on the Phase 3 acceptance state: national policy, regional pressure, crises, military response, and expansion all functioning together.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Additional Sect Archetypes** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The feature improves content completeness, onboarding, replayability, or ship-readiness in a measurable way.
