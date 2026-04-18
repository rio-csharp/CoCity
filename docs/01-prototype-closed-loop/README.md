# Phase 1 - Prototype Closed Loop

## Goal
Prove the core governance loop: the player sets policy, ministries execute daily work, sects and mortal settlements react, and the state receives visible consequences next turn.

## Intended Player Experience
The player should feel they are governing a cultivation state rather than micromanaging a single faction.

## Architecture Priorities
- Prioritize deterministic turn resolution and highly visible state changes.
- Keep administrative logic, world simulation, and presentation concerns separated so the prototype can expand without rewrites.
- Surface every important outcome through reports, dashboards, or alerts instead of hidden state changes.

## Task Sequence
- [1.1 - Core Data Foundations](./1-1-core-data-foundations.md)
- [1.2 - Mortal Realm Baseline Simulation](./1-2-mortal-realm-baseline-simulation.md)
- [1.3 - Mortal Industry Simulation](./1-3-mortal-industry-simulation.md)
- [1.4 - Mortal Taxation Loop](./1-4-mortal-taxation-loop.md)
- [1.5 - Sect Baseline State](./1-5-sect-baseline-state.md)
- [1.6 - Sect Recruitment Workflow](./1-6-sect-recruitment-workflow.md)
- [1.7 - Sect Autonomous Operations](./1-7-sect-autonomous-operations.md)
- [1.8 - Foundational Building System](./1-8-foundational-building-system.md)
- [1.9 - Sect Auto-Construction Logic](./1-9-sect-auto-construction-logic.md)
- [1.10 - Ministry Framework](./1-10-ministry-framework.md)
- [1.11 - Ministry Automation Rules](./1-11-ministry-automation-rules.md)
- [1.12 - Player Core Actions](./1-12-player-core-actions.md)
- [1.13 - Turn Advancement Pipeline](./1-13-turn-advancement-pipeline.md)
- [1.14 - Baseline User Interface](./1-14-baseline-user-interface.md)
- [1.15 - Baseline Events and Alerts](./1-15-baseline-events-and-alerts.md)
- [1.16 - Theme Foundation](./1-16-theme-foundation.md)
- [1.17 - Localization and Language Switching](./1-17-localization-and-language-switching.md)
- [1.18 - Gameplay UI Redesign](./1-18-gameplay-ui-redesign.md)

## Exit Standard
Playable for 30 to 60 turns with no dead loop, enough information for the player to understand why the state changed, baseline day/night presentation support, baseline Chinese/English language switching, and a game-oriented interface that is more inviting than a raw debug dashboard.
