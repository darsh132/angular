# ADR-005: Server-Side Analytics

- Status: Accepted
- Date: 2026-08-25

## Context
Project-wide dashboards and sprint analytics can require large issue sets. Calculating metrics from whatever data happens to be loaded in a browser risks incorrect or incomplete results.

## Decision
Calculate authoritative sprint and project aggregates in backend application services using EF Core database-side queries, then expose purpose-built response DTOs.

## Consequences
- Consistent metrics across clients.
- Less data transferred to browsers.
- Better scalability for reporting.
- Analytics logic requires service-level regression tests.
