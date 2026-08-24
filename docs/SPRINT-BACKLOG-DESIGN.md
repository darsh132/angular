# Sprint and Backlog Design

## Domain rules

- A sprint belongs to exactly one project.
- An issue may belong to zero or one sprint.
- An issue and its sprint must belong to the same project.
- Sprint lifecycle is Planned → Active → Completed.
- Only Planned sprints can start.
- Only Active sprints can complete.
- A project can have only one Active sprint.
- Completed sprints cannot receive new issues.
- Removing an issue from a sprint returns it to the project backlog.
- Sprint end date must be after its start date.

## API

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/projects/{projectId}/sprints` | List project sprints |
| POST | `/api/projects/{projectId}/sprints` | Create planned sprint |
| POST | `/api/projects/{projectId}/sprints/{sprintId}/start` | Start sprint |
| POST | `/api/projects/{projectId}/sprints/{sprintId}/complete` | Complete sprint |
| POST | `/api/projects/{projectId}/sprints/{sprintId}/issues/{issueId}` | Assign issue |
| DELETE | `/api/projects/{projectId}/sprints/issues/{issueId}` | Remove issue |

## Application boundary

`SprintApplicationService` owns lifecycle and assignment use cases. The controller handles HTTP only. This keeps business rules independent of Angular and EF Core implementation details.

## Next UI increment

The Angular backlog will consume these APIs and provide:

1. Unassigned backlog list.
2. Sprint sections.
3. Add-to-sprint action.
4. Remove-from-sprint action.
5. Start/complete sprint controls.
6. Sprint story-point progress.
7. Drag-and-drop after the API workflow is proven.

## Testing matrix

- Create sprint with invalid dates → 400.
- Start planned sprint → Active.
- Start another sprint for same project → 409.
- Complete planned sprint → 409.
- Complete active sprint → Completed.
- Assign issue from same project → success.
- Assign issue from different project → 409.
- Assign to completed sprint → 409.
- Remove issue → SprintId becomes null.
