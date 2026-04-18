---
name: cocity-phased-task-delivery
description: Execute one CoCity docs-driven development task at a time. Use when asked to implement a task from docs/, continue the next task in a phase, reconcile docs/progress-tracker.md, or run the full branch-review-PR-merge loop for a CoCity feature increment.
---

# CoCity phased task delivery

Use this skill for CoCity's documentation-led delivery workflow.

Treat the following files as the authoritative scope source, in this order:

1. `docs/README.md`
2. `docs/progress-tracker.md`
3. The current phase `README.md`
4. The current task document

Read `task-flow.md` for the operating procedure and `review-standards.md` for the mandatory review and validation gates.

## Default unit of work

- Complete exactly one task document per invocation.
- Only continue automatically to the next task if the user explicitly asks to continue the phase, continue with the next task, or repeat the loop.

## Hard rules

- Never implement functionality whose first owning document is a later task file.
- Never trust the tracker alone; verify predecessor work in code before starting the current task.
- Never mark a task as done until implementation, tests, three review rounds, tracker updates, and branch/PR/merge work are complete.
- If the working tree mixes multiple tasks or phases, stop and ask the user which target to continue.
- If validation reveals a pre-existing baseline failure, record it clearly, avoid making it worse, and do not misreport it as a regression introduced by the current task.
- Preserve previous task behavior unless a requirement explicitly changes it by design.

## Entry prompts this skill should handle

- "Implement task 1.3."
- "Continue the next task in phase 1."
- "Work through the next docs task and update the tracker."
- "Finish the current CoCity feature using the task workflow."

## Repository-specific notes

- CoCity is a phased game project. The task order is defined by the numbered files under `docs/01-*` through `docs/04-*`.
- Each task is one document, and phase boundaries matter.
- On Windows, build the MAUI app by overriding `TargetFrameworks` to the Windows target so the command does not require missing Apple workloads:
  `dotnet build .\CoCity.slnx -p:TargetFrameworks=net10.0-windows10.0.19041.0 -m:1`
- Run tests the same way once test projects exist:
  `dotnet test .\CoCity.slnx -p:TargetFrameworks=net10.0-windows10.0.19041.0 -m:1`
- If no automated tests exist for touched game logic yet, add focused unit and integration coverage for the code introduced by the current task before marking it complete.
