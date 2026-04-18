# Task flow

Follow this workflow in order every time this skill is used.

## 1. Identify the single target task

1. Check the current working tree first.
   - Run `git --no-pager status --short`.
   - If there are staged or unstaged changes, inspect them before doing anything else.
2. Determine the target task using this priority order:
   - An explicit task number, task filename, or phase supplied by the user.
   - A cleanly inferable task from the current branch name, current documentation edits, or current code changes.
   - The earliest unfinished task in `docs/progress-tracker.md`, starting from the earliest unfinished phase.
3. Infer the phase from the task document path under `docs/`.
4. If the working tree contains unrelated edits across multiple tasks or phases, ask the user which target to continue and do not guess.

## 2. Read the full task context before coding

Read all of the following before implementation:

1. `docs/README.md`
2. `docs/progress-tracker.md`
3. The phase `README.md`
4. The current task document
5. Every task file in the same phase folder

Use the same-phase files in two ways:

- Earlier files define dependencies and behavior that must still hold.
- Later files define future ownership boundaries so you do not implement upcoming features early.

## 3. Reconcile tracker state before implementation

Use `docs/progress-tracker.md` as the source of status reporting, but correct it when it drifts from reality.

1. Verify the row for the current task exists and matches the correct phase and deliverable.
2. Verify predecessor rows that should already be complete are actually reflected in the code.
3. If the tracker is inaccurate, fix it before starting the new feature work.
4. Move the target task to `In Progress` when active.
5. Use `Blocked` only when a real blocker prevents continuation.
6. Only set the task to `Done` after merge-ready completion.

Keep `Owner` and `Notes` concise and factual.

## 4. Verify dependencies in code, not just in docs

Before changing code:

1. Check that all predecessor task completion signals still hold in the current codebase.
2. Verify the code contracts that the current task depends on actually exist and are usable.
3. If a predecessor is marked done but is incomplete in code, repair that gap first or ask the user how to proceed if the fix would materially change scope.

Do not bury predecessor repair work inside the current task unless it is tightly coupled and necessary for the current task to function.

## 5. Refine the current task document before implementation

Update the current task document when clarification is needed for safe implementation.

Allowed documentation refinements:

- Clarify ambiguous requirements.
- Tighten scope boundaries.
- Add missing dependency notes.
- Add concrete completion signals.
- Add implementation notes that help avoid under-building or over-building.

Forbidden documentation refinements:

- Pulling future-task requirements into the current task.
- Rewriting the phase plan to justify opportunistic extra features.
- Marking speculative work as required when it is not documented.

## 6. Start from main and isolate the work

Unless the user explicitly asks to continue a specific existing branch:

1. Ensure local `main` is current.
   - `git --no-pager fetch origin --prune`
   - `git --no-pager switch main`
   - `git --no-pager pull --ff-only`
2. Create a fresh branch named `feat/<task-number>-<task-slug>`.
3. Do all task work on that branch only.

If the target branch already exists for the same task and is the correct branch, continue on it instead of creating a second branch.

## 7. Implement only the current increment

Implementation rules:

1. Build only what the current task owns.
2. Keep domain rules, mutable runtime state, orchestration services, and presentation/view models separated.
3. Keep simulation behavior deterministic and explainable between turns.
4. Add observability so important state changes are visible in the UI, reports, alerts, or history surfaces already in scope.
5. Do not add placeholders that pretend the feature is complete.
6. Do not implement later-task systems just because the current code area is adjacent.

When shared code must change, verify the change preserves earlier task behavior unless the document explicitly changes that behavior.

## 8. Add regression protection and feature tests

Testing is mandatory.

1. Run existing automated tests if they exist.
2. If the repository still has no test project for the touched logic, add focused unit and integration tests for the current task's new domain behavior.
3. Prefer tests around deterministic simulation services, rule evaluators, state transitions, and view-model projections rather than brittle UI rendering tests.
4. Include regression coverage for the predecessor behavior your changes could break.

Do not accept failing tests. Fix the code or the test, then rerun the affected suite.

## 9. Validate with repository-aware commands

On Windows, use the Windows-only target override for MAUI validation:

- Build: `dotnet build .\CoCity.slnx -p:TargetFrameworks=net10.0-windows10.0.19041.0 -m:1`
- Test: `dotnet test .\CoCity.slnx -p:TargetFrameworks=net10.0-windows10.0.19041.0 -m:1`

If a command fails because of a pre-existing baseline issue:

1. Confirm the failure is unrelated to the current change.
2. Record it clearly in your notes or handoff.
3. Do not lower the bar for new code quality or misstate the repository as healthy when it is not.

## 10. Run the full completion loop

After implementation and validation:

1. Compare the finished code against the current task document's requirements, scope boundaries, non-goals, dependencies, and completion signal.
2. Update `docs/progress-tracker.md` with final status, owner, and notes.
3. Commit the work.
4. Push the branch.
5. Open a pull request.
6. Review the PR yourself and fix issues found.
7. Merge the PR yourself if repository policy allows.

Prefer automation when available, such as `gh pr create` and `gh pr merge`.

If push, PR creation, or merge is blocked by missing authentication or permissions, complete everything else and report the blocker plainly instead of pretending the loop is finished.

## 11. Only then consider the next task

By default, stop after one completed task.

If the user explicitly asks to continue automatically:

1. Re-open the tracker.
2. Select the next task in the same phase unless the phase is complete.
3. If the phase is complete, move to the next phase.
4. Repeat this workflow from step 1.
