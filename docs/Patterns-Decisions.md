# Patterns & Design Decisions

This document explains the recurring design patterns used in the Clothing Shop backend: what each pattern is **in this codebase**, why it was chosen over the obvious alternative, and what it costs. Every entry names its trade-off.

> Related: [`Architecture.md`](Architecture.md) for layering and request flow, [`Database-Design.md`](Database-Design.md) for persistence detail.

> **Scope — backend only.** All patterns below describe `backend/src`. The React frontend (`frontend/src`) intentionally uses no formal design or architectural patterns — no CQRS, mediator pipeline, repository/unit-of-work, domain model, or SOLID layering. It is organized by feature folders with hooks/context for state and data-fetching only, so it is out of scope for this document.

---

## 1. At a Glance

| # | Pattern / Decision | Where | Why (one line) | Cost (one line) |
|---|---|---|---|---|
| 1 | Clean Architecture layering | `backend/src/{Domain,Application,Infrastructure,Api}` | Keep business rules independent of frameworks | More projects, mappings, indirection |
| 2 | CQRS with MediatR | `Application/Features/*` | One use case = one request, handler, validator | Type proliferation for trivial reads |
| 3 | MediatR pipeline behaviours | `Application/Common/Behaviours` | Cross-cutting concerns without repeating code | Implicit execution order |
| 4 | Result/Error instead of exceptions | `Domain/Common/Results`, `Api/Extensions/ProblemExtensions.cs` | Expected failures are explicit return values | Verbose call sites; dual model remains |
| 5 | Rich domain model + Value Objects | `Domain`, `Domain/Common/ValueObjects` | Invalid states rejected at construction | More domain code plus EF mapping work |
| 6 | Domain Events | `Domain/Common/DomainEvent.cs`, `DispatchDomainEventsInterceptor.cs`, `Carts/EventHandlers` | Decouple reactions from originating operation | Pre-save dispatch, no outbox |
| 7 | Repository + Unit of Work | `Application/Common/Interfaces`, `Infrastructure` | Handlers depend on contracts, not EF Core | Abstraction overhead; no explicit transactions |
| 8 | EF Core Interceptors | `Infrastructure/Data/Interceptors` | Timestamps plus event dispatch in one place | Save-time magic; ordering matters |
| 9 | Thin FastEndpoints + Problem Details | `Api/Endpoints`, `Api/Extensions/ProblemExtensions.cs` | HTTP stays dumb; errors map consistently | Endpoint-per-operation boilerplate |
| 10 | Adapter interfaces for externals | `Application/Common/Interfaces/Services`, `Infrastructure/Services` | Swap providers without touching use cases | Extra interface per integration; one Chargily leak remains |
| 11 | .NET Options Pattern | `Infrastructure/Options`, `Infrastructure/DependencyInjection.cs` | Bind and validate configuration as typed settings | Configuration classes and mixed access paths |

