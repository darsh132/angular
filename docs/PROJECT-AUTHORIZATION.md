# Project Authorization

Projects now support membership roles: Viewer, Member and Manager.

- Viewer: read-only project access.
- Member: issue creation/editing and normal workflow actions.
- Manager: project administration and membership management.
- Global Admin: application administrator; treated as Manager for every project.

Authorization is evaluated server-side from the authenticated JWT user id and `ProjectMembers` membership. The Angular client must never be treated as the authority for permissions.

The membership table uses `(ProjectId, UserId)` as its composite primary key and stores the role as a string enum conversion.

## Migration

`202608250003_AddProjectMembers` creates the membership table. Seed initialization should assign the initial project manager and members before project features are exposed.
