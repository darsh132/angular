# Functional Requirements

## Product
Jira Clone — an Agile project-management application built with Angular and ASP.NET Core.

## Actors
- **Admin** — global administration and project management.
- **Manager** — project administration, sprint lifecycle, and membership management.
- **Member** — issue execution and sprint assignment.
- **Viewer** — read-only project access.

## Core capabilities

### FR-01 Authentication
Users shall authenticate using email/password and receive a JWT-backed authenticated session.

### FR-02 Projects
Authenticated users shall view projects they are authorized to access.

### FR-03 Issues
Authorized users shall create, read, update and transition issues. Issues shall support type, priority, status, story points, assignee and sprint membership.

### FR-04 Board
Users shall visualize issues by workflow status and transition issues through drag-and-drop or explicit move actions.

### FR-05 Backlog
Users shall filter backlog work and assign/remove issues from sprints.

### FR-06 Sprints
Managers shall create, start and complete sprints. Members with edit permission shall assign/remove issues.

### FR-07 Members
Managers shall view project membership, change project roles and remove members.

### FR-08 Analytics
Authorized users shall view sprint burndown and project delivery metrics. Analytics shall be calculated server-side from persisted data.

### FR-09 Auditability
Material issue and project workflow changes shall be represented in the activity/audit model.

### FR-10 Themes
The UI shall support Tailwind CSS and daisyUI themes without coupling business logic to presentation styling.

## Non-functional requirements
- NFR-01: API authorization is authoritative; client-side permission checks are UX only.
- NFR-02: Business logic shall reside in application services, not controllers.
- NFR-03: Persistence shall use EF Core with SQLite 3.
- NFR-04: Public API contracts shall use typed request/response models.
- NFR-05: CI shall build and test backend and frontend.
- NFR-06: UI shall be responsive for desktop and mobile layouts.
- NFR-07: Errors shall use consistent HTTP status/problem responses.

## Acceptance principles
Every feature is considered complete only when its acceptance behavior is implemented, authorized, tested at the appropriate layer, documented, and included in CI where applicable.
