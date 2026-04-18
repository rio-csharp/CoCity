# Task 1.18 - Gameplay UI Redesign

## Objective
Introduce **Gameplay UI Redesign** so the prototype feels closer to a playable game and less like a vertically stacked debug dashboard.

## Feature Increment
After this task, the player should see a more attractive, game-oriented interface that presents the core governance loop through clearer navigation, hierarchy, and play-focused interaction patterns.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Replace the current single long-scroll presentation with a more game-oriented layout.
- Keep national overview, sect management, mortal realm status, ministry control, alerts, and turn actions quickly reachable.
- Improve visual hierarchy, atmosphere, and affordances so the interface feels inviting to play.
- Preserve theme support and language switching introduced in Tasks 1.16 and 1.17.
- Preserve all current simulation, alerts, and action behavior while redesigning the interface structure and visuals.

## Scope Notes
- Build on the theme and localization foundations instead of bypassing them.
- Favor a layout that supports repeated play sessions, not only one-time inspection.
- Keep the redesign within prototype scope: improve game feel and usability without requiring a full art pipeline or replacing the simulation model.

## Code Design
- Keep presentation structure, localized content, and gameplay orchestration separated.
- Prefer reusable panels, navigation shells, and view-model projections over one oversized page.
- Make the redesigned interface extensible for later management-ecosystem and policy-expansion tasks.

## Dependencies
- Build on Task 1.17 before implementation, and preserve all earlier loop guarantees for this phase.

## Completion Signal
- The prototype no longer relies on one long vertical dashboard as its primary interaction model.
- The player can operate the closed loop through a clearer, more attractive, game-oriented interface.
- Theme and language support remain functional after the redesign.
