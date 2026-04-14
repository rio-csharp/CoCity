# Task 2.8 - Sect Loyalty System

## Objective
Introduce **Sect Loyalty System** so the economic ecosystem becomes strategically richer and sect behavior starts to diverge in meaningful ways.

## Feature Increment
After this task, **Sect Loyalty System** should make specialization, trade, or ecosystem pressure more legible to the player.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Sects have a loyalty value.
- Loyalty affects tax compliance and cooperation.
- High tax pressure reduces loyalty.
- Policy favoritism influences loyalty.
- Very low loyalty can lead sects to delay or evade taxes.

## Code Design
- Model supply, demand, specialization, and production quality as explicit simulation rules rather than one-off modifiers.
- Keep AI decision making transparent enough to explain why sects changed direction, traded, or expanded.
- Treat market, reputation, loyalty, and stability as connected systems that can all produce long-tail consequences.
- Keep application orchestration separate from domain rules so this feature can evolve without forcing UI rewrites.
- Define clear inputs, outputs, and side effects for this feature before implementation, especially where it touches turn resolution.
- Add enough reporting hooks that future debugging, balancing, and save/load work can inspect what this feature changed.

## Dependencies
- Build on Task 2.7 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Sect Loyalty System** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The economic ecosystem shows a meaningful strategic difference after this task is added.
