# Security Architecture

## Overview

The Clothing Shop API applies security at multiple boundaries: transport encryption, JWT-based authentication, endpoint authorization, request throttling, strict development CORS, secure refresh-token cookies, and payment-webhook signature validation.

Security is implemented primarily in the .NET backend. The React frontend improves the user experience around sessions and rate-limit responses, but it is not a security boundary: the API must validate every request independently.

> **Scope:** This document describes the security behavior in `backend/src`, with deployment notes based on `compose.yaml`. Secrets must be supplied through environment variables and must never be committed to the repository.

---

## 1. Security Boundaries

| Boundary | Protection | Source of truth |
| --- | --- | --- |
| Browser → API | HTTPS, HSTS, HTTPS redirection | `Api/Program.cs` |
| Cross-origin frontend | Development CORS allow-list | `Api/DependencyInjection.cs` |
| Unauthenticated requests | Anonymous authentication routes and IP/identity rate limits | `Api/Endpoints/Auth/*`, rate-limit policies |
| Authenticated requests | Signed JWT bearer tokens and authorization checks | `Infrastructure/DependencyInjection.cs`, API endpoints |
| Session renewal | Rotating, hashed, seven-day refresh tokens in an `HttpOnly` cookie | Login/refresh handlers and authentication endpoints |
| Payment callbacks | Chargily webhook signature validation | `Api/Program.cs` and Chargily middleware |
| Service-to-service traffic | Private Docker network and database credentials | `compose.yaml`, environment variables |
| Log and telemetry output | `[Sensitive]` redaction of passwords, tokens, codes, phone numbers, and addresses | `Application/Common/Attributes/SensitiveAttribute.cs`, `Application/Common/Behaviours/LoggingBehaviour.cs` |

The browser is treated as untrusted. Hiding an admin page in the frontend is not authorization; protected API operations must reject unauthenticated or unauthorized callers on the server.

---

## 2. HTTPS and Transport Security

The API enables both HSTS and HTTPS redirection in `Api/Program.cs`:

```csharp
app.UseHsts();
app.UseHttpsRedirection();
```

These provide two different protections:

- **HSTS:** tells browsers to use HTTPS for subsequent requests to the API host, reducing plaintext downgrade and certificate-warning bypass opportunities.
- **HTTPS redirection:** redirects ordinary HTTP requests to HTTPS when the host and HTTPS port can be resolved.

The Compose deployment configures Kestrel with an HTTPS certificate:

```yaml
ASPNETCORE_URLS=http://+:8080;https://+:8081
ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=${PASSWORD}
```

The API HTTPS port is published as `https://localhost:7146`. The frontend development server also uses a local certificate and is exposed at `https://localhost:5173`.

### Production requirement

The local development certificate and ngrok URL are development conveniences. A production deployment must use a publicly trusted certificate, terminate HTTPS correctly at the edge or load balancer, and preserve the HTTPS scheme so redirects, secure cookies, and absolute URLs are generated correctly. Do not expose the API over plaintext HTTP to untrusted networks.

### Important proxy assumption

The application does not currently configure `UseForwardedHeaders`. If a reverse proxy or gateway terminates TLS, the deployment must be designed so ASP.NET Core correctly identifies the original HTTPS request. Otherwise, HTTPS redirection, generated URLs, and secure-cookie decisions can be wrong. Configure trusted proxies deliberately rather than accepting forwarded headers from arbitrary networks.


---

## 3. Authentication

### 3.1 JWT bearer authentication

JWT bearer authentication is registered in `Infrastructure/DependencyInjection.cs`. The configured validation parameters are:

- Validate the issuer.
- Validate the audience.
- Validate token lifetime.
- Validate the signing key.
- Use the configured symmetric signing key.
- Set `ClockSkew` to zero.

The API uses JWT bearer tokens for authenticated requests. The frontend stores the access token in memory and attaches it as:

```http
Authorization: Bearer <access-token>
```

The client-side JWT decoder is used only to read non-sensitive UI information such as the role. It does **not** verify the signature. Signature verification is the API's responsibility; a browser-side decoded role is never sufficient for an authorization decision.

