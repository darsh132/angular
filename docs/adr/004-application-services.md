# ADR-004: Application-Service Layer

- Status: Accepted
- Date: 2026-08-25

## Context
Putting workflow rules directly in controllers or EF Core entities would make HTTP, business and persistence concerns tightly coupled.

## Decision
Use focused application services for issue workflows, sprint workflows, analytics and dashboard aggregation. Controllers remain thin HTTP adapters.

## Consequences
- Business workflows are independently testable.
- Controllers remain predictable.
- Services must avoid becoming generic god-services; each service should have a coherent responsibility.
