# Task 2.16 - Expanded User Interface

## Objective
Introduce **Expanded User Interface** so the economic ecosystem becomes strategically richer and sect behavior starts to diverge in meaningful ways.

## Feature Increment
After this task, **Expanded User Interface** should make specialization, trade, or ecosystem pressure more legible to the player.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Add a market panel showing resource prices and trade volume.
- Add an industry-chain panel showing sect industries and dependencies.
- Enhance the sect detail panel with industry, prestige, loyalty, and product quality.
- Enhance the mortal panel with industry output and local government efficiency.
- Add an event-chain panel showing recent events.

## Code Design
- Model supply, demand, specialization, and production quality as explicit simulation rules rather than one-off modifiers.
- Keep AI decision making transparent enough to explain why sects changed direction, traded, or expanded.
- Treat market, reputation, loyalty, and stability as connected systems that can all produce long-tail consequences.
- Build screens around projections or view models that consume application services, not raw simulation internals.
- Keep navigation, filtering, and summary widgets aligned with player decisions so the UI explains cause and effect instead of only listing values.
- Treat readability, information hierarchy, and error states as part of the feature definition, not a later polish pass.

## Dependencies
- Build on Task 2.15 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Expanded User Interface** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The economic ecosystem shows a meaningful strategic difference after this task is added.
