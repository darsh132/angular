# Database Design

## Persistence
SQLite 3 is the development/default persistence engine. EF Core owns schema mapping, migrations and transactional persistence.

## Core aggregates

```text
Project
 ├── ProjectMember -> User
 ├── Issue
 │    ├── Comment -> User
 │    └── Activity -> User
 └── Sprint
      └── Issue (SprintId)
```

## Main entities

### Project
`Id`, `Key`, `Name`, `Description`.

### User
Identity/account information used for authentication and assignment.

### ProjectMember
Associates a user with a project and a project role.

### Issue
`Id`, `Key`, `Title`, `Description`, `Status`, `Priority`, `Type`, `StoryPoints`, `ProjectId`, `AssigneeId`, `SprintId`, timestamps.

### Sprint
`Id`, `ProjectId`, `Name`, `Goal`, `Status`, `StartDate`, `EndDate`.

### Comment
Issue discussion linked to an author.

### Activity
Audit/history record for material issue changes.

## Integrity rules
- Issue belongs to exactly one project.
- Sprint belongs to one project.
- An issue can only be assigned to a sprint belonging to its project.
- Project membership is unique per user/project.
- Foreign keys should be enforced by SQLite.

## Query considerations
Dashboard and analytics endpoints use database-side filtering/grouping/aggregation. Avoid loading all project issues into memory for project-wide metrics.

## Migration policy
Schema changes must be represented by EF Core migrations and reviewed with the model change. Application startup applies pending migrations in the current development setup; production deployment should use an explicit migration strategy appropriate to the hosting environment.