SOLID is covered in [Section 4](#4-solid--how-the-codebase-applies-it).

---

## 2. Patterns in Detail

### 2.1 Clean Architecture layering

- **What here:** four projects with inward dependencies: `Api -> Application -> Domain` and `Infrastructure -> Application`. `Domain` depends on no other backend project except the narrow `MediatR.Contracts` notification contract. See `Architecture.md` section 1.
- **Why over the alternative:** the alternative is a single-project app where EF entities leak into controllers. That starts faster but couples rules to ASP.NET and EF. Here a rule like `Product.AddVariant` compiles and tests with no web or database reference.
- **Cost:** more ceremony: separate projects, DTO mapping, interface plus implementation pairs. Trivial CRUD pays the full tax.

### 2.2 CQRS via MediatR (commands, queries, handlers)

- **What here:** each use case is an `IRequest<TResponse>` plus a handler, for example `CreatePurchaseCommand` with `CreatePurchaseCommandHandler`, with a FluentValidation validator alongside. The complete endpoint-to-handler request path is shown in [`Architecture.md` §4](Architecture.md#4-request-processing-pipeline).
- **Why over the alternative:** the alternative is fat services or controllers with many public methods. CQRS gives one request shape, one validator, one handler, and one pipeline, so logging, validation, and performance come for free.
- **Cost:** explosion of small types. A simple lookup still needs query plus handler plus endpoint plus DTOs. Worth it for checkout and webhooks; overkill for pure CRUD.

### 2.3 MediatR pipeline behaviours

- **What here:** `ValidationBehaviour`, `LoggingBehaviour`, `PerformanceBehaviour`, and `UnhandledExceptionBehaviour` run around every request, registered in `Application/DependencyInjection.cs`. Their placement around handler execution is shown in [`Architecture.md` §4](Architecture.md#4-request-processing-pipeline).
- **Why over the alternative:** the alternative is calling validation and log lines at the top of every handler. Behaviours enforce it globally so a handler cannot forget validation.
- **Cost:** implicit control flow. A reader sees a handler with no visible validation and must know the pipeline exists. Ordering is configuration, not code at the call site.

### 2.4 Result and Error instead of exceptions for expected failures

- **What here:** `Result<TValue>` carries either a value or a list of `Error` values. Factories like `Error.NotFound`, `Error.Conflict`, and `Error.Validation` carry code, description, and kind. Handlers return `Result<T>`; `ProblemExtensions.ToProblem` maps the kind to an HTTP status and Problem Details payload. Example: variant-not-found and insufficient-stock in `CreatePurchaseCommandHandler` return errors instead of throwing.
- **Why over the alternative:** the alternative throws exceptions for business outcomes. Exceptions hide control flow and are expensive as control flow. `Result` forces the caller to handle the failure path and gives the API a typed error to map.
- **Cost:** verbosity, since every call site checks `IsError` or calls `Match`. The codebase is also dual-model: unexpected failures still throw and flow through `UnhandledExceptionBehaviour` plus global handling.

### 2.5 Rich domain model plus Value Objects

- **What here:** entities expose behaviour, not setters: `Product.Create`, `Product.AddVariant`, `User.Create`, `Payment.UpdateStatus`. Invariants live in the entity through `Validate` methods and error types such as `ProductErrors` and `UserErrors`. Value objects (`Email`, `PhoneNumber`, `Password`, `Address`) validate on `Create` and persist through EF value conversions or owned types.
- **Why over the alternative:** the alternative is anemic entities with public setters and validation scattered across handlers. Centralizing rules means `Email.Create` normalizes and validates once, and every caller gets the same guarantee.
- **Cost:** more domain code to write and test, plus EF mapping friction such as conversions, owned-type configuration, and private-collection access modes.

### 2.6 Domain Events

- **What here:** entities collect `DomainEvent` objects in memory through `Entity.AddDomainEvent`. `DispatchDomainEventsInterceptor` publishes pending events through MediatR during `SavingChangesAsync`. The concrete reaction today: `User.Create` raises `UserCreatedDomainEvent`, and `UserCreatedDomainEventHandler` creates the user cart.
- **Why over the alternative:** the alternative calls cart creation directly inside registration. Events keep registration ignorant of shopping concerns, so adding a second reaction does not touch the register flow.
- **Cost:** dispatch happens before the database write, with no outbox and no transaction coordination. See `Architecture.md` sections 3.4 and 5.4 and trade-off 3. If the save fails after the handler already saved the cart, the reaction outlives its trigger.

### 2.7 Repository plus Unit of Work

- **What here:** handlers depend on repository specializations such as `IProductRepository` and `IVariantRepository`, aggregated behind `IUnitOfWork`, which exposes **13 repository properties** plus `SaveChangesAsync`. Implementations live in `Infrastructure/Repositories` and `Infrastructure/UnitOfWork/UnitOfWork.cs`, which delegates to `AppDbContext`. The count matches the 13 database tables, but it does **not** mean that every table is modeled as an aggregate root.
- **Why over the alternative:** the alternative injects `AppDbContext` into handlers. That couples use cases to EF Core query shape, tracking, and provider quirks, and makes handler tests need a database double. Repository contracts express intent and hide query mechanics. The wider repository surface is also pragmatic: handlers can load variants, cart items, purchase items, images, or payments with purpose-built queries instead of routing every operation through an aggregate-root repository.
- **Cost:** an interface plus implementation per repository, plus a wide `IUnitOfWork` surface. Independently exposing repositories for entities that also appear as child collections can let handlers bypass parent-level coordination if they mutate those children directly; repository availability is therefore not a substitute for domain invariants. `IUnitOfWork` also has no explicit begin or commit transaction API: one `SaveChangesAsync` is the atomic boundary, so multi-save flows such as checkout are not atomic. See `Architecture.md` trade-off 6.

### 2.8 EF Core Interceptors

- **What here:** `AuditableEntityInterceptor` stamps `CreatedAtUtc` and `LastModifiedUtc`, including owned-entity changes. `DispatchDomainEventsInterceptor` publishes pending domain events. Both run on save; audit is registered first so timestamps exist before events publish.
- **Why over the alternative:** the alternative sets timestamps and publishes events manually in every handler. Interceptors make the guarantee structural so no handler can forget it.
- **Cost:** save-time magic. Behaviour is invisible at the handler call site, and ordering plus transactional semantics must be read from DI registration and interceptor code. Debugging starts in infrastructure, not application.

### 2.9 Thin FastEndpoints boundary plus Problem Details

- **What here:** endpoints under `Api/Endpoints` bind route, query, or body values, build a command or query, send it through MediatR, and map `Result` to `Results.Ok` or `errors.ToProblem`. See `CreatePurchase.cs` and `PaymentWebhook.cs`.
- **Why over the alternative:** the alternative puts logic in controllers, such as price math or stock checks in the endpoint. Thin endpoints keep HTTP replaceable and force rules into domain and application code where they are testable without a web server.
- **Cost:** one endpoint class per operation plus request and response DTOs and route groups. Route and validation rules can drift if endpoint binding and FluentValidation evolve separately.

### 2.10 Adapter interfaces for external services

- **What here:** contracts such as `IEmailService`, `IPaymentGatewayService`, `IOAuthService`, `ITokenProvider`, `IPasswordService`, `ITokenHasherService`, and background-job contracts live in `Application/Common/Interfaces`. Implementations live in `Infrastructure/Services` and `BackgroundJobs`. Checkout calls `IPaymentGatewayService.CreateCheckout`, never the Chargily SDK directly.
- **Why over the alternative:** the alternative constructs vendor clients inside handlers. Adapters let tests stub the gateway and let the app swap providers without rewriting checkout.
- **Cost:** each integration is two types plus registration plus mapping. One leak is documented: `IPaymentGatewayService` exposes the Chargily `CheckoutStatus` type, so the contract is not fully vendor-neutral yet. See `Architecture.md` section 6 and trade-off 7.

### 2.11 .NET Options Pattern for configuration

- **What here:** configuration is represented by four typed classes in `Infrastructure/Options`: `JwtSettings`, `ImageStorageOptions`, `EmailOptions`, and `ChargilyOptions`. Each class has a `SectionName`, typed properties, `init` accessors, and data-annotation constraints. `DependencyInjection.cs` registers each with `AddOptions<T>().BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()`, so invalid or missing configuration fails application startup instead of being discovered later during a request. Services such as `ImageService`, `EmailService`, and `ChargilyPaymentGatewayService` receive their settings through `IOptions<T>`.
- **Why over the alternative:** the alternative is repeatedly reading string keys from `IConfiguration` or passing loose configuration parameters into services. Typed options make the required configuration shape explicit, centralize validation, and let infrastructure services depend on a named settings object without repeating key names and conversion rules.
- **Cost:** each integration adds a configuration class, binding/validation registration, and a typed dependency. The current implementation is not fully uniform: startup code also calls `IConfiguration.Get<T>()` for JWT, email, and Chargily configuration, while some services consume `IOptions<T>` at runtime. This mixed access creates two configuration paths and should be consolidated if more settings are added.

The pattern is distinct from a functional `Option<T>` type. The former organizes configuration and validates it at startup; the latter would model a value that may or may not be present. This codebase does not implement a functional `Option<T>` abstraction, as documented in [Section 3.5](#35-no-functional-optiontmaybet-abstraction).

---

## 3. What Was Deliberately Not Used

These omissions are conscious, not gaps in knowledge. Each was considered and rejected for the current scope.

### 3.1 No generic service/crud base class

- **What:** there is no shared `BaseService<T>` with generic create/update/delete.
- **Why not:** generic CRUD pushes validation and authorization into conditional branches and invites fat services. Feature-specific handlers keep rules beside their use case.
- **Cost of this choice:** repeated handler scaffolding for simple operations.

### 3.2 No anemic entities with public setters

- **What:** entities do not expose public setters for business state; mutations go through `Create` and `Update*` methods returning `Result`.
- **Why not:** public setters let any caller put `Product` in an invalid state, for example a negative `BasePrice` or a duplicate variant. Centralized methods make invalid states unrepresentable at the call site.
- **Cost of this choice:** more domain methods and tests; EF Core needs private constructors and property-access configuration.

### 3.3 No exceptions for expected business outcomes

- **What:** expected failures such as `VARIANT_NOT_FOUND` or `INSUFFICIENT_STOCK` return `Result` errors; exceptions remain for unexpected technical faults.
- **Why not:** exception-driven business flow hides the failure path and couples the API to catch-block conventions. `Result` keeps both paths visible and typed.
- **Cost of this choice:** verbose error checks at every call site and a dual mental model for junior contributors.

### 3.4 No direct `DbContext` in handlers

- **What:** handlers depend on repository contracts and `IUnitOfWork`, not `AppDbContext`.
- **Why not:** direct context access couples handlers to EF tracking, `Include` shape, and provider-specific query behaviour.
- **Cost of this choice:** an interface and implementation per persistence concern plus a wide `IUnitOfWork`, without gaining explicit transaction control today. This wider surface can expose child entities independently, so handlers must still respect parent-level domain invariants.

### 3.5 No functional `Option<T>`/`Maybe<T>` abstraction

- **What:** the project has no functional optional-value type such as `Option<T>`, `Maybe<T>`, or `Some/None`. This is separate from the **Options Pattern** used for external-service configuration in `Infrastructure/Options`. C# nullable reference types represent simple absence, such as a nullable entity returned by `IRepository<TEntity>.GetByIdAsync`; `Result<T>` represents expected business failures.
- **Why not used:** adding another value wrapper would introduce new types, conversions, and handling rules without replacing the existing result model. The current flows generally need either a nullable lookup or a typed business error.
- **Cost of this choice:** nullability does not make absence impossible to ignore. Callers can still dereference a nullable lookup without a compiler error. If optional-value transformations or composable absence handling become common, a dedicated functional Option abstraction should be reconsidered.

---

## 4. SOLID — How the Codebase Applies It

SOLID is used as a code-review lens here, not as dogma. Each principle maps to a concrete mechanism above.

### 4.1 Single Responsibility Principle

- **Applied:** `Product` owns catalog invariants; `CreatePurchaseCommandHandler` orchestrates checkout; `ChargilyPaymentGatewayService` talks to the payment provider; `AuditableEntityInterceptor` owns timestamps.
- **Why:** a change in gateway SDK shape should not require editing checkout orchestration, and a timestamp rule should not require editing every handler.
- **Cost:** responsibility splits multiply types. Checkout understanding now spans endpoint, handler, domain entities, repositories, gateway contract, and interceptor code.

### 4.2 Open/Closed Principle

- **Applied:** new checkout reactions and cross-cutting behaviour are added without editing existing handlers: add an `INotificationHandler<T>` for a new domain event, or add a MediatR behaviour for a new pipeline concern.
- **Why over modification:** editing `RegisterCommandHandler` for every downstream reaction turns registration into a change magnet. Extension points keep stable flows closed.
- **Cost:** indirection. The runtime behaviour of registration is no longer visible in the registration handler alone.

### 4.3 Liskov Substitution Principle

- **Applied:** repository and service implementations must honor their contracts without strengthening preconditions. Any `IProductRepository` or `IPaymentGatewayService` should be substitutable in handler tests and production wiring.
- **Why:** handlers program against contracts so infrastructure can change providers or query strategies safely.
- **Cost and gap:** the Chargily `CheckoutStatus` leak weakens this guarantee for payment-status queries: a non-Chargily implementation cannot satisfy that return type cleanly. See `Architecture.md` trade-off 7.

### 4.4 Interface Segregation Principle

- **Applied:** narrow service contracts such as `IPasswordService`, `ITokenHasherService`, and `IEmailService` replace one god service. Repository contracts are also separated by persistence concern, with the child-repository trade-off documented in [Section 2.7](#27-repository-plus-unit-of-work). Handlers take only what they use.
- **Why:** a handler that needs password verification should not depend on email delivery or checkout creation.

For the interface-wide coordination and its child-repository exception, see [Section 2.7](#27-repository-plus-unit-of-work).

### 4.5 Dependency Inversion Principle

- **Applied:** `Application` owns ports such as `IRepository<T>`, `IUnitOfWork`, `IUser`, and external-service contracts; `Infrastructure` implements them; `Api/Program.cs` wires implementations at startup.
- **Why over direct dependence:** high-level checkout policy should not depend on EF Core, SMTP, JWT, OAuth, or Chargily details. Depending on abstractions lets tests substitute fakes and lets providers change beneath stable use cases.
- **Cost:** every integration requires an interface, implementation, registration, mapping, and failure translation. Abstractions can also hide important semantics, such as the absence of explicit transactions behind `IUnitOfWork`.