### 3.2 Token lifecycle

A successful local login creates an access token valid for **15 minutes** and a refresh token. The access token is returned in the login response for use as a bearer token. The refresh token is persisted in the database as a hash, not as its original plaintext value.

Refresh tokens expire after seven days. During refresh, the current token is revoked and a new token is generated. This provides token rotation and limits the usefulness of a previously issued refresh token.

### 3.3 Refresh-token cookie

The login endpoint writes the refresh token to a cookie with these security properties:

| Cookie setting | Meaning |
| --- | --- |
| `HttpOnly = true` | Browser JavaScript cannot read the refresh token. |
| `Secure = true` | The browser sends it only over HTTPS. |
| `SameSite = None` | Supports the current cross-origin frontend/API deployment. |
| `Path = "/api/auth"` | Limits the cookie to authentication routes. |
| Expiration | Matches the seven-day refresh-token lifetime. |

`SameSite=None` requires HTTPS in modern browsers. The API and frontend must therefore use secure origins in deployed environments. If moved to a same-site deployment, the cookie policy should be tightened to the smallest workable `SameSite` value.

The frontend enables `withCredentials` on its HTTP client and attempts silent refresh when an API request receives a `401`, subject to the retry guard in `apiClient.ts`. This is a client convenience; the backend still validates the cookie and refresh-token state.

### 3.4 Google OAuth

Google OAuth is an external authentication adapter exposed through the application OAuth service. The frontend obtains a Google credential using the configured public client ID and sends the resulting identity information to the backend. The backend must verify the credential with Google before creating or linking a local user.

> The Google client ID is a public client identifier, not a secret. The backend still requires protection against forged or invalid Google tokens; the client ID alone must never be treated as proof of identity.

---

## 4. Authorization

Authorization is registered separately from authentication in `Infrastructure/DependencyInjection.cs` with `services.AddAuthorization()`.

Authentication answers **who the caller is**. Authorization answers **what the caller may do**. The API therefore needs both:

1. A valid JWT bearer token.
2. An authorization decision based on the endpoint's required role or policy.

The main roles are:

- `Customer` — storefront, cart, checkout, and customer-owned account operations.
- `Admin` — administrative catalogue, purchase, dashboard, and user-management operations.

Frontend route guards such as `ProtectedRoute` and `AdminRoute` improve navigation and prevent showing restricted screens, but they are not substitutes for API enforcement. Every protected endpoint must independently validate the bearer token and its role or policy.

### Endpoint authentication rules

The `AuthGroup` endpoints are explicitly anonymous because they must create sessions:

- Login
- Registration
- Refresh
- Password-reset and verification flows

Protected groups, such as cart, purchase, user, dashboard, and administrative operations, must not inherit an anonymous override. Any endpoint added to a protected group should be reviewed to confirm that its intended authentication and authorization rules are present.

### Security rule

The server must never trust a role supplied in a request body, query string, or client-controlled frontend state. Roles come from the validated token or user context and are checked by backend policies.


---

## 5. CORS

CORS is registered as `clothingStoreDevCors` in `Api/DependencyInjection.cs`:

```csharp
policy.WithOrigins("https://localhost:5173")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

The policy is applied only in the Development environment:

```csharp
if (app.Environment.IsDevelopment())
    app.UseCors("clothingStoreDevCors");
```

This is an explicit origin allow-list rather than a wildcard. It is compatible with the refresh-token cookie because credentialed requests require an exact origin. The frontend is expected to run at the exact HTTPS origin `https://localhost:5173` during development.

### Security rule

`AllowAnyHeader` and `AllowAnyMethod` reduce the strictness of the CORS policy, but the policy is still limited to one configured origin. For production:

- Replace the development policy with the deployed frontend origin.
- Do not use `AllowAnyOrigin` together with credentials.
- Keep the API and frontend origins controlled and reviewed.
- Do not treat CORS as authentication; browsers enforce it, non-browser clients do not.

---

## 6. Rate Limiting and Abuse Protection

