# ADR-002: EF Core and SQLite

- Status: Accepted
- Date: 2026-08-25

## Context
The clone needs a lightweight relational database with real relational constraints and a straightforward local developer experience.

## Decision
Use Entity Framework Core as the ORM and SQLite 3 as the default persistence provider.

## Consequences
- Minimal local infrastructure.
- Real relational schema, foreign keys, migrations and transactions.
- Database-side aggregation remains available.
- SQLite has concurrency and operational limits; a server database may be appropriate at higher scale.
