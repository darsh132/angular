# EF Core Migrations

This project uses EF Core migrations for schema lifecycle management.

The API startup applies pending migrations with `Database.MigrateAsync()` before seed initialization.

## Developer workflow

From `backend/JiraClone.Api`:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

For a clean database, delete the local `jira.db` and start the API. Pending migrations are applied automatically.

## Production rule

Do not use `EnsureCreated()` for deployed databases. Schema changes must be represented by reviewed EF Core migrations.

## Current model constraints

- `User.Email` unique
- `Project.Key` unique
- `(Issue.ProjectId, Issue.Number)` unique
- Sprint and Issue relationships use explicit delete behavior
- enum persistence uses string columns
