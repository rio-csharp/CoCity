# Task 1.1 - Core Data Foundations

## Objective
Introduce **Core Data Foundations** so the prototype loop gains another reliable state-management capability without hiding outcomes from the player.

## Feature Increment
After this task, the player should see **Core Data Foundations** participate directly in the first playable taxation-administration-production loop.

## Detailed Requirements
Implement the scope below as production-ready game behavior rather than temporary UI-only placeholders.
- Region data: name, spiritual vein value, and baseline attributes.
- Sect data: name, funding, population, and output.
- Mortal town data: population, industries, and output.
- Ministry data: authority, standards, and officials.
- Treasury data: funds and tax income.

## Code Design
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.
- Separate static content definitions from mutable campaign state so later saves, balancing passes, and content growth remain manageable.
- Use stable identifiers and explicit relationships instead of direct object graphs to avoid tight coupling between simulation, UI, and persistence.
- Document which fields are authoritative, derived, cached, or presentation-only before implementation begins.

## Dependencies
- This phase starts from the repository baseline and establishes the first playable governance loop.
- Keep the feature compatible with the phase exit standard described in README.md for this folder.

## Completion Signal
- The player-facing game loop can demonstrate **Core Data Foundations** without relying on placeholder-only behavior.
- State changes from this task are visible in the appropriate reports, panels, or simulation outputs.
- The first closed-loop prototype remains deterministic enough to reason about between turns.