ASP.NET Core rate limiting is enabled with `UseRateLimiter()` after authentication and before authorization. Rejected requests receive HTTP `429 Too Many Requests`.

| Policy | Partition | Limit | Purpose |
| --- | --- | --- | --- |
| `standard` | Client IP | 100/minute, four sliding-window segments | General request protection |
| `auth-ip-create` | Client IP | 10/minute | Registration abuse and cost control |
| `auth-ip-relaxed` | Client IP | 30/minute | External login, registration, and refresh cost/DoS control |
| `auth-ip-spray-guard` | Client IP | 20/minute | Login spray-attack protection |
| `authenticated-read` | JWT `sub`, falling back to IP | 60/minute | Authenticated read protection |
| `authenticated-write` | JWT `sub`, falling back to IP | 30/minute | Authenticated mutation protection |
| `purchase-strict` | JWT `sub`, falling back to IP | 10/minute | Checkout and purchase protection |
| `upload-concurrency` | Shared upload limiter | 4 concurrent, queue of 2 | Upload concurrency control |
| `auth-email-strict` | Email partition | 5/minute | Login attempts targeting one email |
| `auth-target-strict` | Verification target | 3 per 15 minutes | Verification-code abuse protection |

The email and target limiters are keyed services used directly by authentication endpoints. For example, the login endpoint acquires an email-specific lease and returns `429` with `Retry-After` when the lease is unavailable.

Queues are disabled for the sliding-window authentication policies (`QueueLimit = 0`) so excess requests are rejected instead of being held indefinitely. The frontend avoids automatically retrying `429` responses in its React Query configuration and displays rate-limit feedback where implemented.

Rate limits are defense in depth, not a substitute for authentication, authorization, or input validation.

### Proxy warning

The current partition key uses `HttpContext.Connection.RemoteIpAddress`. Behind a proxy, this may be the proxy's address rather than the real client address. Do not blindly trust forwarded IP headers. Configure `ForwardedHeaders` with a known proxy allow-list before relying on IP rate limits in a production topology.


---

## 7. Password and Account Security

Local passwords are handled through the password-security abstraction, while refresh tokens are handled through token hashing and domain services. Authentication failures are represented as application errors and mapped to HTTP responses by the API.

Security expectations for account flows include:

- Passwords must be stored through a password-hashing implementation, never in plaintext.
- Refresh tokens must be stored and compared as hashes.
- Registration and verification flows should not reveal whether an account exists in a way that enables account enumeration.
- Verification and reset codes must be short-lived, single-use, and rate-limited.
- Authentication errors should not include password hashes, token values, or detailed provider errors in the response.

