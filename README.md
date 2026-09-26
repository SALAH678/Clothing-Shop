# Clothing Shop

Full-stack e-commerce application for a clothing store. The backend is a **.NET 10 Clean Architecture** solution exposing a versioned REST API; the frontend is a **React 19 + TypeScript** single-page application.

---

## Overview

Clothing Shop is a complete online store for a clothing retailer operating in the Algerian market: prices are in Algerian dinars (DZD), orders carry a _wilaya_ (province) shipping address, and online payments are processed through **Chargily Pay**, the Algerian payment gateway.

The repository contains two independently runnable applications and the infrastructure that wires them together:

| Path           | Description                                                                                                    |
| -------------- | -------------------------------------------------------------------------------------------------------------- |
| `backend/`     | .NET 10 solution — `Domain`, `Application`, `Infrastructure`, `Api` projects plus `test/Application.UnitTests` |
| `frontend/`    | React 19 + TypeScript SPA built with Vite and Tailwind CSS 4                                                   |
| `compose.yaml` | Docker Compose stack: PostgreSQL, Seq, the API, an ngrok tunnel and the frontend dev server                    |
| `docs/`        | Project documentation — architecture, API reference, operations and more                                       |

**What a visitor can do:** browse collections → open a product → pick a size/colour variant → add to bag or buy now → check out with a shipping address → pay through Chargily → land on a success or failure page and retry a failed payment.

**What an administrator can do:** sign in with an Admin account and manage the catalogue (categories and sub-categories, products with variants and images), review purchases, read aggregate dashboard statistics and create user accounts.

