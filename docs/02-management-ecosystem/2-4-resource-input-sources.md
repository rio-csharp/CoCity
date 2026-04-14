# Task 2.4 - Resource Input Sources

## Objective
Introduce **Resource Input Sources** so the economic ecosystem becomes strategically richer and sect behavior starts to diverge in meaningful ways.

## Feature Increment
After this task, **Resource Input Sources** should make specialization, trade, or ecosystem pressure more legible to the player.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Ore comes from mining sects or mortal mining.
- Herbs come from spirit agriculture sects.
- Paper and ink come from mortal handicrafts.
- Fertilizer comes from mortal agriculture or special sects.
- Formation materials come from special industries.

## Code Design
- Model supply, demand, specialization, and production quality as explicit simulation rules rather than one-off modifiers.
- Keep AI decision making transparent enough to explain why sects changed direction, traded, or expanded.
- Treat market, reputation, loyalty, and stability as connected systems that can all produce long-tail consequences.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 2.3 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Resource Input Sources** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The economic ecosystem shows a meaningful strategic difference after this task is added.
