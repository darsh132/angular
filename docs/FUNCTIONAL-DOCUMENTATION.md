# Functional Documentation

## Product vision
Jira Clone is a learning/portfolio-grade Agile issue tracker demonstrating a vertical slice from Angular UI to ASP.NET Core and SQLite persistence.

## Actors
- Product Owner: prioritizes and reviews work.
- Scrum Master: monitors flow and sprint health.
- Developer: works on and transitions issues.
- Viewer: observes project status.

## Current functional scope

### Scrum board
Issues are grouped into Backlog, Todo, InProgress, InReview and Done. Each card exposes issue key, title, type, priority and assignee.

### Search and filtering
The board sends search text to the API. The backend searches issue title/description and can also filter by workflow status.

### Workflow transition
Users can move an issue to another status. The API persists the new state and timestamp; the board reloads after a successful transition.

### Issue creation API
The backend supports creation with title, description, status, priority, type and optional assignee. Issue numbers are generated per project.

### Responsive UI and themes
The UI uses Tailwind CSS 4 and daisyUI 5. Corporate, Night and Forest themes can be selected at runtime. Layouts adapt from mobile to desktop.

## Acceptance criteria
- Board loads seeded issues from the API.
- Search works without client-side downloading of unrelated data.
- Status changes persist after refresh.
- Invalid issue IDs return HTTP 404.
- Empty workflow columns have an explicit empty state.
- UI is usable at mobile, tablet and desktop widths.

## Definition of Done
- Acceptance criteria pass.
- Build and tests pass.
- No secrets are committed.
- API contracts and documentation are updated.
- Changes follow the repository's OOP/SOLID conventions.
- A reviewer can run the application from the documented commands.

## Product backlog roadmap
1. Production authentication and authorization.
2. Project/team management.
3. Backlog and sprint planning.
4. Issue details, comments and activity.
5. Drag-and-drop board.
6. Dashboard analytics.
7. Notifications and audit history.
