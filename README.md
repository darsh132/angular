# Jira Clone — Angular + .NET 8

A production-oriented Jira-style issue tracking application built with Angular and ASP.NET Core .NET 8.

## Stack

- Angular
- ASP.NET Core .NET 8 Web API
- Entity Framework Core
- SQLite by default for local development
- JWT authentication
- REST API
- Board / backlog / issue workflow

## Planned modules

- Authentication and user profiles
- Projects
- Scrum/Kanban boards
- Backlog and sprint management
- Issues with status, priority, type and assignee
- Issue details, comments and activity
- Search and filters
- Dashboard metrics
- Responsive Jira-style UI

## Repository layout

```text
frontend/   Angular application
backend/    ASP.NET Core .NET 8 Web API
```

## Local development

### Backend

```bash
cd backend
 dotnet restore
 dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm start
```

The frontend is configured to call the local API at `https://localhost:7001/api`.
