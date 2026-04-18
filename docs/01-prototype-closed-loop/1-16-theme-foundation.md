# Task 1.16 - Theme Foundation

## Objective
Introduce **Theme Foundation** so the prototype gains a more game-like presentation layer without baking styling decisions directly into individual pages.

## Feature Increment
After this task, the player should be able to switch between at least a **day theme** and a **night theme** while the prototype keeps the same gameplay behavior.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Provide a player-visible theme switch control.
- Support at least **Day** and **Night** themes.
- Route shared surfaces, text, accents, borders, and emphasis states through centralized theme resources.
- Keep the existing prototype panels and controls readable in both themes.
- Preserve all current simulation, alerts, and action behavior while only changing presentation and theme state.

## Scope Notes
- Establish a reusable theme-token foundation first; do not bury theme logic in a one-off page rewrite.
- Prefer app-wide theme resources and view-model-friendly state over control-specific hardcoded colors.
- Keep room for later polish, animations, and richer art direction, but do not block on those in this task.

## Code Design
- Keep styling tokens, theme state, and gameplay state separated.
- Prefer resource-driven colors, spacing, and emphasis roles that future pages can reuse.
- Ensure theme switching is deterministic, immediate, and safe across the current MAUI application shell.

## Dependencies
- Build on Task 1.15 before implementation, and preserve all earlier loop guarantees for this phase.

## Completion Signal
- The player can switch between day and night presentation in the running app.
- The prototype remains readable and playable in both themes.
- Later UI redesign work can build on shared theme resources rather than re-solving theme state from scratch.
