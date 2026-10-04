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
| **Windows, macOS or Linux**                                        | The stack runs on all three. Exactly one setting is OS-specific: Compose mounts the API certificate from `${USERPROFILE}/.aspnet/https`, so on macOS and Linux you export `USERPROFILE=$HOME` before starting the stack (see [step 1](#1-generate-the-aspnet-https-certificate)) |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) — or Docker Engine with the Compose v2 plugin | Runs the whole stack (Docker Engine + Compose v2)                                                             |
| [.NET SDK](https://dotnet.microsoft.com/download) **10.0**        | Used once to generate the ASP.NET HTTPS development certificate                                               |
| [mkcert](https://github.com/FiloSottile/mkcert)                   | Generates the local HTTPS certificates the frontend dev server requires                                       |
| An [ngrok](https://ngrok.com/) account                            | Provides the public HTTPS URL that lets Chargily reach the payment webhook                                    |
| A modern browser                                                  | To reach the app, Swagger, Seq and the ngrok inspector                                                        |

PostgreSQL, Node.js, the .NET runtime and every other service run **inside containers** — nothing else needs to be installed on the host.

> **Provided for you:** the Google OAuth client ID, the Chargily **test** API key and the SMTP credentials are committed in `backend/src/Api/appsettings.Development.json`, so Google sign-in, payments and verification emails work without any extra accounts. The values *you* supply are `NGROK_AUTHTOKEN`, `NGROK_DOMAIN`, `PASSWORD` and `SECRET_KEY`.

> **⚠️ About those shared credentials.** The Google client ID is safe to publish — a client ID is a public identifier, not a secret, and it is designed to ship inside browser code. The other two committed credentials are **real credentials**, and "test" does not make them harmless: anyone who can read this repository can send email as the configured SMTP account and create checkouts against the Chargily account. They are committed on purpose so the project works end to end for evaluation. If that trade-off stops being acceptable, move them out of `appsettings.Development.json` into user secrets or environment variables and rotate both the SMTP app password and the Chargily key — anything ever pushed to a public repository should be treated as compromised.

> **Portability note — one variable decides the OS support.** The API certificate reaches the container through the Compose bind mount `${USERPROFILE}/.aspnet/https:/https:ro`. `USERPROFILE` is a Windows variable, so on macOS and Linux set it to your home directory before `docker compose up`:
>
> ```bash
> export USERPROFILE="$HOME"      # add to ~/.zshrc or ~/.bashrc to make it permanent
> ```
>
> That single export is what removes the Windows-only limitation — everything else in this README already works unchanged on macOS and Linux. Two alternatives avoid the variable altogether: edit `compose.yaml` to mount `${HOME}/.aspnet/https:/https:ro` (macOS/Linux) or `./certs:/https:ro` (project-relative, works everywhere, with the certificate placed in `./certs`).

## Running the Application

### Docker Compose

Everything runs as containers: PostgreSQL, Seq (the log server), the API, an ngrok tunnel and the frontend dev server.

#### 0. Clone the repository and create the environment files

The clone is the same on every platform:

```bash
git clone <repository-url>
cd "Clothing Shop"
```

Neither environment file is committed — both are gitignored — so create them from the examples that ship with the repo:

```powershell
# Windows (PowerShell)
Copy-Item .env.example .env
Copy-Item frontend/.env.example frontend/.env
```

```bash
# macOS / Linux (bash or zsh)
cp .env.example .env
cp frontend/.env.example frontend/.env
```

You fill in four values in `.env` at step 3. `frontend/.env` needs no editing: it already points at `https://localhost:7146` and carries the public Google client ID.

#### 1. Generate the ASP.NET HTTPS certificate

The API serves HTTPS from a certificate that Compose mounts into the container, and that certificate is protected by a password. Choose a password now — it is the `PASSWORD` value you write into `.env` at step 3. Where the certificate is written is the one OS-specific detail of the whole setup, because the Compose mount reads it from `$USERPROFILE/.aspnet/https` on every platform.

**Windows (PowerShell):**

```powershell
dotnet dev-certs https --trust
dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\aspnetapp.pfx" -p <your-password>
```

**macOS (bash or zsh):**

```bash
dotnet dev-certs https --trust
dotnet dev-certs https -ep "$HOME/.aspnet/https/aspnetapp.pfx" -p <your-password>
export USERPROFILE="$HOME"      # only needed if you did not change the mount in compose.yaml
```

On macOS, `--trust` adds the certificate to your login keychain and asks for your keychain password.

**Linux (bash or zsh):**

```bash
mkdir -p "$HOME/.aspnet/https"
dotnet dev-certs https -ep "$HOME/.aspnet/https/aspnetapp.pfx" -p <your-password>
export USERPROFILE="$HOME"      # only needed if you did not change the mount in compose.yaml
```

`.NET` does **not** support `dotnet dev-certs https --trust` on Linux — the command prints a warning and the certificate is created but left untrusted. The stack still runs, because the container only needs the `.pfx` to serve HTTPS; the browser will simply warn on `https://localhost:7146` until you trust it yourself:

```bash
# Optional: system-wide trust (Debian/Ubuntu) to silence the browser warning on the API origin
sudo dotnet dev-certs https -ep /usr/local/share/ca-certificates/aspnet-dev.crt --format PEM
sudo update-ca-certificates
```

Chrome and Firefox on Linux read their own NSS database, so they may still warn even after the system store is updated. Install `libnss3-tools` and import the certificate there too:

```bash
dotnet dev-certs https -ep "$HOME/.aspnet/https/aspnet-dev.pem" --format PEM
certutil -d "sql:$HOME/.pki/nssdb" -A -t "C,," -n "ASP.NET dev" -i "$HOME/.aspnet/https/aspnet-dev.pem"
```

Run `--trust` (Windows/macOS) or the trust steps above (Linux) once per machine. The password you pass to `-p` must match `PASSWORD` in `.env` **exactly** on every platform, or the `api` container restarts in a loop.

#### 2. Generate the frontend HTTPS certificates

The frontend dev server must serve HTTPS from the exact origin `https://localhost:5173`, because that is the only origin the API's CORS policy allows. The commands are identical on all three platforms:

```bash
mkcert -install
mkcert localhost 127.0.0.1 ::1
```

This writes `localhost+2.pem` and `localhost+2-key.pem` into the current folder — move both into `frontend/` (the copy commands in step 0 show the PowerShell and bash forms).

| OS | What `mkcert -install` does |
| --- | --- |
| Windows | Adds the local CA to the Windows trust store; no elevation required |
| macOS | Adds it to your login keychain and asks for the keychain password the first time |
| Linux | Adds it to the system and NSS stores. Install the NSS tools first — `sudo apt install libnss3-tools` (Debian/Ubuntu) or `sudo dnf install nss-tools` (Fedora) — otherwise Chrome and Firefox will not trust the frontend origin |

> Certificates are machine-specific: only the machine that generated them trusts them, so every user generates their own pair. Note that these are a **second** certificate authority, separate from the ASP.NET development certificate in step 1: `mkcert` covers the frontend origin `https://localhost:5173`, while the dev certificate covers the API origin `https://localhost:7146`.

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
| `docs/`    | Full documentation set — architecture, backend layers, API reference, frontend guide, security and operations |
| `images/`  | UI screenshots of every page — storefront (home, categories, product details), auth flows, customer pages (profile, checkout), and the admin panel tabs |

---

## Clean Up and Uninstall

Removing everything this project put on your machine is straightforward, because the application installs nothing permanently: PostgreSQL, Seq, the API, the tunnel and the frontend all run as containers. The steps below go from the disposable parts to the host tools, and every command is run from the repository root.

### 1. Remove the stack, its volumes and its images

```powershell
docker compose down -v --rmi all --remove-orphans
docker builder prune -f     # also clears the BuildKit cache left by the .NET and Node builds
```

| Removed    | What it was                                                                                                                                                                   |
| ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Containers | `clothing_shop_db`, `clothing_shop_seq`, `clothing_shop_api`, `clothing_shop_ngrok`, `clothing_shop_app`                                                                       |
| Volumes    | `clothingshop_db_data` (the seeded catalogue and demo accounts), `clothingshop_seq_data` (log history), `clothingshop_images` (uploaded product and category images)             |
| Images     | The two built here — `clothing_shop_api:1.0`, `clothing_shop_app:1.0` — plus the pulled ones: `postgres:18-alpine`, `datalust/seq:latest`, `ngrok/ngrok:latest`, `node:22-alpine` |

Volume names carry the Compose project name, which is derived from the folder name, so a checkout in `Clothing Shop` produces `clothingshop_*`. If you cloned into a differently named folder, run `docker volume ls` and remove by name instead. If `docker images` still shows a build-only base image such as `mcr.microsoft.com/dotnet/sdk:10.0` — pulled by the API's Dockerfile rather than by a running service — remove it by name with `docker rmi`.

> **⚠️ Do not "be thorough" with `docker system prune -a --volumes`.** It deletes every unused container, image, network **and volume** on the machine, including data belonging to unrelated projects. `docker compose down -v --rmi all` removes only what this stack created.

### 2. Delete the files the setup generated

```powershell
# Windows (PowerShell)
dotnet dev-certs https --clean                    # removes %USERPROFILE%\.aspnet\https\aspnetapp.pfx
mkcert -uninstall                                 # removes the local CA that signed the frontend certificates
Remove-Item .\frontend\localhost+2.pem, .\frontend\localhost+2-key.pem
Remove-Item .env, .\frontend\.env                 # your ngrok token, JWT key and certificate password
Remove-Item -Recurse -Force .\frontend\node_modules, .\frontend\dist, .\frontend\.vite
Get-ChildItem .\backend -Recurse -Directory -Include bin, obj | Remove-Item -Recurse -Force
```

```bash
# macOS / Linux (bash or zsh)
dotnet dev-certs https --clean                    # removes ~/.aspnet/https/aspnetapp.pfx
mkcert -uninstall                                 # removes the local CA that signed the frontend certificates
rm -f frontend/localhost+2.pem frontend/localhost+2-key.pem
rm -f .env frontend/.env                          # your ngrok token, JWT key and certificate password
rm -rf frontend/node_modules frontend/dist frontend/.vite
find backend -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
```

| Item                                                     | Created by                                                                            |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| `%USERPROFILE%\.aspnet\https\aspnetapp.pfx` (Windows) / `~/.aspnet/https/aspnetapp.pfx` (macOS, Linux) | Setup step 1 (`dotnet dev-certs`); mounted into the API container |
| `frontend/localhost+2.pem`, `frontend/localhost+2-key.pem` | Setup step 2 (mkcert); served by the Vite dev server                                   |
| `.env` and `frontend/.env`                                | Setup step 0, copied from the `.env.example` files                                      |
| `frontend/node_modules`, `frontend/dist`, `frontend/.vite` | Only if you ran `npm install`, `npm run build` or `npm run lint` on the host           |
| `backend/**/bin`, `backend/**/obj`                        | Only if you built or tested the solution on the host                                    |

`dotnet dev-certs https --clean` removes **every** ASP.NET development certificate on the machine, including the one setup step 1 trusted with `--trust`, and `mkcert -uninstall` removes a CA that other local projects may also trust — skip either command if something else depends on it.

### 3. Uninstall the host tools

Only four tools ever touch the host, and the project itself installs none of them: Docker Desktop runs the stack, the .NET SDK is used once in setup step 1, mkcert once in setup step 2, and Node.js only for the optional host-side frontend checks.

| Tool                    | Uninstall                                                                                                                                                                                 |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| .NET SDK 10             | `winget uninstall Microsoft.DotNet.SDK.10`, or *Settings → Apps → Installed apps → Microsoft .NET SDK 10.0.401*. If it came with **Visual Studio**, remove it from the Visual Studio Installer instead so VS keeps the component it needs |
| `dotnet-ef` global tool | `dotnet tool uninstall -g dotnet-ef`                                                                                                                                                        |
| Node.js 22+             | `winget uninstall OpenJS.NodeJS`, or *Settings → Apps* — an MSI-installed build is listed as **Node.js**                                                                                     |
| mkcert                  | `winget uninstall FiloSottile.mkcert`; the binary is linked into `%LOCALAPPDATA%\Microsoft\WinGet\Links`                                                                                     |
| Docker Desktop          | `winget uninstall Docker.DockerDesktop`, or run `"C:\Program Files\Docker\Docker\Docker Desktop Installer.exe" uninstall`, then `wsl --unregister docker-desktop` to delete its WSL distribution |

The table above covers Windows. On macOS and Linux the same four tools come from a package manager instead, and the exact names depend on how you installed them:

```bash
# macOS (Homebrew)
brew uninstall --cask docker        # without Docker Desktop: brew uninstall docker colima
brew uninstall dotnet-sdk mkcert node
dotnet tool uninstall -g dotnet-ef

# Debian / Ubuntu (apt)
sudo apt remove dotnet-sdk-10.0 nodejs mkcert
sudo apt remove docker-ce docker-ce-cli containerd.io docker-compose-plugin
dotnet tool uninstall -g dotnet-ef
```

`ngrok` has no host installation on any platform — the stack runs the `ngrok/ngrok` image — and on Windows **do not** unregister the `Ubuntu` WSL distribution, because Docker Desktop did not create it.

### 4. Revoke the credentials and accounts you created

- **ngrok** — delete the reserved domain and rotate the authtoken at [dashboard.ngrok.com](https://dashboard.ngrok.com/); an authtoken is a credential, and a reserved domain can only ever be bound by one agent.
- **Chargily and SMTP** — the Chargily *test* key and the SMTP credentials are committed in `backend/src/Api/appsettings.Development.json`, so rotate anything you supplied yourself.
- **Google** — if you signed in with Google on the storefront, revoke this application at [myaccount.google.com/permissions](https://myaccount.google.com/permissions).
- **Demo accounts** — `demo.admin@gmail.com` and `demo.customer@gmail.com` exist only inside the `clothingshop_db_data` volume, which step 1 already deleted.

### 5. Verify, then delete the folder

```powershell
docker volume ls        # expect no clothingshop_* entries
docker ps -a            # expect no clothing_shop_* containers
dotnet --list-sdks      # empty once the SDK is gone
```

Nothing should answer on `https://localhost:5173`, `https://localhost:7146`, `http://localhost:5341` or `http://localhost:4500`. The last item to remove is the folder itself:

```powershell
# Windows (PowerShell)
cd ..
Remove-Item -Recurse -Force "Clothing Shop"
```

```bash
# macOS / Linux (bash or zsh)
cd ..
rm -rf "Clothing Shop"
```

Nothing from this project then remains: no container, volume, image, certificate, environment file, host tool or credential.

---

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.
