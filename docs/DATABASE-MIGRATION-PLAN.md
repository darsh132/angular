# Database Migration Plan

## Current state
The application currently uses `Database.EnsureCreated()` for frictionless local bootstrapping. This is acceptable for the prototype phase but is not the intended production schema-evolution mechanism.

## Target
Move to EF Core migrations before production deployment.

```bash
dotnet ef migrations add InitialCreate --project backend/JiraClone.Api --startup-project backend/JiraClone.Api
dotnet ef database update --project backend/JiraClone.Api --startup-project backend/JiraClone.Api
```

## Planned migration sequence

1. Initial schema: users, projects, sprints, issues, comments.
2. Issue story points.
3. Issue activity/audit history.
4. Future labels, due dates and attachments.

## Deployment rule
Never use `EnsureCreated()` against an existing production database after migrations become authoritative. Deployment should execute the reviewed migration set and fail safely if schema application fails.

## Concurrency
Issue numbering currently uses `MAX(Number) + 1` inside the application service. The unique `(ProjectId, Number)` database index remains the integrity boundary, but a future high-concurrency implementation should use an atomic project sequence/counter rather than relying on `MAX + 1`.
