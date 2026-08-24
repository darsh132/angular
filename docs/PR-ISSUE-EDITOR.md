# PR: Issue Editor

## User story

As a project contributor, I want to edit issue metadata through a focused form so that issue information stays current without mixing editing concerns into the issue-detail page.

## Acceptance criteria

- Edit title and description.
- Change type and priority.
- Change story points.
- Change assignee.
- Save through the .NET API.
- Cancel without persistence.
- Validate basic client-side constraints.
- Preserve server-side business-rule enforcement.
- Provide automated component coverage.

## Technical notes

The editor is a standalone Angular component. The page owns loading/navigation; the editor owns form state and update submission. The API remains authoritative for domain validation and audit history.
