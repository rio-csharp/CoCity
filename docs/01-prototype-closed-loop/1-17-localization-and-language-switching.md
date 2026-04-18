# Task 1.17 - Localization and Language Switching

## Objective
Introduce **Localization and Language Switching** so the prototype can present the same governance loop to players in more than one language.

## Feature Increment
After this task, the player should be able to switch the prototype between at least **English** and **Chinese** without changing gameplay behavior.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Provide a player-visible language switch control.
- Support at least **English** and **Chinese**.
- Move player-facing UI strings for the current prototype surfaces into a reusable localization system.
- Keep language changes app-wide and immediate enough for regular play.
- Preserve all current simulation, alerts, and action behavior while only changing presentation text and language state.

## Scope Notes
- Build a reusable localization path for the current MAUI application instead of scattering ad hoc string toggles through individual pages.
- Focus on the existing playable prototype surfaces first: overview, sect, mortal, ministry, alerts, turn controls, and settings/switcher text.
- Keep room for more languages later, but do not block on them in this task.

## Code Design
- Keep language resources, selection state, and gameplay state separated.
- Prefer deterministic resource lookup and clear fallback behavior when a localized string is missing.
- Make the localization path easy to extend to future screens and future content-heavy phases.

## Dependencies
- Build on Task 1.16 before implementation, and preserve all earlier loop guarantees for this phase.

## Completion Signal
- The player can switch between English and Chinese in the running app.
- The current prototype surfaces update through a shared localization mechanism.
- Later tasks can add more localized content without replacing the language-switching path.
