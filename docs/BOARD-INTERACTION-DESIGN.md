# Board Interaction Design

## Drag-and-drop contract

The Angular board uses Angular CDK drag-and-drop only as a presentation/input mechanism. A drop does not become authoritative until the backend accepts the workflow transition.

```text
User drags issue
      ↓
Angular updates local state optimistically
      ↓
PATCH /api/issues/{id}/status
      ↓
IssueApplicationService
      ↓
IssueWorkflow validation
      ↓
SQLite transaction/persistence
      ↓
Success → keep UI state
Failure → restore previous state
```

## Why optimistic UI?

A status transition should feel immediate while the API call is in flight. The previous issue collection is retained so a rejected transition can roll back without a full-page refresh.

## Source of truth

The API remains authoritative. The client must not invent successful transitions or bypass domain validation.

## Valid/invalid transitions

The backend workflow policy decides whether a transition is legal. This keeps drag-and-drop, dropdown actions and future keyboard shortcuts consistent because they all invoke the same API operation.

## Failure behavior

- API success: clear the moving state.
- API error: restore the previous issue collection and clear the moving state.
- Invalid transition: backend returns a conflict and the UI rolls back.

## Accessibility/fallback

Drag-and-drop is an interaction accelerator, not the only workflow control. Every issue retains a "Move issue" action so keyboard and non-pointer users can perform transitions without dragging.

## Performance

The current board keeps the loaded issue set in a signal and derives columns by status. For large projects, the next optimization is server-side pagination/filtering and a normalized client store rather than repeatedly filtering a very large collection.
