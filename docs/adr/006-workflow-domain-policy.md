# ADR-006: Issue Workflow as a Domain Policy

- Status: Accepted
- Date: 2026-08-25

## Context
Issue status transitions are business rules. A browser control or controller should not be the authoritative source of allowed transitions.

## Decision
Keep the transition matrix in `IssueWorkflow` under the domain layer. Application services must call `EnsureCanTransition` before mutating issue state. The API remains responsible for project authorization before the application service is invoked.

## Workflow
```text
Backlog -> Todo
Todo -> InProgress | Backlog
InProgress -> InReview | Todo
InReview -> Done | InProgress
Done -> InProgress
```

## Consequences
- Workflow rules are deterministic and unit-testable.
- Angular can evolve its board UX without changing business policy.
- Invalid transitions fail before persistence mutation.
- Integration tests verify that successful transitions create audit activity and invalid transitions leave the persisted status unchanged.

## Future evolution
If workflows become project-configurable, replace the static policy with a persisted workflow configuration while retaining the same domain/application boundary and authorization checks.
