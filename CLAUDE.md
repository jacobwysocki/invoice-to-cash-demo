# Invoice-to-Cash — Demo

Small demo in the Invoice-to-Cash domain: a **.NET 10 Web API** (`/backend`) and a **React + Vite + TypeScript** frontend (`/frontend`). The API exposes invoices and the commission they generate; the frontend is the experience layer consuming that API.

## Architecture

- **Layering (backend):** `Controllers` → `Services` → `Data`. Controllers are thin; they only translate HTTP ↔ service calls.
- **Business logic** lives in the service layer. The commission rule lives **only** in `CommissionService`.
- **DTOs** (`InvoiceDto`, `CommissionTotalDto`) are the API contract, decoupled from the `Invoice` domain entity.
- **Frontend** calls the API through a typed client in `src/api.ts` (in a real project these types are generated from Swagger).

## Business rule

Commission = **2.5% of Amount** for `Paid` invoices, **0** otherwise. Rounded to 2 decimals. Single source of truth: `CommissionService.CalculateCommission`.

## Run

```bash
# Backend (from /backend)
dotnet run                      # serves http://localhost:5088, Swagger at /swagger

# Tests (from repo root)
dotnet test

# Frontend (from /frontend)
npm install && npm run dev      # serves http://localhost:5173, proxies /api -> backend
```

## Conventions

- C#: file-scoped namespaces, `record` for immutable models/DTOs, constructor injection.
- Endpoints return `ActionResult<T>` with correct status codes and verbs.
- React: functional components, hooks, typed props; handle loading and error states.

## Gotchas

- **Keep the commission rule in one place** (`CommissionService`). Do not duplicate the 2.5% anywhere else, and never put it in a controller.
- **Controllers stay thin** — no business logic, no data access beyond calling a service.
- The frontend proxies `/api` to the backend via `vite.config.ts`; if requests 404, check the backend is on port 5088.

## Workflow

Changes should go through **plan mode first** (explore → plan → code). Tests must pass before commit — show the test output as evidence, don't assert success.
