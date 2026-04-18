# Task 1.4 - Mortal Taxation Loop

## Objective
Introduce **Mortal Taxation Loop** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Mortal Taxation Loop** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- The mortal realm generates tax revenue.
- Tax rates can be adjusted.
- Tax rates affect mortal stability.
- Collected taxes flow into the national treasury.

## Scope Boundaries
- This task establishes the **baseline taxation loop** for the mortal realm. It may expose a narrow tax-rate control needed to demonstrate the loop, but it must not expand into the broader player action surface reserved for Task 1.12.
- Implement direct taxation rules and treasury accumulation without introducing the ministry automation layer reserved for Task 1.11.
- Represent tax-driven stability impact in a lightweight, deterministic way suitable for the prototype, but do **not** build the full mortal stability system reserved for Task 2.14.
- Do **not** implement sect taxation, tax exemptions, unrest events, or ministry success/failure handling in this task.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Express modifiers, prices, and policy effects through composable rule evaluators so balancing can happen without rewriting feature logic.
- Ensure all economic consequences can be traced through reports, logs, or explanatory UI labels.
- Prefer configuration-driven thresholds and weights over hardcoded branch logic whenever the feature will need tuning later.

## Dependencies
- Build on Task 1.3 before implementation, and preserve all earlier loop guarantees for this phase.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Mortal Taxation Loop** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
