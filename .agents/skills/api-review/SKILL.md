---
name: api-review
description: Review a REST endpoint in this project for correctness and consistency with our conventions. Use when a new or changed controller/endpoint needs a quality pass before commit.
---

# API Review

Review the specified REST endpoint(s) against the standards below. Report findings as a short, prioritised list with specific file/line references and a suggested fix for each.

## What to check

1. **HTTP semantics** — correct verb (GET for reads, POST for creates, etc.) and correct status codes (200 for success, 404 when a resource is missing, 400 for invalid input). Reads must be side-effect free.
2. **Layering** — the controller must stay thin. Flag any business logic or data access that leaked into the controller instead of living in a service. This is the most important check.
3. **Contract** — the endpoint returns a DTO, not the domain entity directly. New fields on the contract are intentional.
4. **Validation & errors** — invalid input is handled and returns a clear error, not an unhandled exception.
5. **Naming & consistency** — route, method, and DTO names match the conventions in AGENTS.md and the rest of the codebase.

## Output

Report only issues that affect **correctness or our conventions**. Give each a severity (blocker / should-fix / optional) and a concrete fix.

## Gotchas

- **Don't flag style preferences** (brace placement, var vs explicit type, ordering) — those are not review findings here.
- **Don't invent problems.** A reviewer asked to find issues will usually report some even when the code is sound; if the endpoint is clean, say so.
- The single most common real issue in this codebase is **business logic creeping into a controller** — check that first and specifically (e.g. a commission calculation inlined in the controller instead of delegated to `CommissionService`).
- Rounding/precision on money: confirm monetary math goes through the service, not the controller or the frontend.
