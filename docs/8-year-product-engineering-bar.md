# 8-Year Product Engineering Bar

## Objective
The Jira clone is intentionally being developed to demonstrate the engineering judgment expected from an experienced product engineer, not merely feature breadth. The target bar is maintainable, secure, observable, testable and evolvable software suitable for a real team and a meaningful production workload.

## Product capabilities
- Authentication and secure session lifecycle.
- Project-scoped RBAC.
- Issues, workflow transitions, comments and audit history.
- Backlog, board, sprint planning and sprint lifecycle.
- Project dashboard, burndown and velocity.
- Issue metadata and relationships.
- Epics and subtasks.
- Search, filtering, sorting and pagination.
- Notifications and user preferences.
- Attachments with secure storage boundaries.
- Import/export and bulk operations.
- Configurable project workflows where the domain warrants it.

## Engineering bar
### Architecture
- Clear frontend/backend boundary.
- Focused application services.
- Explicit domain policies for business invariants.
- DTO-based API contracts.
- Database constraints as a second line of defense.
- ADRs for consequential technical decisions.

### Reliability
- Idempotent mutation design where retries are possible.
- Optimistic concurrency for conflicting edits.
- Transaction boundaries around multi-write workflows.
- Structured logging with correlation IDs.
- Health/readiness checks.
- Graceful failure and ProblemDetails responses.
- Backup and restore procedure.

### Security
- Password hashing using a vetted framework implementation.
- Short-lived access tokens and rotating refresh tokens.
- HttpOnly/Secure cookie handling for refresh tokens.
- Token reuse detection and family revocation.
- Server-authoritative authorization.
- Input validation and output encoding.
- Rate limiting for authentication-sensitive endpoints.
- Secret configuration outside source control.

### Performance
- Server-side filtering/pagination.
- Database-side aggregates.
- Appropriate indexes.
- `AsNoTracking` for read models.
- Avoid N+1 query patterns.
- Cancellation propagation.
- Explicit performance budgets for critical endpoints.

### Testing
- Domain/unit tests for business rules.
- Integration tests for persistence and authorization.
- API contract tests for critical endpoints.
- Angular component/service tests.
- E2E tests for critical user journeys.
- Security regression tests.
- Concurrency tests for race-sensitive workflows.

### Delivery
- Conventional commits.
- Pull-request review discipline.
- Automated build/test quality gates.
- Reproducible local verification scripts.
- Database migration review.
- Release/rollback runbook.
- Feature flags for risky incremental releases where appropriate.

## Product evolution roadmap

### Stage A — Foundation
Authentication, RBAC, projects, issues, board, backlog, sprints, analytics, audit history.

### Stage B — Collaboration
Labels, components, watchers, issue relationships, attachments, notifications, mentions and user preferences.

### Stage C — Jira-scale hierarchy
Epics, subtasks, parent/child relationships, cross-project links, configurable issue types and workflow configuration.

### Stage D — Product operations
Advanced search, saved filters, bulk actions, dashboards, exports, import, activity feeds and administrative configuration.

### Stage E — Production engineering
Observability, rate limiting, concurrency control, caching where justified, background jobs, retention policies, backup/restore automation and operational health endpoints.

## Definition of senior-level quality
A feature is not considered complete because the UI works. It must have a coherent domain model, authorization boundary, persistence integrity, error semantics, automated tests, observability considerations, documentation and a safe delivery path.
