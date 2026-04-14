# Task 2.12 - Mortal Industry Upgrades

## Objective
Introduce **Mortal Industry Upgrades** so the economic ecosystem becomes strategically richer and sect behavior starts to diverge in meaningful ways.

## Feature Increment
After this task, **Mortal Industry Upgrades** should make specialization, trade, or ecosystem pressure more legible to the player.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Mortal agriculture can upgrade into high-yield agriculture.
- Mortal handicrafts can upgrade into fine handicrafts.
- Mortal commerce can upgrade into long-distance trade.
- Upgrades require investment and time.
- Upgrades increase output.

## Code Design
- Model supply, demand, specialization, and production quality as explicit simulation rules rather than one-off modifiers.
- Keep AI decision making transparent enough to explain why sects changed direction, traded, or expanded.
- Treat market, reputation, loyalty, and stability as connected systems that can all produce long-tail consequences.
- Express modifiers, prices, and policy effects through composable rule evaluators so balancing can happen without rewriting feature logic.
- Ensure all economic consequences can be traced through reports, logs, or explanatory UI labels.
- Prefer configuration-driven thresholds and weights over hardcoded branch logic whenever the feature will need tuning later.

## Dependencies
- Build on Task 2.11 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Mortal Industry Upgrades** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The economic ecosystem shows a meaningful strategic difference after this task is added.