> Demo credentials for a seeded database are listed under [Testing the Application](#testing-the-application).

## Features

### Storefront

- **Home page** — hero, featured collections (fed by the live categories endpoint), store features and store location.
- **Category browsing** — a categories overview grid plus a per-collection product grid.
- **Product filtering & search** — filter by size, colour and price range; debounced text search; sort by newest or price; "Load more" infinite pagination (capped at 5 pages / 60 products, after which the user is asked to refine the filters).
- **Product details** — image gallery with a main image, variant selection by size and colour, stock-aware quantity selector, **Add to Bag** and **Buy Now**.
- **Cart drawer** — server-backed cart with add, remove and quantity update, a live item-count badge and a running total.

### Checkout & payments

- Checkout from the **cart** (`origin = Cart`) or directly from a product (**Buy Now**, `origin = BuyNow`).
- Shipping details capture phone, wilaya, city and street.
- **Server-side pricing** — unit prices are snapshotted from the catalogue and never trusted from the client.
- **Chargily Pay** hosted checkout; the API creates the checkout and returns a redirect URL.
- **Webhook-driven status updates** — a signature-validated webhook marks the payment `Paid` or `Failed`, clears the cart for a cart purchase, restores stock on failure and emails the store owner a purchase notification.
- **Retry payment** for a failed purchase (re-validates stock and creates a new Chargily checkout).
- Success and failure result pages, plus a resumable checkout after a refresh.

### Accounts & authentication

- Email + password registration with a **6-digit verification code** (5-minute expiry, delivered by email).
- **Google OAuth** sign-in and sign-up — `@react-oauth/google` on the client, ID-token validation on the server.
- JWT access tokens (15 minutes) with **silent refresh**: the refresh token is an httpOnly cookie scoped to `/api/auth`, and the access token is kept in memory only.
- Forgot/reset password through an emailed code; resetting a password revokes every existing refresh token.
- Profile page to view and update first name, last name and phone number.

### Administration

- **Dashboard overview** — totals for purchases, products, users and categories.
- **Users** — paginated list and account creation.
- **Products** — paginated list with search/price/sort filters, creation with variants and images (multipart upload), update and delete.
- **Variants** — create, update and delete per product.
- **Categories** — create, update, delete and assign/unassign sub-categories.
- **Purchases** — paginated list with customer and item details.

### Platform & operations

- **Rate limiting** — eight named policies plus two independently keyed limiters, with `Retry-After` surfaced as a live countdown in the UI.
- **RFC 7807 Problem Details** — every domain error maps to a consistent HTTP status and payload.
- **Structured logging** — Serilog to console and [Seq](https://datalust.co/seq), including request logging.
- **Health checks** — `/health` reports database connectivity and background-job heartbeats.
- **Background jobs** — [TickerQ](https://tickerq.net/) schedules verification/reset emails, purchase notifications and orphaned-image cleanup.
- **Image storage** — validated uploads (`.jpg`, `.jpeg`, `.png`, `.webp`, max 5 MB) stored on disk with GUID filenames and served from `/images`.
- **API documentation** — FastEndpoints Swagger UI in Development, with versioning through the `X-Api-Version` header.

## Tech Stack

### Backend

| Area             | Technology                                         | Version         |
| ---------------- | -------------------------------------------------- | --------------- |
| Runtime          | .NET / ASP.NET Core                                | 10.0            |
| Endpoints        | FastEndpoints (+ AspVersioning, Security, Swagger) | 8.3.0           |
| CQRS / messaging | MediatR                                            | 14.2.0          |
| Validation       | FluentValidation                                   | 12.1.1          |
| Object mapping   | AutoMapper                                         | 16.2.0          |
| Data access      | EF Core                                            | 10.0.11         |
| Database         | PostgreSQL via Npgsql                              | 10.0.3          |
| Password hashing | BCrypt.Net-Next                                    | 4.2.0           |
| Authentication   | Microsoft.AspNetCore.Authentication.JwtBearer      | 10.0.11         |
| Google sign-in   | Google.Apis.Auth                                   | 1.76.0          |
| Payments         | Chargily.Pay / Chargily.Pay.AspNet                 | 2.0.3 / 2.0.1   |
| Email            | FluentEmail.Core / FluentEmail.Smtp                | 3.0.2           |
| Background jobs  | TickerQ                                            | 10.4.0          |
| Logging          | Serilog.AspNetCore, Serilog.Sinks.Seq              | 10.0.0 / 9.1.0  |
| Health checks    | AspNetCore.HealthChecks.\* + EF Core health checks | 9.0.0 / 10.0.11 |
| API docs         | Swashbuckle.AspNetCore                             | 10.1.1          |

### Frontend

| Area              | Technology                           | Version     |
| ----------------- | ------------------------------------ | ----------- |
| UI library        | React / React DOM                    | 19.2        |
| Language          | TypeScript                           | ~6.0        |
| Build tool        | Vite (+ `@vitejs/plugin-react`)      | 8.2         |
| Styling           | Tailwind CSS (+ `@tailwindcss/vite`) | 4.3         |
| Routing           | React Router DOM                     | 7.18        |
| Server state      | TanStack React Query                 | 5.102       |
| HTTP client       | axios                                | 1.20        |
| Forms             | react-hook-form                      | 7.86        |
| Schema validation | zod                                  | 4.4         |
| Google sign-in    | @react-oauth/google                  | 0.13        |
| Icons             | lucide-react                         | 1.39        |
| Linting           | ESLint + typescript-eslint           | 10.x / 8.65 |

### Testing

| Area           | Technology         | Version |
| -------------- | ------------------ | ------- |
| Test framework | xunit.v3           | 4.0.1   |
| Mocking        | Moq                | 4.20.72 |
| Assertions     | FluentAssertions   | 8.11.0  |
| Coverage       | coverlet.collector | 10.0.1  |

### Infrastructure

Docker & Docker Compose · PostgreSQL 18 (alpine) · Seq · ngrok · mkcert (local HTTPS)

## Prerequisites

The application is run **entirely with Docker Compose** — the database, logging backend, API, tunnel and frontend all run as containers. The host therefore only needs the tooling required to generate local certificates and expose a public webhook URL.

| Requirement                                                       | Why it is needed                                                                                              |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| **Windows**                                                       | The Compose stack mounts the API certificate from `%USERPROFILE%\.aspnet\https`, which is a Windows-only path |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Runs the whole stack (Docker Engine + Compose v2)                                                             |
| [.NET SDK](https://dotnet.microsoft.com/download) **10.0**        | Used once to generate the ASP.NET HTTPS development certificate                                               |
| [mkcert](https://github.com/FiloSottile/mkcert)                   | Generates the local HTTPS certificates the frontend dev server requires                                       |
| An [ngrok](https://ngrok.com/) account                            | Provides the public HTTPS URL that lets Chargily reach the payment webhook                                    |
| A modern browser                                                  | To reach the app, Swagger, Seq and the ngrok inspector                                                        |

PostgreSQL, Node.js, the .NET runtime and every other service run **inside containers** — nothing else needs to be installed on the host.

> **Provided for you:** the Google OAuth client ID, the Chargily **test** API key and the SMTP credentials are committed in `backend/src/Api/appsettings.Development.json`, so Google sign-in, payments and verification emails work without any extra accounts. The values *you* supply are `NGROK_AUTHTOKEN`, `NGROK_DOMAIN`, `PASSWORD` and `SECRET_KEY`.

> **⚠️ About those shared credentials.** The Google client ID is safe to publish — a client ID is a public identifier, not a secret, and it is designed to ship inside browser code. The other two committed credentials are **real credentials**, and "test" does not make them harmless: anyone who can read this repository can send email as the configured SMTP account and create checkouts against the Chargily account. They are committed on purpose so the project works end to end for evaluation. If that trade-off stops being acceptable, move them out of `appsettings.Development.json` into user secrets or environment variables and rotate both the SMTP app password and the Chargily key — anything ever pushed to a public repository should be treated as compromised.

> **🚧 Scope decision — Windows only.** The API certificate is mounted from `%USERPROFILE%\.aspnet\https`, a Windows-only path, so this stack is supported on Windows and documented as such rather than pretending to be portable. Lifting the limitation is a one-line change: mount a project-relative folder instead (`./certs:/https:ro`) and place the generated certificate there, then adjust step 1.

## Running the Application

### Docker Compose

Everything runs as containers: PostgreSQL, Seq (the log server), the API, an ngrok tunnel and the frontend dev server.

#### 0. Clone the repository and create the environment files

```powershell
git clone <repository-url>
cd "Clothing Shop"
```

Neither environment file is committed — both are gitignored — so create them from the examples that ship with the repo:

```powershell
Copy-Item .env.example .env
Copy-Item frontend/.env.example frontend/.env
```

You fill in four values in `.env` at step 3. `frontend/.env` needs no editing: it already points at `https://localhost:7146` and carries the public Google client ID.

#### 1. Generate the ASP.NET HTTPS certificate

The API serves HTTPS from a certificate that Compose mounts into the container, and that certificate is protected by a password. Choose a password now — it is the `PASSWORD` value you write into `.env` at step 3.

In PowerShell:

```powershell
dotnet dev-certs https --trust
dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\aspnetapp.pfx" -p <your-password>
```

`--trust` makes `https://localhost:7146` trusted by your browser. Run it once per machine; it shows a confirmation prompt.

#### 2. Generate the frontend HTTPS certificates

The frontend dev server must serve HTTPS from the exact origin `https://localhost:5173`, because that is the only origin the API's CORS policy allows.

```powershell
mkcert -install
mkcert localhost 127.0.0.1 ::1
```

This writes `localhost+2.pem` and `localhost+2-key.pem` into the current folder — move both into `frontend/`.

> Certificates are machine-specific: only the machine that generated them trusts them, so every user generates their own pair.

#### 3. Fill in the environment files

Open the `.env` you created in step 0. Only four values are required — everything else is pre-filled with a working default.

| Variable                              | Value                                                                                                      |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `PASSWORD`                            | The certificate password you chose in step 1. It must match **exactly**                                    |
| `NGROK_AUTHTOKEN`                     | Your ngrok authtoken, from the ngrok dashboard                                                             |
| `NGROK_DOMAIN`                        | Your reserved ngrok domain — **hostname only**, without `https://` (for example `my-store.ngrok-free.app`) |
| `SECRET_KEY`                          | A long random string (min. 32 chars) that signs JWT access tokens — generate with `openssl rand -base64 64`, or in PowerShell `[Convert]::ToBase64String((1..64 \| ForEach-Object { Get-Random -Max 256 }))`. Zero signup friction, it just needs to exist and be sufficiently random |
| `DB_USER` / `DB_PASSWORD` / `DB_NAME` | Pre-filled by `.env.example`. The database runs in its own container, so these values only ever apply to it — change them if you like                             |
| `SEQ_FIRSTRUN_ADMINUSERNAME` / `SEQ_FIRSTRUN_ADMINPASSWORD` | Pre-filled by `.env.example`. Initial admin credentials for the local Seq web interface                                             |
| `EMAILADMIN`                          | Pre-filled with an example recipient for purchase-notification emails                                      |

`frontend/.env` needs no editing — it is copied from `frontend/.env.example`, which already sets `VITE_API_URL=https://localhost:7146` and the public Google client ID.

> A reserved ngrok domain can only be bound by one agent at a time, so two people cannot share the same `NGROK_DOMAIN` — which is why `.env.example` ships a placeholder instead of a working value.

#### 4. Start the stack

```powershell
docker compose up --build -d
```

The **first run takes a few minutes**: before it accepts requests, the API applies its EF Core migrations and then seeds a full demo catalogue from the snapshot embedded in `ApplicationDbContextInitialiser` (`backend/src/Infrastructure/Data/`). The frontend container comes up before the API is ready, so a connection error immediately after `up` is expected.

Follow the API's progress with:

```powershell
docker compose logs -f api
```

#### 5. Open the application

| What            | URL                            |
| --------------- | ------------------------------ |
| Storefront      | https://localhost:5173         |
| Swagger UI      | https://localhost:7146/swagger |
| API health      | https://localhost:7146/health  |
| Seq (logs)      | http://localhost:5341          |
| ngrok inspector | http://localhost:4500          |

#### 6. Stopping and resetting

```powershell
docker compose down       # stop the stack, keeping the database and uploaded images
docker compose down -v    # stop and delete the volumes — the next start re-seeds from scratch
```

#### Configuration reference

The API reads its settings from `backend/src/Api/appsettings.Development.json`, and Compose overrides a handful of them:

| Compose override | Effect |
| --- | --- |
| `DB_USER` / `DB_PASSWORD` / `DB_NAME` | Interpolated into both the `db` container's credentials and the API's connection string, so the two can never drift apart |
| `ASPNETCORE_URLS`, `ports` | The API listens on 8080/8081 and is published to the host as `7146 → 8081` |
| `ASPNETCORE_ENVIRONMENT=Development` | Enables Swagger and the CORS policy for `https://localhost:5173` |
| `ConnectionStrings__DefaultConnection` | Points the API at the `db` container |
| `ASPNETCORE_Kestrel__Certificates__Default__*` | Loads the certificate generated in step 1, unlocked with `PASSWORD` |
| `JwtSettings__SecretKey` | Overrides the committed value with your generated `SECRET_KEY`, so JWT signing/validation is unique per deployment |
| `Serilog__WriteTo__*` | Ships logs to the `seq` container |
| `Email__EmailAdmin` | Overrides the purchase-notification recipient with `EMAILADMIN` |
| `Chargily__WebhookEndpointUrl` | Points the payment webhook at your ngrok domain |

#### Troubleshooting

| Symptom                                          | Likely cause                                                                                                                 |
| ------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| The `api` container restarts in a loop           | The certificate from step 1 is missing, or `PASSWORD` in `.env` does not match the password you used                         |
| The browser reports `ERR_CERT_AUTHORITY_INVALID` | The mkcert certificates were generated without `mkcert -install`, or they were copied from another machine — regenerate them |
| The `ngrok` container exits immediately          | `NGROK_AUTHTOKEN` is invalid, or `NGROK_DOMAIN` is not reserved on that ngrok account                                        |
| `docker compose logs api` prints nothing         | Known limitation — the Compose file overwrites Serilog's Console sink. Read the logs in Seq instead                          |
| `http://localhost:8080` does not respond         | Expected — HTTPS redirection targets port 8081, which is not published. Use `https://localhost:7146`                         |
| A payment never confirms                         | The webhook is unreachable — check the ngrok inspector at http://localhost:4500 for failed requests                          |

## Testing the Application

### Demo accounts

A freshly seeded database contains synthetic demo accounts (no real personal data):

| Email                     | Password    | Role     |
| ------------------------- | ----------- | -------- |
| `demo.admin@gmail.com`    | `Demo1234!` | Admin    |
| `demo.customer@gmail.com` | `Demo1234!` | Customer |

An Admin account unlocks the `/admin` panel; a Customer account exercises the storefront, cart and checkout flows.

### Backend unit tests

```bash
cd backend
dotnet test
```

The test project (`test/Application.UnitTests`) uses xUnit v3, Moq and FluentAssertions, and currently covers the external-auth registration and payment-webhook handlers.

### Frontend checks

These require **Node.js 22+** on the host — the application itself does not, since it runs in a container:

```bash
cd frontend
npm install
npm run lint     # ESLint
npm run build    # tsc -b && vite build — type-checks the whole app
```

The frontend has **no automated test runner configured** (no Vitest or Jest dependency); `lint` and `build` are the available automated checks.

### Manual API testing

- **`backend/src/Api/requests.http`** — a REST Client file for VS Code / Visual Studio with ready-made login and refresh requests.
- **Swagger UI** at `https://localhost:7146/swagger` while running in Development — the quickest way to explore the endpoints with an authenticated session (`POST /api/auth/login` issues the refresh cookie that the other requests then reuse).

### Verifying the payment flow

Payments depend on an externally reachable webhook, and Chargily cannot call `localhost`. The Compose stack therefore publishes the API through an ngrok tunnel and points `Chargily:WebhookEndpointUrl` at it — see [step 3](#3-fill-in-the-environment-files).

To exercise a full payment: sign in, add an item to the bag, complete checkout with an Algerian phone number and address, then pay on the Chargily page. The purchase is only confirmed when Chargily calls the webhook, so monitor it at http://localhost:4500 (ngrok inspector) and in Seq.

### Where to go next

| Document | Contents                                                                                                      |
| -------- | ------------------------------------------------------------------------------------------------------------- |
| `docs/`  | Full documentation set — architecture, backend layers, API reference, frontend guide, security and operations |

---

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.
