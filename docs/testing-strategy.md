# Testing Strategy

## Test pyramid

```text
        E2E / UI
       /       \
   API integration
      /       \
   Unit / domain
```

### Unit tests
Focus on deterministic business rules and application services. Sprint analytics tests cover empty scope, completed/remaining points and project isolation.

### Integration tests
Use an isolated SQLite database for EF Core persistence and verify controller/application-service authorization, constraints and HTTP contracts.

### Frontend tests
Test components for rendering states, filter behavior, URL synchronization, optimistic rollback and permission-aware actions.

### E2E tests
Cover critical journeys:
1. Login.
2. Open dashboard.
3. Filter board.
4. Create issue.
5. Move issue.
6. Plan sprint.
7. Start/complete sprint as manager.
8. Verify analytics.

## Regression priorities
Security and authorization tests have priority over visual tests. Analytics calculations must be tested at the service layer so UI refactoring cannot change business semantics.

## Test data
Tests should use deterministic fixtures/builders and avoid depending on production data or network services.

## Definition of test completion
A story with business logic is not complete until its primary success path, authorization behavior and relevant edge cases are covered.
