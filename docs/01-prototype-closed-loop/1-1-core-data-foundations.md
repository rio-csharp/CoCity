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

## Scope Boundaries
- This task establishes the **foundational data model** for the prototype. It should define the shape of the game world, not its full turn-based behavior.
- Deliver a **seeded in-memory world snapshot** that can be inspected in the running app, so later tasks inherit a concrete and testable baseline.
- Keep relationships explicit through identifiers and top-level collections rather than tightly nested mutable object graphs.
- Do **not** implement turn resolution, taxation logic, recruitment logic, ministry automation, construction rules, market behavior, or event processing in this task.
- Do **not** introduce persistence yet; the goal is a clean runtime model that can support persistence later.

## Data Inventory
The first implementation pass should define these entities and the minimum information each one must own:

1. **Region**
   - Stable identifier.
   - Display name.
   - Spiritual vein strength.
   - Baseline descriptive attributes that explain why the region is strategically different.
   - References to the towns and sects located in the region.
2. **Sect**
   - Stable identifier.
   - Region identifier.
   - Display name.
   - Funding.
   - Population.
   - Baseline output snapshot.
3. **Mortal Town**
   - Stable identifier.
   - Region identifier.
   - Display name.
   - Population.
   - Industry mix.
   - Baseline output snapshot.
4. **Ministry**
   - Stable identifier.
   - Ministry identity or type.
   - Authority summary.
   - Handling standard summary.
   - Official roster.
5. **Official**
   - Stable identifier.
   - Display name.
   - Role.
   - Administration, integrity, and loyalty ratings.
6. **Treasury**
   - Current funds.
   - Current tax income baseline.

## Implementation Notes
- Use immutable or effectively immutable data carriers for the foundational world snapshot so the first simulation layer has a reliable source of truth.
- Keep the model expressive without overcommitting to later mechanics. For example, store baseline outputs and industry allocations as data, but do not embed production algorithms yet.
- Seed a small but readable realm that already reflects the game fantasy: multiple regions, several sects, several towns, three ministries, and a national treasury.
- Ensure the first UI surface is **read-only** and focused on inspection. It only needs to prove the data exists and is wired cleanly through the application.

## Proposed Code Structure
- **Domain model layer**: world-state records or classes for regions, sects, towns, ministries, officials, and treasury.
- **Foundation data provider**: a small in-memory builder or repository that constructs the initial campaign snapshot.
- **Presentation layer**: a page-level view model that transforms the foundation snapshot into player-readable sections for the app.
- **App composition**: dependency injection wiring for the provider and page model so later tasks can extend the same flow.

## Non-Goals
- No save/load workflow.
- No editable forms for player policy input.
- No derived simulation pipeline beyond simple display-ready summaries.
- No balancing pass outside of choosing sensible seed values.

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
- The app displays the seeded realm data through a clean read-only inspection surface.
- Later prototype tasks can build on these data contracts without replacing the entire model.
