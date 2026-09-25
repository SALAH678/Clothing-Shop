# API Design and Reference Specification

## Overview

The Clothing Shop backend exposes a RESTful HTTP API built using **FastEndpoints** on .NET 10 / ASP.NET Core 10. Instead of traditional MVC controllers with action methods, the API follows the **REPR (Request-Endpoint-Response)** pattern: each endpoint is modeled as an independent class encapsulating route definition, authorization requirements, rate limiting policies, OpenAPI documentation, and request dispatching via MediatR handlers.

This document serves as the complete technical specification for all 37 HTTP endpoints across 7 resource groups, defining routing schemas, authentication scopes, request/response models, pagination standards, multipart upload contracts, and error responses.

---

## 1. Architectural Principles and Protocols

### 1.1 REPR Pattern with FastEndpoints

Each API operation is isolated in its own endpoint class inheriting from either `Endpoint<TRequest, TResponse>` or `EndpointWithoutRequest<TResponse>`. The endpoint handles HTTP parameter binding, invokes the corresponding MediatR command or query, maps the domain result using explicit pattern matching, and emits standard HTTP status codes.

```text
HTTP Request ──> FastEndpoints (Route / Auth / Throttling)
                      │
                      ▼
               MediatR Pipeline (Logging / Tracing / Validation / Transaction)
                      │
                      ▼
               Application Handler ──> Domain Model & Repositories
                      │
                      ▼
               Result<T> / Error
                      │
                      ▼
               HTTP Response (Status Code + Payload / RFC 7807 ProblemDetails)
```

### 1.2 Base URL and Versioning Strategy

- **Default Base Path:** `/api`
- **Versioning Mechanism:** Header-based versioning using the `X-Api-Version` HTTP request header.
- **Default Version:** `1.0` (`ApiVersion(1.0)`). When `X-Api-Version` is omitted, the API assumes version `1.0`.
- **Version Set:** `ClothingStoreApi` maps all production endpoints to API Version `1.0`.
- **OpenAPI / Swagger:** FastEndpoints integrates Swagger generation at `/swagger` and raw schema at `/swagger/v1/swagger.json`. JWT Bearer authentication is pre-configured into the OpenAPI specification document.

| Parameter | Configuration | Location / Header |
| --- | --- | --- |
| **API Version** | `1.0` | Header: `X-Api-Version: 1.0` (Optional, defaults to 1.0) |
| **Media Type** | `application/json` | Header: `Content-Type: application/json` / `Accept: application/json` |
| **Multipart Type** | `multipart/form-data` | Used for endpoints handling binary image file uploads |
| **Authorization** | `Bearer <JWT>` | Header: `Authorization: Bearer eyJhbGciOi...` |

### 1.3 Security and Session Management

- **Access Token:** Short-lived JWT (15-minute validity) passed via the `Authorization: Bearer <token>` header for all protected endpoints.
- **Refresh Token:** Long-lived credential (7 days) stored securely in an `HttpOnly`, `Secure`, `SameSite=None` cookie with `Path=/api/auth`. JavaScript running in the browser cannot access this cookie.
- **Role-Based Authorization:** Endpoints enforce access controls using FastEndpoints `Roles("Admin")` or `Roles("Admin", "Customer")`.
- **Rate Limiting:** Every endpoint is protected by a named rate limit policy or partitioned keyed limiter registered in ASP.NET Core rate limiting middleware.


---

## 2. Standardized Error Handling (RFC 7807)

The API adheres to the **RFC 7807 Problem Details** specification for all client (4xx) and server (5xx) errors.

Application handlers return a unified `Result<T>` or `Result`. In the event of a domain or validation failure, errors are converted to HTTP responses via `ProblemExtensions.ToProblem()`.

### 2.1 Error Domain Mapping

| Domain `ErrorKind` | HTTP Status Code | RFC 7807 Problem Details Format |
| --- | --- | --- |
| `ErrorKind.Validation` | **400 Bad Request** | `ValidationProblemDetails` containing an `errors` dictionary keyed by property name |
| `ErrorKind.Unauthorized` | **401 Unauthorized** | `ProblemDetails` with `title` (description) and `detail` (error code) |
| `ErrorKind.Forbidden` | **403 Forbidden** | `ProblemDetails` with `title` (description) and `detail` (error code) |
| `ErrorKind.NotFound` | **404 Not Found** | `ProblemDetails` with `title` (description) and `detail` (error code) |
| `ErrorKind.Conflict` | **409 Conflict** | `ProblemDetails` with `title` (description) and `detail` (error code) |
| `ErrorKind.Unexpected` / Other | **500 Internal Server Error** | `ProblemDetails` with generic internal error structure |

### 2.2 Standard Problem Details Response Schemas

#### Standard Domain Error Schema (401, 403, 404, 409, 500)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Product with ID '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found.",
  "status": 404,
  "detail": "Product.NotFound"
}
```

#### Validation Error Schema (400 Bad Request)

Produced automatically when FluentValidation pipeline checks fail:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "The Email field is not a valid e-mail address."
    ],
    "Password": [
      "Password must be at least 8 characters long.",
      "Password must contain at least one uppercase letter."
    ]
  }
}
```

#### Rate Limit Exhaustion (429 Too Many Requests)

When a rate limit policy or partitioned limiter budget is exceeded, the server immediately rejects the request without entering application handlers, returning HTTP 429 and a `Retry-After` header indicating remaining wait time in seconds:

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 60
Content-Type: application/problem+json
```

---

## 3. Pagination and Collection Conventions

All paginated endpoints (`GET /api/products`, `GET /api/purchases`, `GET /api/users`) return a standardized `PaginatedList<T>` envelope.

### 3.1 Pagination Query Parameters

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `PageNumber` | integer | `1` | One-based index of the page to retrieve (minimum: 1). |
| `PageSize` | integer | `10` | Total number of items per page (bounded by validator limits). |

### 3.2 Paginated Envelope Schema

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "totalCount": 48,
  "items": [
    { ... }
  ]
}
```


---

## 4. Complete Endpoint Reference

The backend exposes **37 HTTP endpoints** organized under 7 resource groups:

```text
api/
├── auth/            (10 endpoints) Authentication, session, and password flows
├── carts/           (3 endpoints)  User shopping cart management
├── categories/      (7 endpoints)  Category taxonomy and hierarchy
├── dashboard/       (1 endpoint)   Administrative overview statistics
├── products/        (8 endpoints)  Catalog, variants, and stock management
├── purchases/       (4 endpoints)  Checkout, payments, webhooks, and orders
└── users/           (4 endpoints)  User account profiles and management
```

