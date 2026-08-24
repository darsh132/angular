# Issue Editing and Filtering

## Functional requirements

Users can filter issues by project, sprint, assignee, status, priority, type, and free-text search. Filters are translated to API query parameters and evaluated server-side.

Users can edit title, description, priority, type, story points, assignee, and sprint. The API validates referenced users and sprints and rejects cross-project or completed-sprint assignments.

## Technical design

```text
Angular filter state
        |
        v
GET /api/issues?...
        |
        v
EF Core IQueryable
        |
        v
SQLite
```

Issue updates follow:

```text
Angular form
   -> PUT /api/issues/{id}
   -> IssueApplicationService.UpdateAsync
   -> validation
   -> entity mutation
   -> IssueActivity audit records
   -> SaveChangesAsync
```

## Invariants

- Title must be non-empty.
- Story points cannot be negative.
- Assignee must exist when supplied.
- Sprint must exist when supplied.
- Issue and sprint must belong to the same project.
- Completed sprints cannot receive issues.

## Audit behavior

Priority changes create `PriorityChanged` activity records. Assignee changes create `AssigneeChanged` records. General edits create an `Updated` record. Status changes remain represented by `StatusChanged`.

## Security boundary

The client is not trusted for authorization or domain integrity. The API remains authoritative. Authentication/authorization is a later security-hardening milestone; the current seeded application uses a deterministic actor for audit attribution.
