# Development Standards

## Naming
- C#: PascalCase for types/methods; camelCase for locals/parameters.
- TypeScript: PascalCase classes/types; camelCase properties/functions.
- API routes use resource-oriented nouns and explicit action routes only for state transitions such as `start` and `complete`.

## OOP
- Keep controllers thin.
- Keep business rules in application services.
- Prefer composition and dependency injection over static/global state.
- Do not expose EF Core entities as the public API contract when a DTO is appropriate.
- Keep authorization policy centralized.

## Angular
- Prefer standalone components and lazy routes.
- Keep API calls in services, not templates.
- Keep reusable query/filter state in shared services/models.
- Use signals for local reactive UI state where appropriate.
- Avoid duplicating filter and API mapping logic between Board and Backlog.

## Backend
- Use cancellation tokens on asynchronous data access where practical.
- Use `AsNoTracking()` for read-only EF Core queries.
- Push project-wide aggregates into SQL/EF queries.
- Validate authorization before returning project-scoped data.
- Map exceptions to consistent ProblemDetails responses.

## Git
Use Conventional Commit style:
- `feat:` new capability
- `fix:` defect correction
- `refactor:` structural change without behavior change
- `test:` tests
- `docs:` documentation
- `chore:` maintenance

Keep commits focused and explain the business/technical reason when it is not obvious.

## UI
Tailwind/daisyUI classes are presentation concerns. Do not embed business decisions in styling logic. Ensure layouts remain usable at mobile, tablet and desktop widths.
