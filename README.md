# Invoice-to-Cash — Demo

A small full-stack demo in the **Invoice-to-Cash** domain, built to show a clean **.NET 10 + React** stack and an **AI-assisted engineering workflow**.

- **Backend:** .NET 10 Web API — layered (`Controllers → Services → Data`), DI, Swagger, xUnit tests.
- **Frontend:** React + Vite + TypeScript — a commission console that consumes the API through a typed client.

## Architecture

```
backend/
  Controllers/   InvoicesController        (thin HTTP layer)
  Services/      CommissionService         (business logic — single source of truth)
  Data/          InMemoryInvoiceRepository (seeded sample data; swap for EF/Cosmos in prod)
  Models/        Invoice, InvoiceStatus
backend.Tests/   CommissionServiceTests    (xUnit)
frontend/        React + Vite + TS         (experience layer)
```

The frontend talks to the API over REST. Types in `frontend/src/api.ts` mirror the backend DTOs (in a production system these would be generated from Swagger, as in Squizzu).

## Business rule

Commission = **2.5% of Amount** for `Paid` invoices, `0` otherwise (rounded to 2 dp). It lives in exactly one place: `CommissionService.CalculateCommission`.

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/invoices` | All invoices, each with its commission |
| GET | `/invoices/overdue` | Only overdue invoices |
| GET | `/invoices/commission-total` | Total commission across paid invoices |

## Running it

**Backend** (from `/backend`):
```bash
dotnet run
# http://localhost:5088  ·  Swagger at /swagger
```

**Tests** (from repo root):
```bash
dotnet test
```

**Frontend** (from `/frontend`):
```bash
npm install
npm run dev
# http://localhost:5173  (proxies /api -> http://localhost:5088)
```

Start the backend first, then the frontend.

## AI-assisted development

This repo is set up to be built and maintained with **Claude Code**, using a deliberate workflow rather than one-shot prompting:

- **`CLAUDE.md`** — lean, scoped project context (conventions, the business rule, gotchas) that the agent reads each session.
- **`.claude/skills/api-review/`** — a reusable review skill that checks endpoints for HTTP semantics and, most importantly, that no business logic leaks into controllers. It has a *gotchas* section telling the reviewer to report only real issues, not style nitpicks.
- **`.claude/agents/test-generator.md`** — a subagent that writes and runs xUnit tests in its own isolated context, with a minimal tool allowlist (least-privilege — no write access to production code) and a requirement to show the actual test output as evidence.

Workflow principle: **explore → plan → code → verify.** Changes go through plan mode first; tests must pass before commit, and the agent shows the test output rather than asserting success.
