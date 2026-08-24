# Technical Documentation

## Stack
- Angular 22 standalone components + TypeScript
- Tailwind CSS 4 + daisyUI 5
- ASP.NET Core .NET 8 Web API
- Entity Framework Core 8
- SQLite 3
- Swagger/OpenAPI in development

## Architecture
```text
Angular SPA
   │ REST/HTTP
   ▼
ASP.NET Core Controllers
   │
   ▼
EF Core DbContext
   │
   ▼
SQLite 3
```

The backend uses controllers for HTTP concerns, EF Core for persistence and explicit domain models/enums for business concepts. The frontend uses standalone components, lazy routing, dependency injection and a typed API service.

## OOP/SOLID rules
- Single Responsibility: controllers handle transport; application services should own complex use-case behavior as complexity grows.
- Dependency Inversion: Angular uses injected API services; .NET uses dependency injection for infrastructure.
- Encapsulation: workflow states are represented by enums rather than magic strings.
- Open/Closed: new issue behavior should be introduced through domain/application abstractions rather than unrelated UI edits.
- DTO boundary: public API contracts use request/response records rather than exposing EF entities directly.
- Composition over inheritance for Angular UI and backend services.

## Data model
`User`, `Project`, `Sprint`, `Issue` and `IssueComment` are represented in the backend domain model. Project has many Issues; Sprint can contain Issues; Issue can have an optional Assignee and many Comments.

## API
- `POST /api/auth/login` — development authentication contract.
- `GET /api/projects` — list projects and issue counts.
- `GET /api/issues?search=&status=` — list/search/filter issues.
- `GET /api/issues/{id}` — issue detail with comments.
- `POST /api/issues` — create an issue.
- `PATCH /api/issues/{id}/status` — transition workflow state.
- `POST /api/issues/{id}/comments` — add an issue comment.

## Frontend design
`src/app/core` contains API/infrastructure services. `src/app/features` contains feature-specific components. The board is lazy loaded from the router. Signals are used for local reactive state; RxJS handles HTTP streams.

## Styling
Tailwind supplies utility classes and daisyUI supplies semantic components/theme tokens. Theme selection changes `data-theme` on the document root and does not couple the domain to presentation colors.

## Database
SQLite is the default local store. EF Core maps domain enums and relationships. Unique indexes protect project keys and project-scoped issue numbers. Seed data creates a usable board for development.

## Security
The current login endpoint is explicitly **development-only** and compares the seeded credential value. It must not be deployed as-is. Production work must:
- Replace demo credentials with ASP.NET Core Identity or an equivalent secure credential store.
- Hash passwords using a slow salted password hashing algorithm.
- Use signed short-lived JWT access tokens plus refresh-token rotation.
- Add authorization policies and project roles.
- Add validation, rate limiting, secure headers and audit logging.
- Keep secrets outside source control.

## Testing strategy
- Unit tests for issue transition and validation rules.
- API integration tests against an isolated SQLite database.
- Angular component tests for board states and create/transition workflows.
- E2E test: authenticate → open project → search → create issue → transition issue → refresh.

## CI/CD
Every pull request should restore/build frontend and backend. Main is the protected integration branch. Production deployment should inject environment-specific API configuration and use a managed relational database if SQLite's single-node characteristics are insufficient.

## Local execution
### API
```bash
cd backend/JiraClone.Api
dotnet restore
dotnet run
```

### Angular
```bash
cd frontend
npm install
npm start
```

The current frontend API base is `https://localhost:7001/api`; keep it aligned with the backend launch profile/environment configuration.
