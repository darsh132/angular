# Testing Strategy

## Test pyramid

```text
              E2E / UI
             /        \
        Component     API
          /             \
      Application       Integration
             \         /
              Domain
```

## Backend

- Unit tests: pure workflow/domain invariants.
- Application tests: use-case validation, audit behavior and transactions.
- Integration tests: SQLite provider, EF relationships and migration application.

## Migration test

`MigrationTests` creates a fresh in-memory SQLite database and executes `Database.MigrateAsync()`. This verifies that the committed migration chain can construct a new schema independently of `EnsureCreated()`.

## Acceptance criteria

A feature is not considered complete until:

1. Its domain/application rules have tests.
2. Persistence behavior has an integration test when database semantics matter.
3. The migration chain can create a clean database.
4. CI build/test gates are green.
5. Functional and technical documentation describes the behavior.

## Concurrency

Issue-number allocation is transactionally tested against SQLite. Additional load/concurrency testing should be executed with the target production database if the persistence provider changes.
