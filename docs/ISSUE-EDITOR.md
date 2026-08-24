# Issue Editor

The issue editor is implemented as a standalone Angular component and page rather than expanding the existing issue-detail component.

## Routes

`/issues/:id/edit`

## Responsibilities

- Load the issue and user options.
- Present editable issue fields.
- Validate basic client-side constraints.
- Send `UpdateIssueRequest` to the API.
- Return to the previous screen after save/cancel.

## Server authority

The Angular client does not enforce business invariants as the source of truth. The API validates title, story points, assignee existence, Sprint ownership, project consistency, and completed-Sprint restrictions.

## Component boundary

```text
IssueEditorPage
  ├── data loading / navigation
  └── IssueEditor
       └── form state / PUT request
```

This keeps presentation concerns separate from issue-detail comments, workflow actions and activity history.
