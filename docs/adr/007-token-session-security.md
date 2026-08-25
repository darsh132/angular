# ADR-007: Short-Lived Access Tokens and Rotating HttpOnly Refresh Cookies

- Status: Accepted
- Date: 2026-08-25

## Context
A long-lived bearer access token increases the impact of token theft. Storing a refresh token in browser JavaScript storage would additionally expose it to XSS token exfiltration.

## Decision
- Access JWTs are short-lived (15 minutes by default).
- Refresh tokens are cryptographically random 64-byte values.
- Only SHA-256 hashes of refresh tokens are persisted.
- Refresh tokens are rotated on every successful refresh.
- The previous token is revoked and linked to the replacement hash.
- The raw refresh token is delivered only as an `HttpOnly`, `Secure`, `SameSite=Strict` cookie scoped to `/api/auth`.
- Logout revokes the current refresh token and clears the cookie.
- Angular stores only the short-lived access token and user presentation state; it does not store the refresh token.
- Concurrent access-token refresh requests share one in-flight refresh operation in the Angular service.

## Consequences
- A stolen refresh token is not directly recoverable from the database.
- JavaScript cannot read the refresh cookie.
- Access-token expiry is transparent to API consumers through the interceptor refresh flow.
- Cookie-based refresh endpoints must retain appropriate SameSite/CSRF protections and must be served over HTTPS.
- Production secrets must be supplied through a secret-management mechanism rather than committed configuration.

## Configuration
`Jwt:AccessTokenMinutes` defaults to 15 and is bounded to 5–60 minutes. `Jwt:RefreshTokenDays` defaults to 30 and is bounded to 1–90 days.
