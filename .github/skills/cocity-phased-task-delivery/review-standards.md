# Review standards

Every task must pass at least three review rounds. If any round finds issues, fix them and rerun the relevant validation before moving on. The task is not complete until all three rounds pass cleanly.

## Round 1 - scope and logic review

Confirm that the implementation matches the current task document exactly.

Check all of the following:

1. Every documented requirement for the current task is implemented.
2. No later-task functionality was implemented early.
3. Predecessor behavior still works unless the task explicitly changes it.
4. The game logic is deterministic and explainable.
5. State transitions, simulation math, and player-visible outcomes are internally consistent.
6. Documentation and code use the same terminology and ownership boundaries.

If any requirement is missing or any future scope leaked in, fix it before continuing.

## Round 2 - code quality and compatibility review

Review the code for production quality.

Check all of the following:

1. .NET compiler and analyzer output does not regress.
2. Nullability, DI wiring, and data flow are consistent and explicit.
3. Shared game logic stays out of platform-specific MAUI files unless truly required.
4. Cross-platform compatibility is preserved by avoiding unnecessary Windows-only assumptions in shared code.
5. Public APIs, state models, and service boundaries are clean enough for later tasks to build on.
6. Tests cover both happy path and meaningful edge cases introduced by the task.

If the repository already has baseline warnings or unrelated failures, do not introduce new ones.

## Round 3 - gameplay, performance, and regression review

Review the task as part of the running game loop.

Check all of the following:

1. The feature keeps the turn loop understandable and smooth.
2. Hot-path code avoids needless allocations, repeated expensive projections, and hidden UI-thread work.
3. Mutable state does not grow uncontrollably across turns.
4. New calculations are surfaced clearly enough for players to understand why realm state changed.
5. Existing gameplay from earlier tasks still behaves correctly.
6. The feature does not silently degrade memory, performance, or observability.

If you find a problem, fix it and repeat the round until it passes.

## Test expectations

Unit and integration coverage should be high enough that the task's core logic can fail fast when future work regresses it.

At minimum, cover:

- deterministic simulation rules
- state mutation boundaries
- task-specific edge cases
- predecessor regressions that the new change could break
- view-model or orchestration projections when they encode task logic

## Completion gate

Do not mark the task done until:

1. all three review rounds have passed
2. build and tests are green or any remaining failure is proven to be pre-existing and unrelated
3. the implementation matches the task document
4. `docs/progress-tracker.md` is updated
5. branch, push, PR, self-review, and merge steps are complete or explicitly reported as blocked by external permissions
