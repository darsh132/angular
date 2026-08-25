# Angular Authentication and Authorization

## HTTP authentication

`authInterceptor` is registered from `frontend/src/main.ts` using `provideHttpClient(withInterceptors(...))`. Every API request receives the JWT as a Bearer token when one is available.

A `401 Unauthorized` response clears the local authentication state and redirects to `/login`.

## Permission model

The frontend `PermissionService` is a UX layer for the backend project-role model:

- Viewer: view project data.
- Member: view and edit issues/comments and assign issues to sprints.
- Manager: all Member capabilities plus Sprint lifecycle and membership management.
- Admin: global access.

The service must never be treated as an authorization boundary. The .NET API independently validates the authenticated user's project membership and role.

## Testing

`auth.interceptor.spec.ts` verifies both Bearer-header propagation and 401 logout/navigation behavior.
