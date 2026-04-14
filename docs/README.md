# CoCity Development Documentation

## Game Direction

CoCity is a **cultivation-state governance simulator**. The player does not run a single sect directly. Instead, the player governs an entire realm by setting tax policy, ministry standards, construction priorities, crisis responses, and long-term national strategy. The core fantasy is to manage the tension between:

- **Sects** as semi-autonomous power centers with their own interests, growth, and loyalty;
- **Mortal society** as the tax base, labor pool, and industrial foundation of the realm;
- **Government ministries** as delegated administrative organs that execute policy with varying competence and integrity.

The loop expands across four milestones:

1. **Prototype Closed Loop** - prove the governance triangle works.
2. **Management Ecosystem** - build specialization, trade, and market pressure.
3. **Policy and Expansion** - add statecraft, crises, defense, and territorial growth.
4. **Full Release Content** - complete the content set, endgame, onboarding, and release polish.

## Repository Layout

- src/CoCity/ - current MAUI application source.
- docs/ - planning and execution documentation for phased development.
- Draft.md - the original planning draft used as source material for this English documentation set.

## How to Use This Documentation

- Start with the phase README in the relevant folder.
- Implement tasks in order unless a later design pass intentionally reshapes dependencies.
- Use progress-tracker.md to record status, ownership, and implementation notes.
- Treat each task file as the authoritative design brief for that increment; it contains scope, design guidance, and completion signals, but not implementation code.

## Cross-Cutting Design Principles

1. Keep the simulation deterministic enough to explain why the realm changed from one turn to the next.
2. Separate static content definitions, mutable runtime state, orchestration services, and presentation models.
3. Prefer data-driven rules for content that will grow: sect types, buildings, events, policies, achievements, and balance values.
4. Ensure every important consequence is surfaced through UI, reports, alerts, or history rather than hidden in internal state.
5. Build with save/load, balancing, and observability in mind from the start, even if those systems are delivered later.
