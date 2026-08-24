# Authentication & Authorization

## Authentication

The API uses ASP.NET Core JWT Bearer authentication.

```text
Angular Login
    |
POST /api/auth/login
    |
AuthService
    |
PasswordHasher verification
    |
JWT issued
    |
Angular stores token
    |
Authorization: Bearer <token>
```

The JWT contains:

- user id / `NameIdentifier`
- display name
- email
- role

The signing key is configuration-driven under `Jwt:Key`. The checked-in value is for local development only; production must supply a secret through environment/secret management.

## Authorization

The API has a fallback authorization policy requiring an authenticated user. `AuthController.Login` is explicitly anonymous; all other API endpoints require a valid JWT unless a future endpoint overrides the policy.

Roles are persisted as `Admin` or `User`. Role claims are already included in the JWT and can be used by future `[Authorize(Roles = "Admin")]` policies.

## Password storage

Passwords are hashed with ASP.NET Core `PasswordHasher<TUser>`. Plain-text seeded credentials are not persisted in a fresh database. Startup seed logic also upgrades the legacy demo `demo123` value if it exists in an older local database.

## Audit identity

Issue creation, updates, status transitions and comments use the authenticated `NameIdentifier` claim to resolve `IssueActivity.ActorId`. Direct application-service tests without an HTTP context retain a deterministic test/dev fallback.

## Development credentials

Fresh local seed users use the demo password `demo123`:

- `darshan@example.com` — Admin
- `aarav@example.com` — User
- `priya@example.com` — User

These credentials are for development/demo use only.
