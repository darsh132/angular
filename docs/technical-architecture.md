# Technical Architecture

## Architecture style
The system uses a layered application architecture with a thin ASP.NET Core API/controller layer, application services for business workflows, EF Core for persistence, and an Angular standalone-component frontend.

```text
Angular UI
  -> JiraApiService / query services
  -> HTTP + JWT
  -> ASP.NET Core controllers
  -> authorization services
  -> application services
  -> EF Core DbContext
  -> SQLite 3
```

## Frontend
- Angular standalone components and lazy routes.
- Angular Router for navigation and URL-persisted query state.
- HttpClient interceptor for JWT propagation.
- Angular CDK for board/backlog drag-and-drop.
- Tailwind CSS + daisyUI for responsive styling and themes.
- Typed API contracts in `JiraApiService`.

## Backend
- ASP.NET Core .NET 8 Web API.
- JWT Bearer authentication.
- Fallback authorization policy requiring authentication.
- `ProjectAuthorizationService` for project-scoped permissions.
- Application services such as `IssueApplicationService`, `SprintApplicationService`, `SprintAnalyticsService`, and `ProjectDashboardService`.
- ProblemDetails-based error handling.
- EF Core with SQLite 3 and migrations.

## OOP/design principles
- **Single Responsibility:** controllers translate HTTP; application services execute business workflows; DbContext persists data.
- **Dependency Inversion:** controllers depend on injected application services rather than concrete persistence operations.
- **Encapsulation:** authorization decisions are centralized in `ProjectAuthorizationService`.
- **Separation of concerns:** analytics calculations are server-side and separate from Angular presentation.
- **DTO boundary:** API request/response models prevent persistence entities from becoming the frontend contract.

## Security boundary
The browser may hide or disable controls based on `PermissionService`, but every sensitive API operation independently evaluates the authenticated user and project membership on the server.

## Data flow example
```text
PATCH /api/issues/42/status
 -> JWT authentication
 -> controller
 -> authorization
 -> issue application service
 -> transaction / EF Core
 -> SQLite
 -> activity/audit update
 -> HTTP response
```

## Analytics
Sprint analytics and project dashboard aggregates execute against persisted database state. Angular receives purpose-built response contracts rather than reconstructing project-wide metrics from an issue collection.

## Quality gates
The intended CI gate is backend restore/build/test plus frontend dependency installation/build/test. Changes are not considered production-ready solely because the application compiles; authorization, regression tests and documentation are part of the definition of done.
