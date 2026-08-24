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
- Single Responsibility: controllers handle transport; services/components own focused behavior.
- Dependency Inversion: Angular uses injected API services; .NET uses dependency injection for DbContext and application services.
- Encapsulation: workflow states are represented by enums rather than magic strings.
- Open/Closed: new issue types/status behavior should be added through domain abstractions rather than editing unrelated UI concerns.
- DTO boundary: public API contracts should use request/response records as the surface expands.
- Composition over inheritance for Angular UI and backend services.

## Data model
`User`, `Project`, `Sprint`, `Issue` and `IssueComment` are represented in the backend domain model. Project has many Issues; Sprint can contain Issues; Issue can have an optional Assignee and many Comments.

## API
- `GET /api/issues?search=&status=` — list/search/filter issues.
- `POST /api/issues` — create an issue.
- `PATCH /api/issues/{id}/status` — transition workflow state.
- Project and health endpoints are provided by the API composition layer.

## Frontend design
`src/app/core` contains API/infrastructure services. `src/app/features` contains feature-specific components. The board is lazy loaded from the router. Signals are used for local reactive state; RxJS handles HTTP streams.

## Styling
Tailwind supplies utility classes and daisyUI supplies semantic components/theme tokens. Theme selection changes `data-theme` on the document root and does not couple the domain to presentation colors.

## Database
SQLite is the default local store. EF Core maps domain enums and relationships. Seed data creates a usable board for development.

## Security hardening required before production
- Replace demo credentials with ASP.NET Core Identity or an equivalent secure credential store.
- Hash passwords using a slow salted password hashing algorithm.
- Use signed short-lived JWT access tokens plus refresh-token rotation.
- Add authorization policies and project roles.
- Add validation, rate limiting, CSRF strategy where applicable, secure headers and audit logging.
- Keep secrets in environment/secret stores.

## Testing strategy
- Unit tests for issue transition and application rules.
- API integration tests against an isolated SQLite database.
- Angular component tests for board states and transitions.
- E2E test: authenticate → open project → search → transition issue → refresh.

## CI/CD
Every pull request should restore/build/test frontend and backend. Main is the protected integration branch. Production deployment should inject environment-specific API configuration and use a managed relational database if SQLite's single-node characteristics are insufficient.

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
