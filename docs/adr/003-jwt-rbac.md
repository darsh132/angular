# ADR-003: JWT and Project RBAC

- Status: Accepted
- Date: 2026-08-25

## Context
The application requires authenticated API access and project-scoped roles.

## Decision
Use JWT Bearer authentication for API identity and a project membership/role model for authorization. Backend authorization is authoritative.

## Consequences
- Stateless API authentication.
- Explicit project authorization checks.
- Frontend can provide role-aware UX without becoming a security boundary.
- Token lifecycle and secret management must be handled securely in deployment.