---

### 4.1 Authentication API (`/api/auth`)

Group Base Route: `/api/auth`  
Common Policy: Publicly accessible (`AllowAnonymous`), session tokens managed through refresh cookies.

```
POST /api/auth/register
POST /api/auth/verify-email
POST /api/auth/login
POST /api/auth/google/register
POST /api/auth/google/login
POST /api/auth/refresh
POST /api/auth/resend-code
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/logout
```

#### 4.1.1 Register User Account
- **HTTP Route:** `POST /api/auth/register`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `auth-ip-create` (Sliding window: 10 requests / 1 minute by Client IP)
- **Description:** Registers a new local user with role `Customer` or `Admin`, stores hashed password, creates an empty cart, and publishes a verification email job via TickerQ.
- **Request Body (`RegisterCommand`):**
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "0555123456",
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "role": "Customer"
  }
  ```
- **Responses:**
  - `200 OK` — Account registered successfully (`Success`).
  - `400 Bad Request` — Validation failure (weak password, invalid Algerian phone number, invalid email format).
  - `409 Conflict` — An account with the supplied email already exists.
  - `429 Too Many Requests` — Registration rate limit exceeded.
  - `500 Internal Server Error` — Registration transaction failed.

#### 4.1.2 Verify Email Address
- **HTTP Route:** `POST /api/auth/verify-email`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Limiter:** `auth-email-strict` (Keyed partitioned sliding window: 5 requests / 1 minute by target email)
- **Description:** Validates a 6-digit verification code sent to the user's email address. On success, marks `IsEmailVerified = true` and invalidates the token.
- **Request Body (`VerifyEmailCommand`):**
  ```json
  {
    "email": "john.doe@example.com",
    "code": "123456"
  }
  ```
- **Responses:**
  - `200 OK` — Verification successful (`Success`).
  - `400 Bad Request` — Invalid verification code format or expired token.
  - `401 Unauthorized` — Verification code does not match.
  - `404 Not Found` — User or verification token not found.
  - `409 Conflict` — Email has already been verified.
  - `429 Too Many Requests` — Max attempts exceeded for this email target.


#### 4.1.3 User Login (Password Authentication)
- **HTTP Route:** `POST /api/auth/login`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `auth-ip-spray-guard` (Sliding window: 20 requests / 1 minute by Client IP) + `auth-email-strict` (Keyed partitioned sliding window: 5 requests / 1 minute by target email)
- **Description:** Authenticates local credentials, generates a 15-minute access token, persists a hashed 7-day refresh token in the database, sets an `HttpOnly` refresh token cookie, and returns the profile with access token.
- **Request Body (`LoginCommand`):**
  ```json
  {
    "email": "john.doe@example.com",
    "password": "SecurePassword123!"
  }
  ```
- **Response Headers:**
  ```http
  Set-Cookie: refreshToken=d280b1e4...; Path=/api/auth; Secure; HttpOnly; SameSite=None; Expires=Sun, 02 Oct 2026 12:00:00 GMT
  ```
- **Response Body (`LoginResponse`):**
  ```json
  {
    "user": {
      "userId": "11111111-1111-1111-1111-111111111111",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "role": "Customer",
      "phoneNumber": "0555123456",
      "isEmailVerified": true
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
  ```
- **Responses:**
  - `200 OK` — Authenticated successfully.
  - `400 Bad Request` — Malformed request body.
  - `401 Unauthorized` — Invalid password or unverified email address.
  - `429 Too Many Requests` — Exceeded IP spray guard or targeted email login threshold.

#### 4.1.4 Google OAuth Registration
- **HTTP Route:** `POST /api/auth/google/register`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `auth-ip-relaxed` (Sliding window: 30 requests / 1 minute by Client IP)
- **Description:** Validates Google ID token via Google Token Validator, checks email verification from Google claims, creates a local account linked to the Google external provider with the specified phone number, and sets session tokens.
- **Request Body (`RegisterExternalAuthCommand`):**
  ```json
  {
    "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
    "phoneNumber": "0555123456"
  }
  ```
- **Response Body:** `LoginResponse` (User DTO and access token; sets `refreshToken` cookie).
- **Responses:**
  - `200 OK` — Successfully registered and authenticated.
  - `400 Bad Request` — Invalid ID token signature, unverified Google email, or user already exists.
  - `429 Too Many Requests` — Rate limit exceeded.

#### 4.1.5 Google OAuth Login
- **HTTP Route:** `POST /api/auth/google/login`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `auth-ip-relaxed` (Sliding window: 30 requests / 1 minute by Client IP)
- **Description:** Authenticates a user using a Google ID token. If the Google account is not explicitly linked but a verified user with the same email exists, automatically links the Google provider to the account.
- **Request Body (`LogInExternalAuthCommand`):**
  ```json
  {
    "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
  }
  ```
- **Response Body:** `LoginResponse` (User DTO and access token; sets `refreshToken` cookie).
- **Responses:**
  - `200 OK` — Authenticated successfully.
  - `400 Bad Request` — Invalid Google ID token or unverified external email.
  - `404 Not Found` — User account not found and cannot be automatically linked.
  - `429 Too Many Requests` — Rate limit exceeded.


#### 4.1.6 Refresh Access Token
- **HTTP Route:** `POST /api/auth/refresh`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `auth-ip-relaxed` (Sliding window: 30 requests / 1 minute by Client IP)
- **Cookie Requirement:** `refreshToken` cookie must be present.
- **Description:** Reads the refresh token from the cookie, computes SHA-256 hash, looks up active refresh token in database, revokes it, issues a rotated refresh token, and returns a new 15-minute access token.
- **Request Body:** None (reads cookie).
- **Response Body:** `LoginResponse` (User DTO, new access token, rotated `refreshToken` cookie).
- **Responses:**
  - `200 OK` — Tokens rotated successfully.
  - `401 Unauthorized` — Cookie missing, token expired, or token revoked.
  - `404 Not Found` — Refresh token record not found.

#### 4.1.7 Resend Verification Code
- **HTTP Route:** `POST /api/auth/resend-code`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Limiter:** `auth-target-strict` (Keyed partitioned sliding window: 3 requests / 15 minutes by target email)
- **Description:** Generates a new verification code for unverified users and triggers dispatch of an email or SMS notification.
- **Request Body (`ResendCodeCommand`):**
  ```json
  {
    "email": "john.doe@example.com",
    "tokenType": "EmailVerification"
  }
  ```
- **Responses:**
  - `200 OK` — Verification code regenerated and queued (`Success`).
  - `400 Bad Request` — Invalid request format or unsupported token type.
  - `409 Conflict` — User email is already verified.
  - `429 Too Many Requests` — Rate limit reached for target address.

#### 4.1.8 Forgot Password (Reset Request)
- **HTTP Route:** `POST /api/auth/forgot-password`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Limiter:** `auth-target-strict` (Keyed partitioned sliding window: 3 requests / 15 minutes by target email)
- **Description:** Checks if an account exists for the given email, creates a password reset token, and queues an email notification containing the reset code.
- **Request Body (`ForgotPasswordCommand`):**
  ```json
  {
    "email": "john.doe@example.com"
  }
  ```
- **Responses:**
  - `200 OK` — Reset instructions dispatched (`Success`).
  - `400 Bad Request` — Invalid email address syntax.
  - `429 Too Many Requests` — Reset attempts exceeded for this email.

#### 4.1.9 Reset Password (Confirm Reset)
- **HTTP Route:** `POST /api/auth/reset-password`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Limiter:** `auth-email-strict` (Keyed partitioned sliding window: 5 requests / 1 minute by target email)
- **Description:** Validates the reset code against the stored reset token. If valid, replaces the user's password hash with the new password and revokes the token.
- **Request Body (`ResetPasswordCommand`):**
  ```json
  {
    "email": "john.doe@example.com",
    "token": "654321",
    "newPassword": "NewSecurePassword456!"
  }
  ```
- **Responses:**
  - `200 OK` — Password successfully reset (`Success`).
  - `400 Bad Request` — Weak password or invalid code format.
  - `401 Unauthorized` — Invalid or expired password reset token.
  - `404 Not Found` — Token not found.
  - `429 Too Many Requests` — Verification attempts exhausted.

#### 4.1.10 User Logout
- **HTTP Route:** `POST /api/auth/logout`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `standard` (Sliding window: 100 requests / 1 minute by Client IP)
- **Description:** Revokes the refresh token record in the database matching the cookie and deletes the `refreshToken` cookie by setting its expiration date to the past.
- **Request Body:** None.
- **Responses:**
  - `200 OK` — Session terminated and cookie cleared (`Success`).


---

### 4.2 Carts API (`/api/carts`)

Group Base Route: `/api/carts`  
Authorization: Requires authenticated session with role `Admin` or `Customer`.

```
GET    /api/carts
POST   /api/carts/items/{variantId}
DELETE /api/carts/items/{CartItemId}
```

#### 4.2.1 Get Current User Cart
- **HTTP Route:** `GET /api/carts`
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `authenticated-read` (Sliding window: 60 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Retrieves the shopping cart for the currently authenticated user, calculating item subtotals and the total amount.
- **Request Body:** None.
- **Response Body (`CartDto`):**
  ```json
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "totalAmount": 149.97,
    "items": [
      {
        "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
        "quantity": 3,
        "variant": {
          "id": "44444444-4444-4444-4444-444444444444",
          "size": "L",
          "color": "Black",
          "stockQuantity": 15,
          "product": {
            "id": "11111111-1111-1111-1111-111111111111",
            "name": "Classic Crewneck T-Shirt",
            "imageUrl": "https://cdn.example.com/images/shirt.webp",
            "basePrice": 49.99,
            "discount": 0.0
          }
        }
      }
    ]
  }
  ```
- **Responses:**
  - `200 OK` — Cart retrieved successfully.
  - `401 Unauthorized` — Missing or invalid JWT bearer token.
  - `404 Not Found` — Cart not found for current user.

#### 4.2.2 Add Item to Cart
- **HTTP Route:** `POST /api/carts/items/{variantId}`
- **Route Parameters:**
  - `variantId` (Guid, required) — ID of the product variant to add.
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Adds the specified quantity of a product variant to the current user's cart. If the variant is already present in the cart, increments the existing item quantity.
- **Request Body (`AddCartItemCommand`):**
  ```json
  {
    "variantId": "44444444-4444-4444-4444-444444444444",
    "quantity": 2
  }
  ```
- **Response Body:** `CartDto` (Updated cart structure).
- **Responses:**
  - `200 OK` — Cart updated successfully.
  - `400 Bad Request` — Quantity less than or equal to 0, or variant out of stock.
  - `401 Unauthorized` — Authentication required.
  - `404 Not Found` — Variant or Cart does not exist.

#### 4.2.3 Remove Item from Cart
- **HTTP Route:** `DELETE /api/carts/items/{CartItemId}`
- **Route Parameters:**
  - `CartItemId` (Guid, required) — ID of the cart item line to delete.
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Removes an item line from the current user's cart.
- **Request Body:** None.
- **Responses:**
  - `200 OK` — Item successfully removed (`Success`).
  - `401 Unauthorized` — Authentication required.
  - `404 Not Found` — Cart or item not found.


---

### 4.3 Categories API (`/api/categories`)

Group Base Route: `/api/categories`

```
GET    /api/categories
GET    /api/categories/{categoryId}
POST   /api/categories
PUT    /api/categories/{categoryId}
DELETE /api/categories/{categoryId}
PUT    /api/categories/{categoryId}/subcategories
DELETE /api/categories/{categoryId}/subcategories
```

#### 4.3.1 Get All Categories
- **HTTP Route:** `GET /api/categories`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `standard` (Sliding window: 100 requests / 1 minute by Client IP)
- **Description:** Retrieves the full category hierarchy tree, including subcategories for each category.
- **Request Body:** None.
- **Response Body (`List<CategoryDto>`):**
  ```json
  [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "categoryName": "Men",
      "imageUrl": "https://cdn.example.com/categories/men.webp",
      "subcategories": [
        {
          "id": "22222222-2222-2222-2222-222222222222",
          "categoryName": "T-Shirts",
          "imageUrl": null,
          "subcategories": []
        }
      ]
    }
  ]
  ```
- **Responses:**
  - `200 OK` — Categories retrieved successfully.

#### 4.3.2 Get Category by ID
- **HTTP Route:** `GET /api/categories/{categoryId}`
- **Route Parameters:**
  - `categoryId` (Guid, required) — Target category identifier.
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `authenticated-read` (Sliding window: 60 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Returns the details of a single category, including child subcategories.
- **Response Body (`CategoryDto`):** Single category object.
- **Responses:**
  - `200 OK` — Category retrieved.
  - `401 Unauthorized` — Authentication required.
  - `404 Not Found` — Category not found.

#### 4.3.3 Create Category (Multipart File Upload)
- **HTTP Route:** `POST /api/categories`
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policies:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP) and `upload-concurrency` (Max 4 concurrent uploads, queue: 2)
- **Content-Type:** `multipart/form-data`
- **Description:** Creates a new top-level or standalone category with an optional image file (`.jpg`, `.jpeg`, `.png`, `.webp`, max 5MB).
- **Form Fields (`CreateCategoryRequest`):**
  - `categoryName` (string, required, 3-100 characters): Name of the category.
  - `image` (binary file, optional): Category cover image file.
- **Responses:**
  - `200 OK` — Category created successfully (`CategoryDto`).
  - `400 Bad Request` — Validation failed (e.g. invalid file extension or size > 5MB).
  - `401 Unauthorized` — Missing or invalid token.
  - `403 Forbidden` — Caller lacks `Admin` role.
  - `500 Internal Server Error` — Creation failed.


#### 4.3.4 Update Category (Multipart File Upload)
- **HTTP Route:** `PUT /api/categories/{categoryId}`
- **Route Parameters:**
  - `categoryId` (Guid, required) — ID of the category to update.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policies:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP) and `upload-concurrency` (Max 4 concurrent uploads, queue: 2)
- **Content-Type:** `multipart/form-data`
- **Description:** Updates the category name and/or uploads a replacement image. The old image is marked for background cleanup if replaced.
- **Form Fields (`UpdateCategoryRequest`):**
  - `categoryName` (string, optional): New category name.
  - `image` (binary file, optional): New category image file.
- **Responses:**
  - `200 OK` — Category updated (`CategoryDto`).
  - `400 Bad Request` — Invalid file format or data.
  - `404 Not Found` — Category does not exist.

#### 4.3.5 Delete Category
- **HTTP Route:** `DELETE /api/categories/{categoryId}`
- **Route Parameters:**
  - `categoryId` (Guid, required) — ID of the category to remove.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Deletes a category and enqueues background cleanup for any associated image asset.
- **Responses:**
  - `200 OK` — Category deleted (`Deleted`).
  - `401 Unauthorized` — Authentication required.
  - `403 Forbidden` — Administrator privilege required.
  - `404 Not Found` — Category not found.

#### 4.3.6 Assign Subcategories to Category
- **HTTP Route:** `PUT /api/categories/{categoryId}/subcategories`
- **Route Parameters:**
  - `categoryId` (Guid, required) — Parent category identifier.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Associates a list of existing categories as subcategories of the target parent category.
- **Request Body (`AssignSubCategoriesToCategoryCommand`):**
  ```json
  {
    "categoryId": "11111111-1111-1111-1111-111111111111",
    "subCategoryIds": [
      "22222222-2222-2222-2222-222222222222",
      "33333333-3333-3333-3333-333333333333"
    ]
  }
  ```
- **Responses:**
  - `200 OK` — Subcategories assigned (`Success`).
  - `400 Bad Request` — Category IDs list empty or invalid.
  - `404 Not Found` — Parent category or any subcategory not found.

#### 4.3.7 Unassign Subcategories from Category
- **HTTP Route:** `DELETE /api/categories/{categoryId}/subcategories`
- **Route Parameters:**
  - `categoryId` (Guid, required) — Parent category identifier.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Removes the parent assignment from the listed subcategories, making them standalone categories.
- **Request Body (`UnAssignSubCategoriesToCategoryCommand`):**
  ```json
  {
    "categoryId": "11111111-1111-1111-1111-111111111111",
    "subCategoryIds": [
      "22222222-2222-2222-2222-222222222222"
    ]
  }
  ```
- **Responses:**
  - `200 OK` — Subcategories unassigned (`Success`).
  - `404 Not Found` — Parent or subcategory not found.


---

### 4.4 Products and Variants API (`/api/products`)

Group Base Route: `/api/products`

```
GET    /api/products
GET    /api/products/{productId}
POST   /api/products
PUT    /api/products/{productId}
DELETE /api/products/{productId}
POST   /api/products/{productId}/variants
PUT    /api/products/variants/{variantId}
DELETE /api/products/variants/{variantId}
```

#### 4.4.1 Get Paginated Products (with Filtering & Search)
- **HTTP Route:** `GET /api/products`
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `standard` (Sliding window: 100 requests / 1 minute by Client IP)
- **Description:** Retrieves products matching optional category, text search, price bounds, sizes, colors, and sorting.
- **Query Parameters (`GetProductsRequest`):**
  - `CategoryId` (Guid, optional): Filter by category ID.
  - `PageNumber` (int, optional, default: 1): Page number.
  - `PageSize` (int, optional, default: 10): Items per page.
  - `Search` (string, optional): Search term for product name or description.
  - `MinPrice` (decimal, optional): Minimum price filter.
  - `MaxPrice` (decimal, optional): Maximum price filter.
  - `Sizes` (List<string>, optional): Array of sizes, e.g. `?sizes=S&sizes=M`.
  - `Colors` (List<string>, optional): Array of colors, e.g. `?colors=Black&colors=Red`.
  - `SortBy` (string, optional): Field to sort by (e.g. `price`, `name`).
  - `Descending` (bool, optional, default: false): Sort order direction.
- **Response Body (`PaginatedList<ProductDto>`):**
  ```json
  {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "totalCount": 25,
    "items": [
      {
        "id": "11111111-1111-1111-1111-111111111111",
        "name": "Heavyweight Cotton Hoodie",
        "description": "Premium 450gsm hoodie with kangaroo pocket",
        "basePrice": 89.99,
        "discount": 10.0,
        "categoryId": "22222222-2222-2222-2222-222222222222",
        "variants": [
          {
            "id": "33333333-3333-3333-3333-333333333333",
            "size": "L",
            "color": "Oatmeal",
            "stockQuantity": 12
          }
        ],
        "images": [
          {
            "imageUrl": "https://cdn.example.com/products/hoodie-front.webp",
            "isMain": true
          }
        ]
      }
    ]
  }
  ```
- **Responses:**
  - `200 OK` — Products retrieved successfully.
  - `400 Bad Request` — Invalid query or pagination parameters.

#### 4.4.2 Get Product by ID
- **HTTP Route:** `GET /api/products/{productId}`
- **Route Parameters:**
  - `productId` (Guid, required) — Product unique identifier.
- **Access Control:** Anonymous (`AllowAnonymous`)
- **Rate Limit Policy:** `standard` (Sliding window: 100 requests / 1 minute by Client IP)
- **Description:** Returns a full product record including all child variants and gallery images.
- **Response Body (`ProductDto`):** Full product details.
- **Responses:**
  - `200 OK` — Product found and returned.
  - `400 Bad Request` — Invalid UUID format.
  - `404 Not Found` — Product does not exist.


#### 4.4.3 Create Product (Multipart File Upload & Variants JSON)
- **HTTP Route:** `POST /api/products`
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policies:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP) and `upload-concurrency` (Max 4 concurrent uploads, queue: 2)
- **Content-Type:** `multipart/form-data`
- **Description:** Creates a product entity with optional variants and multiple image attachments. Variants must be serialized as a JSON string within the `VariantsJson` multipart field.
- **Form Fields (`CreateProductRequest`):**
  - `Name` (string, required, 3-150 chars): Product name.
  - `Description` (string, optional, 10-2000 chars): Product description.
  - `BasePrice` (decimal, required, > 0): Product base price in Algerian Dinars (DZD).
  - `Discount` (decimal, optional, 0-10000): Fixed or promotional discount.
  - `CategoryId` (Guid, required): Associated category ID.
  - `VariantsJson` (string, optional): JSON array of variants, e.g.:
    `[{"size":"M","color":"Black","stockQuantity":25},{"size":"L","color":"Navy","stockQuantity":15}]`
  - `Images` (List of binary files, optional): Up to 5MB per image file (`.jpg`, `.jpeg`, `.png`, `.webp`).
  - `MainImageIndex` (int, optional): Zero-based index of the file in `Images` to mark as `IsMain = true`.
- **Responses:**
  - `200 OK` — Product created successfully (`ProductDto`).
  - `400 Bad Request` — Validation failed (invalid variants JSON, unsupported file extension, image > 5MB).
  - `401 Unauthorized` — Authentication missing.
  - `403 Forbidden` — Admin role required.
  - `500 Internal Server Error` — Creation failed.

#### 4.4.4 Update Product Details
- **HTTP Route:** `PUT /api/products/{productId}`
- **Route Parameters:**
  - `productId` (Guid, required) — Product unique identifier.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Updates product metadata: name, description, base price, discount, and category association.
- **Request Body (`UpdateProductCommand`):**
  ```json
  {
    "productId": "11111111-1111-1111-1111-111111111111",
    "name": "Updated Heavyweight Hoodie",
    "description": "Updated product description text...",
    "basePrice": 99.99,
    "discount": 15.0,
    "categoryId": "22222222-2222-2222-2222-222222222222"
  }
  ```
- **Responses:**
  - `200 OK` — Product updated (`ProductDto`).
  - `400 Bad Request` — Validation failed.
  - `404 Not Found` — Product not found.

#### 4.4.5 Delete Product
- **HTTP Route:** `DELETE /api/products/{productId}`
- **Route Parameters:**
  - `productId` (Guid, required) — ID of product to delete.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Deletes a product, its variants, and triggers background cleanup for all linked image files.
- **Responses:**
  - `200 OK` — Product deleted (`Deleted`).
  - `404 Not Found` — Product not found.


#### 4.4.6 Create Product Variant
- **HTTP Route:** `POST /api/products/{productId}/variants`
- **Route Parameters:**
  - `productId` (Guid, required) — Parent product identifier.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Adds a new SKU variant (size, color, initial stock quantity) to an existing product.
- **Request Body (`CreateVariantCommand`):**
  ```json
  {
    "productId": "11111111-1111-1111-1111-111111111111",
    "size": "XL",
    "color": "Charcoal Grey",
    "stockQuantity": 50
  }
  ```
- **Responses:**
  - `200 OK` — Variant created (`VariantDto`).
  - `400 Bad Request` — Stock quantity negative or size/color missing.
  - `404 Not Found` — Parent product not found.

#### 4.4.7 Update Product Variant
- **HTTP Route:** `PUT /api/products/variants/{variantId}`
- **Route Parameters:**
  - `variantId` (Guid, required) — Target variant identifier.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Updates the size, color, or stock quantity of a variant.
- **Request Body (`UpdateVariantCommand`):**
  ```json
  {
    "variantId": "33333333-3333-3333-3333-333333333333",
    "size": "XXL",
    "color": "Charcoal Grey",
    "stockQuantity": 45
  }
  ```
- **Responses:**
  - `200 OK` — Variant updated (`VariantDto`).
  - `400 Bad Request` — Negative stock quantity.
  - `404 Not Found` — Variant not found.

#### 4.4.8 Delete Product Variant
- **HTTP Route:** `DELETE /api/products/variants/{variantId}`
- **Route Parameters:**
  - `variantId` (Guid, required) — Variant identifier.
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Deletes a specific variant SKU from the product catalog.
- **Responses:**
  - `200 OK` — Variant deleted (`Deleted`).
  - `404 Not Found` — Variant not found.


---

### 4.5 Purchases and Orders API (`/api/purchases`)

Group Base Route: `/api/purchases`

```
POST /api/purchases
POST /api/purchases/retry/{purchaseId}
GET  /api/purchases
POST /api/purchases/webhook/payment
```

#### 4.5.1 Create Purchase (Initiate Checkout)
- **HTTP Route:** `POST /api/purchases`
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `purchase-strict` (Sliding window: 10 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Creates an order in `Pending` state for the authenticated customer. Validates stock for all variants, locks stock quantities, creates a pending payment record, calls the Chargily Pay API to create a checkout session, and returns the checkout URL. If `Origin == "Cart"`, removes the purchased items from the user's cart.
- **Request Body (`CreatePurchaseRequest`):**
  ```json
  {
    "customerPhone": "0555123456",
    "street": "12 Didouche Mourad",
    "city": "Algiers",
    "wilaya": "Alger",
    "origin": "Cart",
    "purchaseItems": [
      {
        "variantId": "33333333-3333-3333-3333-333333333333",
        "quantity": 2
      }
    ]
  }
  ```
- **Validation Rules:**
  - `CustomerPhone`: Valid Algerian mobile phone (`^0[5-7][0-9]{8}$`).
  - `Origin`: Either `"BuyNow"` or `"Cart"`.
  - `Street`, `City`, `Wilaya`: Non-empty strings.
  - `PurchaseItems`: At least 1 item; maximum quantity 50 per item; no duplicate `variantId` entries.
- **Response Body (`CreatePurchaseResult`):**
  ```json
  {
    "purchaseId": "99999999-9999-9999-9999-999999999999",
    "checkoutUrl": "https://pay.chargily.net/test/checkouts/chk_live_12345"
  }
  ```
- **Responses:**
  - `200 OK` — Order created and checkout session initiated.
  - `400 Bad Request` — Validation failure, address invalid, or variant stock insufficient.
  - `401 Unauthorized` — Authentication required.
  - `404 Not Found` — Variant not found.
  - `429 Too Many Requests` — Checkout rate limit exceeded.
  - `500 Internal Server Error` — Chargily payment gateway failure.


#### 4.5.2 Retry Purchase Payment
- **HTTP Route:** `POST /api/purchases/retry/{purchaseId}`
- **Route Parameters:**
  - `purchaseId` (Guid, required) — ID of the purchase to retry.
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `purchase-strict` (Sliding window: 10 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Allows a customer to re-attempt payment for a purchase in `Pending` or `Failed` status. Re-validates variant stock, registers a new payment attempt, requests a new Chargily checkout URL, and returns the URL.
- **Request Body:** None.
- **Response Body (`RetryPurchaseResult`):**
  ```json
  {
    "checkoutUrl": "https://pay.chargily.net/test/checkouts/chk_live_98765"
  }
  ```
- **Responses:**
  - `200 OK` — New payment session initiated.
  - `400 Bad Request` — Order is already paid or cannot be retried.
  - `401 Unauthorized` — Authentication required.
  - `403 Forbidden` — Purchase belongs to another user.
  - `404 Not Found` — Purchase or payment not found.
  - `409 Conflict` — Insufficient stock available to retry.

#### 4.5.3 Get All Purchases (Admin Only)
- **HTTP Route:** `GET /api/purchases`
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-read` (Sliding window: 60 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Returns a paginated list of all customer purchases ordered chronologically.
- **Query Parameters (`GetPurchasesQuery`):**
  - `PageNumber` (int, default: 1): Page index.
  - `PageSize` (int, default: 10): Records per page.
- **Response Body (`PaginatedList<PurchaseDto>`):**
  ```json
  {
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2,
    "totalCount": 18,
    "items": [
      {
        "purchaseId": "99999999-9999-9999-9999-999999999999",
        "customerName": "John Doe",
        "customerPhoneNumber": "0555123456",
        "wilaya": "Alger",
        "city": "Algiers",
        "street": "12 Didouche Mourad",
        "totalAmount": 160.00,
        "status": "Completed",
        "date": "2026-09-25T14:32:00+00:00",
        "purchaseItems": [
          {
            "productName": "Heavyweight Cotton Hoodie",
            "quantity": 2,
            "unitPrice": 80.00
          }
        ]
      }
    ]
  }
  ```
- **Responses:**
  - `200 OK` — Purchases list retrieved.
  - `401 Unauthorized` — Authentication required.
  - `403 Forbidden` — Administrator access required.

#### 4.5.4 Payment Webhook (Chargily Pay Callback)
- **HTTP Route:** `POST /api/purchases/webhook/payment`
- **Access Control:** Anonymous (`AllowAnonymous`) — protected by HMAC-SHA256 signature middleware.
- **Swagger Documentation:** Hidden from public Swagger documentation (`ExcludeFromDescription()`).
- **Description:** Receives payment status notifications from Chargily Pay. The application checks terminal states (`paid`, `failed`, `expired`, `canceled`), updates the payment and purchase states, deducts product variant stock when paid, or restores reserved stock if payment failed or expired.
- **Security:** Incoming request signature is validated by `ChargilyPayWebhookValidationMiddleware` against the configured secret key before entering FastEndpoints.
- **Request Body (`ChargilyWebhookRequest`):**
  ```json
  {
    "id": "evt_live_123456",
    "entity": "event",
    "type": "checkout.paid",
    "data": {
      "id": "chk_live_12345",
      "amount": 160.00,
      "currency": "dzd",
      "status": "paid",
      "metadata": []
    }
  }
  ```
- **Responses:**
  - `200 OK` — Webhook processed successfully (`Updated`).
  - `400 Bad Request` — Signature header invalid or payload malformed.
  - `404 Not Found` — Checkout ID does not match any known payment.
  - `500 Internal Server Error` — Database transaction failed.


---

### 4.6 Dashboard API (`/api/dashboard`)

Group Base Route: `/api/dashboard`

```
GET /api/dashboard/overview
```

#### 4.6.1 Get Dashboard Overview Metrics
- **HTTP Route:** `GET /api/dashboard/overview`
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-read` (Sliding window: 60 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Aggregates top-level operational counters for the admin dashboard.
- **Request Body:** None.
- **Response Body (`OverviewDto`):**
  ```json
  {
    "totalPurchases": 1420,
    "totalProducts": 84,
    "totalUsers": 512,
    "totalCategories": 12
  }
  ```
- **Responses:**
  - `200 OK` — Metrics retrieved successfully.
  - `401 Unauthorized` — Authentication required.
  - `403 Forbidden` — Administrator access required.

---

### 4.7 Users API (`/api/users`)

Group Base Route: `/api/users`

```
GET  /api/users/current
PUT  /api/users/current
GET  /api/users
POST /api/users
```

#### 4.7.1 Get Current User Profile
- **HTTP Route:** `GET /api/users/current`
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `authenticated-read` (Sliding window: 60 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Returns the profile details for the currently authenticated user identified by the JWT claims.
- **Request Body:** None.
- **Response Body (`UserDto`):**
  ```json
  {
    "userId": "11111111-1111-1111-1111-111111111111",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "role": "Customer",
    "phoneNumber": "0555123456",
    "isEmailVerified": true
  }
  ```
- **Responses:**
  - `200 OK` — Profile retrieved.
  - `401 Unauthorized` — Authentication required.
  - `404 Not Found` — User not found.

#### 4.7.2 Update Current User Profile
- **HTTP Route:** `PUT /api/users/current`
- **Access Control:** `Roles("Admin", "Customer")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Updates the first name, last name, and phone number of the authenticated user.
- **Request Body (`UpdateCurrentUserProfileCommand`):**
  ```json
  {
    "firstName": "Johnny",
    "lastName": "Doe",
    "phoneNumber": "0666987654"
  }
  ```
- **Response Body:** `UserDto` (Updated profile).
- **Responses:**
  - `200 OK` — Profile updated successfully.
  - `400 Bad Request` — Validation failed (e.g. invalid phone number).
  - `401 Unauthorized` — Authentication required.
  - `404 Not Found` — User not found.


#### 4.7.3 Get All Users (Admin Only)
- **HTTP Route:** `GET /api/users`
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-read` (Sliding window: 60 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Returns a paginated collection of all registered user accounts.
- **Query Parameters (`GetAllUsersQuery`):**
  - `PageNumber` (int, default: 1): Page number.
  - `PageSize` (int, default: 10): Items per page.
- **Response Body (`PaginatedList<UserDto>`):** Paginated envelope of user accounts.
- **Responses:**
  - `200 OK` — Users list retrieved.
  - `401 Unauthorized` — Authentication required.
  - `403 Forbidden` — Administrator access required.

#### 4.7.4 Create User Account (Admin Only)
- **HTTP Route:** `POST /api/users`
- **Access Control:** `Roles("Admin")`
- **Rate Limit Policy:** `authenticated-write` (Sliding window: 30 requests / 1 minute by JWT `sub`, falling back to IP)
- **Description:** Allows an administrator to create a user account directly with an assigned role (`Customer` or `Admin`).
- **Request Body (`CreateUserCommand`):**
  ```json
  {
    "firstName": "Jane",
    "lastName": "Admin",
    "phoneNumber": "0770112233",
    "email": "jane.admin@example.com",
    "password": "TemporaryAdminPass123!",
    "role": "Admin"
  }
  ```
- **Response Headers:**
  ```http
  Location: /api/users/22222222-2222-2222-2222-222222222222
  ```
- **Response Body:** `UserDto` (HTTP 201 Created).
- **Responses:**
  - `201 Created` — User created successfully.
  - `400 Bad Request` — Invalid payload or password does not meet complexity requirements.
  - `401 Unauthorized` — Authentication required.
  - `403 Forbidden` — Administrator role required.
  - `409 Conflict` — An account with this email address already exists.


---

## 5. Rate Limiting Policy Matrix

The API defines **8 named rate limiting policies** and **2 keyed partition limiters** in `backend/src/Api/DependencyInjection.cs`:

| Policy / Limiter | Type | Partition Key | Limit & Window | Queued Requests |
| --- | --- | --- | --- | --- |
| `standard` | Sliding Window (4 segments) | Client IP | 100 requests / 1 minute | 0 |
| `auth-ip-create` | Sliding Window (4 segments) | Client IP | 10 requests / 1 minute | 0 |
| `auth-ip-relaxed` | Sliding Window (4 segments) | Client IP | 30 requests / 1 minute | 0 |
| `auth-ip-spray-guard` | Sliding Window (4 segments) | Client IP | 20 requests / 1 minute | 0 |
| `authenticated-read` | Sliding Window (4 segments) | JWT `sub`, falling back to Client IP | 60 requests / 1 minute | 0 |
| `authenticated-write` | Sliding Window (4 segments) | JWT `sub`, falling back to Client IP | 30 requests / 1 minute | 0 |
| `purchase-strict` | Sliding Window (4 segments) | JWT `sub`, falling back to Client IP | 10 requests / 1 minute | 0 |
| `upload-concurrency` | Concurrency | Shared `"uploads"` partition (all callers) | 4 concurrent requests | 2 (`OldestFirst`) |
| `auth-email-strict` (keyed) | Sliding Window (4 segments) | Target email | 5 requests / 1 minute | 0 |
| `auth-target-strict` (keyed) | Sliding Window (3 segments) | Target email / identity | 3 requests / 15 minutes | 0 |

Per-endpoint assignments:

| Endpoint Route | HTTP Method | Rate Limiting Policy / Keyed Limiter | Type | Policy Limit & Window |
| --- | --- | --- | --- | --- |
| `/api/auth/register` | `POST` | `auth-ip-create` | Sliding Window | 10 requests / 1 minute by Client IP |
| `/api/auth/verify-email` | `POST` | `auth-email-strict` | Keyed Partition (Sliding Window) | 5 requests / 1 minute by Target Email |
| `/api/auth/login` | `POST` | `auth-ip-spray-guard` + `auth-email-strict` | Sliding Window + Keyed Partition | IP: 20 / 1 min; Email: 5 / 1 min |
| `/api/auth/google/register` | `POST` | `auth-ip-relaxed` | Sliding Window | 30 requests / 1 minute by Client IP |
| `/api/auth/google/login` | `POST` | `auth-ip-relaxed` | Sliding Window | 30 requests / 1 minute by Client IP |
| `/api/auth/refresh` | `POST` | `auth-ip-relaxed` | Sliding Window | 30 requests / 1 minute by Client IP |
| `/api/auth/resend-code` | `POST` | `auth-target-strict` | Keyed Partition (Sliding Window) | 3 requests / 15 minutes by Target Email |
| `/api/auth/forgot-password` | `POST` | `auth-target-strict` | Keyed Partition (Sliding Window) | 3 requests / 15 minutes by Target Email |
| `/api/auth/reset-password` | `POST` | `auth-email-strict` | Keyed Partition (Sliding Window) | 5 requests / 1 minute by Target Email |
| `/api/auth/logout` | `POST` | `standard` | Sliding Window | 100 requests / 1 minute by Client IP |
| `/api/carts` | `GET` | `authenticated-read` | Sliding Window | 60 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/carts/items/{variantId}` | `POST` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/carts/items/{cartItemId}` | `DELETE` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/categories` | `GET` | `standard` | Sliding Window | 100 requests / 1 minute by Client IP |
| `/api/categories/{categoryId}` | `GET` | `authenticated-read` | Sliding Window | 60 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/categories` | `POST` | `authenticated-write` + `upload-concurrency` | Sliding Window + Concurrency | Write: 30/min; Concurrency: 4 max concurrent (queue: 2) |
| `/api/categories/{categoryId}` | `PUT` | `authenticated-write` + `upload-concurrency` | Sliding Window + Concurrency | Write: 30/min; Concurrency: 4 max concurrent (queue: 2) |
| `/api/categories/{categoryId}` | `DELETE` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/categories/{categoryId}/subcategories` | `PUT` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/categories/{categoryId}/subcategories` | `DELETE` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/dashboard/overview` | `GET` | `authenticated-read` | Sliding Window | 60 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/products` | `GET` | `standard` | Sliding Window | 100 requests / 1 minute by Client IP |
| `/api/products/{productId}` | `GET` | `standard` | Sliding Window | 100 requests / 1 minute by Client IP |
| `/api/products` | `POST` | `authenticated-write` + `upload-concurrency` | Sliding Window + Concurrency | Write: 30/min; Concurrency: 4 max concurrent (queue: 2) |
| `/api/products/{productId}` | `PUT` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/products/{productId}` | `DELETE` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/products/{productId}/variants` | `POST` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/products/variants/{variantId}` | `PUT` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/products/variants/{variantId}` | `DELETE` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/purchases` | `POST` | `purchase-strict` | Sliding Window | 10 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/purchases/retry/{purchaseId}` | `POST` | `purchase-strict` | Sliding Window | 10 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/purchases` | `GET` | `authenticated-read` | Sliding Window | 60 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/purchases/webhook/payment` | `POST` | HMAC Middleware (No Rate Limit) | Signature Gate | Validated via Chargily HMAC-SHA256 signature |
| `/api/users/current` | `GET` | `authenticated-read` | Sliding Window | 60 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/users/current` | `PUT` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/users` | `GET` | `authenticated-read` | Sliding Window | 60 requests / 1 minute by JWT `sub` (falls back to IP) |
| `/api/users` | `POST` | `authenticated-write` | Sliding Window | 30 requests / 1 minute by JWT `sub` (falls back to IP) |


---

## 6. Multipart Form Upload Specifications

The API handles binary image uploads in two specific endpoints:
1. `POST /api/categories` & `PUT /api/categories/{categoryId}`
2. `POST /api/products`

### 6.1 Upload Validation Rules

- **Allowed File Formats:** `.jpg`, `.jpeg`, `.png`, `.webp`
- **Maximum File Size:** `5 MB` (`5 * 1024 * 1024` bytes per file).
- **Rate Limit & Concurrency:** Protected by the `upload-concurrency` limiter (maximum 4 concurrent uploads in flight, queue capacity: 2).
- **Storage Lifecycle:** 
  - Uploaded streams are stored locally or to disk based on `ImageStorageOptions`.
  - Replaced or deleted image paths are tracked in `BackgroundJobTracker` and deleted asynchronously by the `ImageCleanupJob` every 24 hours.

### 6.2 Product Creation Multipart Format

Because product creation combines structured entities (variants) with binary streams (images), it requires a specialized form-data layout:

```http
POST /api/products HTTP/1.1
Host: localhost:7146
Authorization: Bearer <token>
Content-Type: multipart/form-data; boundary=----WebKitFormBoundaryXyZ123

------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="Name"

Classic Heavyweight Hoodie
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="Description"

450gsm luxury cotton fleece hoodie
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="BasePrice"

89.99
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="Discount"

10.00
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="CategoryId"

22222222-2222-2222-2222-222222222222
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="MainImageIndex"

0
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="VariantsJson"

[{"Size":"M","Color":"Black","StockQuantity":20},{"Size":"L","Color":"Black","StockQuantity":35}]
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="Images"; filename="hoodie-front.webp"
Content-Type: image/webp

<binary file content>
------WebKitFormBoundaryXyZ123
Content-Disposition: form-data; name="Images"; filename="hoodie-back.webp"
Content-Type: image/webp

<binary file content>
------WebKitFormBoundaryXyZ123--
```


---

## 7. Data Transfer Objects (DTO) Catalog

### 7.1 Common Response Models

#### `Success`
```json
{
  "value": "Operation completed successfully."
}
```

#### `Deleted`
```json
{
  "value": "Entity was successfully deleted."
}
```

#### `Updated`
```json
{
  "value": "Entity was successfully updated."
}
```

---

### 7.2 Identity and Authentication Models

#### `LoginResponse`
```json
{
  "user": {
    "userId": "11111111-1111-1111-1111-111111111111",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "role": "Customer",
    "phoneNumber": "0555123456",
    "isEmailVerified": true
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### `UserDto`
```json
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "role": "string",
  "phoneNumber": "string",
  "isEmailVerified": true
}
```

---

### 7.3 Catalog Models

#### `CategoryDto`
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "categoryName": "string",
  "imageUrl": "string | null",
  "subcategories": [
    {
      "id": "22222222-2222-2222-2222-222222222222",
      "categoryName": "string",
      "imageUrl": "string | null",
      "subcategories": []
    }
  ]
}
```

#### `ProductDto`
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "string",
  "description": "string",
  "basePrice": 0.0,
  "discount": 0.0,
  "categoryId": "22222222-2222-2222-2222-222222222222",
  "variants": [
    {
      "id": "33333333-3333-3333-3333-333333333333",
      "size": "string",
      "color": "string",
      "stockQuantity": 0
    }
  ],
  "images": [
    {
      "imageUrl": "string",
      "isMain": true
    }
  ]
}
```

#### `VariantDto`
```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "size": "string",
  "color": "string",
  "stockQuantity": 0
}
```


---

### 7.4 Cart Models

#### `CartDto`
```json
{
  "id": "44444444-4444-4444-4444-444444444444",
  "totalAmount": 0.0,
  "items": [
    {
      "id": "55555555-5555-5555-5555-555555555555",
      "quantity": 0,
      "variant": {
        "id": "33333333-3333-3333-3333-333333333333",
        "size": "string",
        "color": "string",
        "stockQuantity": 0,
        "product": {
          "id": "11111111-1111-1111-1111-111111111111",
          "name": "string",
          "imageUrl": "string | null",
          "basePrice": 0.0,
          "discount": 0.0
        }
      }
    }
  ]
}
```

---

### 7.5 Purchase and Checkout Models

#### `CreatePurchaseResult`
```json
{
  "purchaseId": "99999999-9999-9999-9999-999999999999",
  "checkoutUrl": "https://pay.chargily.net/test/checkouts/chk_live_12345"
}
```

#### `RetryPurchaseResult`
```json
{
  "checkoutUrl": "https://pay.chargily.net/test/checkouts/chk_live_98765"
}
```

#### `PurchaseDto`
```json
{
  "purchaseId": "99999999-9999-9999-9999-999999999999",
  "customerName": "string",
  "customerPhoneNumber": "string | null",
  "wilaya": "string",
  "city": "string",
  "street": "string",
  "totalAmount": 0.0,
  "status": "Pending | Completed | Failed | Canceled",
  "date": "2026-09-25T14:32:00+00:00",
  "purchaseItems": [
    {
      "quantity": 0,
      "productName": "string",
      "unitPrice": 0.0
    }
  ]
}
```

---

### 7.6 Dashboard Models

#### `OverviewDto`
```json
{
  "totalPurchases": 0,
  "totalProducts": 0,
  "totalUsers": 0,
  "totalCategories": 0
}
```

