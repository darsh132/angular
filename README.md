# Jira Clone — Angular + .NET 8

A portfolio-grade Jira-style Agile issue tracker built as a full-stack vertical slice with Angular 22, ASP.NET Core .NET 8, EF Core 8 and SQLite 3.

## Stack
- Angular 22 standalone components + TypeScript
- Tailwind CSS 4 + daisyUI 5
- ASP.NET Core .NET 8 Web API
- Entity Framework Core 8
- SQLite 3
- REST/OpenAPI
- xUnit test strategy

## Current features
- Responsive Scrum board
- Backlog/Todo/In Progress/In Review/Done workflow
- Server-side search and status filtering
- Issue creation API
- Issue status transitions persisted to SQLite
- Seeded development data
- daisyUI Corporate, Night and Forest themes
- Swagger in development
- Functional, technical and Agile documentation

## Repository layout
```text
frontend/                       Angular SPA
  src/app/core                  API/infrastructure services
  src/app/features/board        Scrum board feature
backend/JiraClone.Api/          ASP.NET Core API
docs/                           Functional, technical and Agile docs
```

## Run locally

### Backend
```bash
cd backend/JiraClone.Api
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200`.

The Angular API service currently targets `https://localhost:7001/api`; keep the API launch profile aligned with that URL.

## Documentation
- [Functional Documentation](docs/FUNCTIONAL-DOCUMENTATION.md)
- [Technical Documentation](docs/TECHNICAL-DOCUMENTATION.md)
- [Agile Backlog](docs/AGILE-BACKLOG.md)

## Engineering standards
The project follows SOLID/OOP boundaries, dependency injection, typed API contracts, async I/O, feature-oriented Angular structure, semantic UI components, small reviewable changes, and a Definition of Done. Production hardening items are explicitly documented rather than presenting demo authentication as secure.
