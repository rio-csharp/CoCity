# Task 4.7 - Difficulty System

## Objective
Introduce **Difficulty System** so the project evolves from a complete simulation into a polished, replayable, release-ready strategy game.

## Feature Increment
After this task, **Difficulty System** should improve depth, readability, replay value, or release completeness in a player-visible way.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Easy difficulty provides abundant resources and weaker threats.
- Normal difficulty provides balanced resources and standard threats.
- Hard difficulty provides tight resources and stronger threats.
- Extreme difficulty provides very scarce resources and very strong threats.
- Difficulty affects resources, events, and threats.

## Code Design
- Preserve the earlier simulation contracts while layering content, UX, onboarding, and release-readiness around them.
- Use data-driven content definitions where possible so sect types, events, buildings, and achievements scale without hardcoding.
- Treat performance, saves, presentation, and release operations as production features, not late afterthoughts.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 4.6 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Difficulty System** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The feature improves content completeness, onboarding, replayability, or ship-readiness in a measurable way.
