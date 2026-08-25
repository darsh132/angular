# Deployment Runbook

## Local prerequisites
- Node.js and npm compatible with the frontend package configuration.
- .NET 8 SDK.
- SQLite 3.
- HTTPS development certificate trusted for the API.

## Backend
```bash
dotnet restore backend/JiraClone.Api/JiraClone.Api.csproj
dotnet build backend/JiraClone.Api/JiraClone.Api.csproj
dotnet run --project backend/JiraClone.Api/JiraClone.Api.csproj
```

Configure the JWT signing key through environment/configuration; never commit a real signing key.

## Frontend
```bash
cd frontend
npm ci
npm start
```

The frontend API base URL is configured in `JiraApiService`; environment-specific configuration should be externalized before production deployment.

## Database
EF Core migrations must be applied before the application uses a changed schema. The current development startup applies pending migrations automatically.

## Production checklist
- Replace development JWT secret with a managed secret.
- Configure production CORS origins.
- Use HTTPS end-to-end.
- Review SQLite suitability for expected concurrency; migrate to a server database if workload requires it.
- Back up the database.
- Enable structured logging and health monitoring.
- Run backend and frontend CI tests before deployment.

## Rollback
1. Stop the new application version.
2. Restore the previous application artifact.
3. If a schema migration is backward-incompatible, follow the migration-specific rollback plan and restore the database backup when necessary.
4. Verify authentication, project access, issue reads and writes, and sprint operations.