Operational authentication logging must not contain passwords, raw access tokens, raw refresh tokens, verification codes, or payment secrets. This is enforced by the `[Sensitive]` redaction mechanism described in [10.1](#101-sensitive-data-redaction-in-request-logging).

---

## 8. Payment Webhook Security

Payment updates arrive asynchronously from Chargily, so the webhook is an untrusted external input boundary.

The API registers Chargily webhook validation middleware and invokes it in the HTTP pipeline:

```csharp
services.AddChargilyPayWebhookValidationMiddleware();
app.UseChargilyPayWebhookValidation();
```

The webhook endpoint relies on Chargily's signature validation rather than trusting arbitrary status payloads. The payment handler also treats repeated terminal callbacks idempotently: a payment already marked `Paid` or `Failed` is not processed again.

The webhook URL is configured externally because Chargily cannot call a local machine. In the Compose setup it points to the ngrok HTTPS domain:

```yaml
Chargily__WebhookEndpointUrl=https://${NGROK_DOMAIN}/api/purchases/webhook/payment
```

The ngrok domain and token are deployment configuration, not application authentication. Rotate an exposed ngrok token if it is disclosed, and restrict or remove the ngrok inspector before exposing a development environment.

---

## 9. Configuration and Secret Safety

Sensitive settings are supplied through environment variables, including:

- Database credentials.
- JWT signing key.
- Google OAuth configuration.
- Chargily API and webhook configuration.
- SMTP credentials.
- ngrok authentication and domain values.
- Development certificate password.

These values must not be committed, placed in frontend bundles, or copied into documentation. In particular, `VITE_*` variables are delivered to the browser and must contain only values that are safe to expose. A JWT signing key, SMTP password, Chargily secret, or ngrok token must remain server-side.

For deployment:

1. Create local environment files from the documented example/template.
2. Use a secret manager or protected runtime environment in production.
3. Rotate credentials that have been exposed in logs, screenshots, commits, or public chat.
4. Restrict access to Seq and any exposed development tooling.
5. Use separate credentials and projects for development and production.

---

## 10. Error Handling and Observability Security

The API uses the default exception handler and Serilog request logging. Unexpected exceptions are converted into a consistent response rather than exposing stack traces to clients.

Logging must follow a data-minimization policy:

- Do not log passwords, bearer tokens, refresh-token cookies, verification codes, phone numbers, or payment secrets.
- Passwords, refresh tokens, external identity tokens, verification and reset codes, phone numbers, and delivery addresses must be marked `[Sensitive]` so the request logger redacts them; see [10.1](#101-sensitive-data-redaction-in-request-logging).
- Avoid logging complete request bodies containing personal data.
- Keep user identifiers and payment references sufficient for operations without exposing credentials.
- Restrict Seq access because logs can contain operational and personal data.
- Use the health endpoint for liveness/readiness checks, but do not place secrets in health output.

### 10.1 Sensitive-data redaction in request logging

The data-minimization rules above are enforced in code, not left to reviewer discipline. Every MediatR request passes through a logging pre-processor that reflects over the request's public properties and replaces the value of any property marked `[Sensitive]` with the literal string `[REDACTED]` before the request is written to the log sink.

The marker is defined in `Application/Common/Attributes/SensitiveAttribute.cs`:

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SensitiveAttribute : Attribute;
```

The consumer is `Application/Common/Behaviours/LoggingBehaviour.cs`, registered as an open pre-processor so it applies to every request without per-endpoint wiring:

```csharp
cnf.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
```

Redaction happens in the property projection, before the value ever reaches `ILogger`:

```csharp
var requestProperties = typeof(TRequest).GetProperties()
    .ToDictionary(property => property.Name, property => Attribute.IsDefined(
                property, typeof(SensitiveAttribute)) ? "[REDACTED]" : property.GetValue(request));

_logger.LogInformation("Request: {Name} {@UserId} {@Email} {@Request}", requestName, userId, email, requestProperties);
```

Because the substitution occurs at the projection step, the plaintext value is never passed to the logger. It is therefore absent from Serilog output, from any `WriteTo.Console` or file sink, and from Seq — redaction cannot be bypassed by a downstream sink or by a log-level change.

The emitted line retains the request name and the caller's user ID and email, which are required to correlate a log entry with an operation, while sensitive members of the payload are masked.

#### Annotated members

| Command | Redacted member | Category |
| --- | --- | --- |
| `RegisterCommand` | `PhoneNumber`, `Password` | Phone number, credential |
| `RegisterExternalAuthCommand` | `IdToken`, `PhoneNumber` | External identity token, phone number |
| `LoginCommand` | `Password` | Credential |
| `LogInExternalAuthCommand` | `IdToken` | External identity token |
| `RefreshCommand` | `refreshToken` | Session token |
| `LogOutCommand` | `refreshToken` | Session token |
| `VerifyEmailCommand` | `Code` | Verification code |
| `ResetPasswordCommand` | `Code`, `NewPassword` | Verification code, credential |
| `CreatePurchaseCommand` | `CustomerAddress` | Delivery address |

For positional records the attribute must be applied with the `property:` target so it lands on the generated property rather than the constructor parameter:

```csharp
public sealed record VerifyEmailCommand(
    string Email,
    [property: Sensitive] string Code)
    : IRequest<Result<AuthResponse>>;
```

Using a bare `[Sensitive]` on a positional record parameter targets the primary-constructor parameter instead of the property. Because `LoggingBehaviour` inspects `GetProperties()`, such an annotation would compile but silently fail to redact. Always write `[property: Sensitive]`.

#### Known gaps

Redaction is opt-in per property, so a newly added sensitive member is logged in plaintext until it is annotated. Two current commands do not yet mark members that are sensitive by the same standard applied elsewhere:

| Command | Unredacted member | Risk |
| --- | --- | --- |
| `CreatePurchaseCommand` | `CustomerPhone` | Customer phone number is written to the log, even though the sibling `CustomerAddress` in the same command is redacted. |
| `CreateUserCommand` | `PhoneNumber`, `Password` | Administrative user creation logs a plaintext password and phone number. This is the most serious of the gaps, because `CreateUserCommand` is dispatched by `POST /api/users`, an admin-only route. |

Note also that `LoggingBehaviour` enumerates only **public properties** via `GetProperties()`. Fields, including private ones, are never projected into the log at all, so a sensitive value held in a field is not leaked by this path; the exposure risk applies to unmarked public properties.

#### Rules for new code

- Annotate every password, token, verification or reset code, phone number, and delivery address on any new request record.
- Use `[property: Sensitive]` for positional record parameters.
- Do not mark whole requests as sensitive to suppress the log entry; annotate the individual members so operational context is preserved.
- Review new commands for sensitive members as part of the same check that confirms the endpoint's authorization rules.


---

## 11. Security Limitations and Deployment Checklist

### Current limitations to keep in mind

- HTTPS relies on the host/container certificate and correct proxy scheme configuration.
- CORS is enabled only in Development and is currently written around the local frontend origin.
- IP-based rate limiting is less accurate behind an untrusted or unconfigured proxy.
- The frontend JWT role decoding is not a security check.
- Sensitive-data redaction in request logs is opt-in per property, so an unannotated member is logged in plaintext. `CreateUserCommand` (password, phone number) and `CreatePurchaseCommand.CustomerPhone` are currently unredacted; see [10.1](#101-sensitive-data-redaction-in-request-logging).
- The backend must continue to enforce authorization for every protected endpoint; UI route guards are convenience only.
- Chargily webhook validation protects the callback boundary, but the ngrok development tunnel should not be treated as a production network perimeter.
- Secrets in `.env` files are sensitive even when they are described as test or demo values.

### Deployment checklist

- [ ] Set a non-committed `.env` from the documented template.
- [ ] Use a strong, randomly generated JWT signing key.
- [ ] Use HTTPS for the API and frontend, including secure-cookie requests.
- [ ] Configure the exact production CORS origin; never use wildcard credentialed CORS.
- [ ] Confirm forwarded headers and trusted proxies before relying on client IP limiting.
- [ ] Keep PostgreSQL and internal services on the private Docker network.
- [ ] Restrict access to Seq, ngrok inspection, and the host ports.
- [ ] Verify that protected endpoints require authentication and role/policy authorization.
- [ ] Confirm refresh-token cookies are `HttpOnly`, `Secure`, path-restricted, and rotated.
- [ ] Verify the Chargily webhook signature before accepting payment status updates.
- [ ] Rotate any credential that has been exposed.
- [ ] Review logs for credentials and personal data.
- [ ] Confirm every sensitive member on new request records is marked `[property: Sensitive]`, and close the outstanding gaps listed in [10.1](#101-sensitive-data-redaction-in-request-logging).
- [ ] Run `npm run lint` and `npm run build` for frontend changes and `dotnet test` for backend tests when implementing security-related changes.

---

## Summary

The backend uses layered controls rather than relying on a single security mechanism:

- HTTPS and HSTS protect transport confidentiality.
- JWT bearer authentication identifies callers.
- Authorization policies control what authenticated callers may do.
- Refresh-token rotation and `HttpOnly` cookies protect browser sessions.
- CORS limits browser cross-origin access to the configured frontend.
- Rate limiting limits registration, login, verification, purchase, and upload abuse.
- Chargily middleware protects webhook authenticity.
- Environment variables and careful logging protect credentials and sensitive data.

The security boundary remains the API: the frontend can hide or redirect restricted UI, but the backend must validate every token, role, origin-relevant request, payment callback, and abuse-control decision.

