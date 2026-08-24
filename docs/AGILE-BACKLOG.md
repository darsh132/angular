# Agile Backlog

## Epic 1 — Foundation

### US-001 — Working vertical slice
**As a** developer, **I want** Angular, .NET 8, EF Core and SQLite integrated **so that** the application demonstrates an end-to-end architecture.

**Story points:** 5

### US-002 — Responsive design system
**As a** user, **I want** responsive Tailwind/daisyUI screens and switchable themes **so that** the application is usable across devices.

**Story points:** 3

## Epic 2 — Issue management

### US-101 — Scrum board
**As a** developer, **I want** issues grouped by workflow state **so that** I can understand work in progress.

**Story points:** 5

### US-102 — Search issues
**As a** team member, **I want** server-side search **so that** I can find issues quickly.

**Story points:** 3

### US-103 — Transition issue
**As a** developer, **I want** to move an issue between workflow states **so that** board state reflects delivery progress.

**Story points:** 5

### US-104 — Create issue
**As a** product owner, **I want** to create issues **so that** new work can enter the backlog.

**Story points:** 5

## Epic 3 — Security and collaboration

### US-201 — Secure authentication
Replace demo authentication with Identity/JWT and project authorization.

**Story points:** 8

### US-202 — Comments and activity
Add comments, activity history and audit events to issue details.

**Story points:** 8

## Sprint 1 goal
Deliver a working vertical slice: API → SQLite → Angular board → search → persisted status transition.

## Scrum operating model
- Sprint Planning: select stories against capacity and define a measurable sprint goal.
- Daily Scrum: progress, next step, blocker.
- Sprint Review: demonstrate working software only.
- Retrospective: capture one actionable process improvement.
- Backlog Refinement: clarify acceptance criteria, dependencies and estimates.

## Engineering gates
A story is complete only when acceptance criteria pass, code is reviewable, tests/build pass, documentation is current and no known critical defect remains.
