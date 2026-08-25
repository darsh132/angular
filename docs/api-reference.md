# API Reference

All protected endpoints require a valid JWT. Project-scoped endpoints additionally enforce project membership/role authorization.

## Authentication

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/login` | Authenticate and issue JWT |

## Projects

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/projects` | List authorized projects |
| GET | `/api/projects/{projectId}/members` | List project members |
| PUT | `/api/projects/{projectId}/members/{userId}` | Add/update member role |
| DELETE | `/api/projects/{projectId}/members/{userId}` | Remove member |
| GET | `/api/projects/{projectId}/dashboard` | Project aggregate dashboard |
| GET | `/api/projects/{projectId}/labels` | List project labels |
| POST | `/api/projects/{projectId}/labels` | Create project label |
| GET | `/api/projects/{projectId}/components` | List project components |
| POST | `/api/projects/{projectId}/components` | Create project component |

## Issues

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/issues` | Query issues with project/status/type/priority/search filters |
| GET | `/api/issues/{id}` | Issue details, metadata, comments and activity |
| POST | `/api/issues` | Create issue |
| PUT | `/api/issues/{id}` | Update issue |
| PATCH | `/api/issues/{id}/status` | Transition workflow status |
| POST | `/api/issues/{id}/comments` | Add comment |
| POST | `/api/issues/{issueId}/labels/{labelId}` | Attach label |
| DELETE | `/api/issues/{issueId}/labels/{labelId}` | Remove label |
| POST | `/api/issues/{issueId}/components/{componentId}` | Attach component |
| DELETE | `/api/issues/{issueId}/components/{componentId}` | Remove component |
| POST | `/api/issues/{issueId}/watchers/{userId}` | Add watcher |
| DELETE | `/api/issues/{issueId}/watchers/{userId}` | Remove watcher |
| PATCH | `/api/issues/{issueId}/due-date` | Set or clear due date |

Metadata relationships are project-scoped: labels and components must belong to the same project as the issue, and watchers must be project members.

## Sprints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/projects/{projectId}/sprints` | List sprints |
| POST | `/api/projects/{projectId}/sprints` | Create sprint |
| POST | `/api/projects/{projectId}/sprints/{sprintId}/start` | Start sprint |
| POST | `/api/projects/{projectId}/sprints/{sprintId}/complete` | Complete sprint |
| POST | `/api/projects/{projectId}/sprints/{sprintId}/issues/{issueId}` | Assign issue to sprint |
| DELETE | `/api/projects/{projectId}/sprints/issues/{issueId}` | Remove issue from sprint |
| GET | `/api/projects/{projectId}/sprints/{sprintId}/analytics` | Server-side burndown/metrics |

## HTTP conventions
- `200` successful read/update operation.
- `201` resource creation where applicable.
- `204` successful mutation without a response body.
- `400` invalid request.
- `401` missing/invalid authentication.
- `403` authenticated but unauthorized.
- `404` resource/project not found or inaccessible according to endpoint policy.
- `409` business/database conflict.
- `500` unexpected server failure.

Errors use ASP.NET Core ProblemDetails where the exception handler maps application failures.
