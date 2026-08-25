# Authorization Model

## Roles

| Role | View | Edit Issues | Manage Sprint | Manage Members |
|---|---:|---:|---:|---:|
| Viewer | Yes | No | No | No |
| Member | Yes | Yes | No | No |
| Manager | Yes | Yes | Yes | Yes |
| Admin | Yes | Yes | Yes | Yes |

## Enforcement
1. JWT authentication establishes the user identity.
2. `ProjectAuthorizationService` resolves project membership.
3. Controller/application operations call `EnsureCanViewAsync`, `EnsureCanEditAsync`, or `EnsureCanManageAsync` as appropriate.
4. Failure results in an HTTP authorization error.
5. Angular `PermissionService` mirrors the matrix only to improve UX.

## Security principle
Never trust frontend permission state. A user can modify browser JavaScript, routes, or HTTP requests. The backend must remain the authoritative policy enforcement point.

## Test matrix
- Anonymous request -> 401.
- Authenticated user without project access -> 403.
- Viewer read -> allowed; mutation -> denied.
- Member issue mutation -> allowed; project management -> denied.
- Manager project management -> allowed.
- Admin -> global administration according to endpoint policy.
