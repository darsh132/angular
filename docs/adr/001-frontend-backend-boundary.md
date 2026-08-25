# ADR-001: Angular and ASP.NET Core Boundary

- Status: Accepted
- Date: 2026-08-25

## Context
The product needs a maintainable SPA frontend and a separately testable HTTP backend.

## Decision
Angular owns presentation, navigation, local UI state and API consumption. ASP.NET Core owns authentication, authorization, business workflows and persistence.

## Consequences
- Clear ownership of responsibilities.
- Backend can be tested without a browser.
- Frontend can evolve independently of persistence implementation.
- API contracts must be maintained deliberately.
