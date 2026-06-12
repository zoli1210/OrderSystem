# Authentication and Authorization

OrderSystem uses JWT Bearer authentication with ASP.NET Core Identity.

Authenticated users receive a JWT token after login and send it with protected requests.

```text
Authorization: Bearer {token}
```

## Main Authentication Endpoints

```text
POST /auth/register
POST /auth/login
GET  /auth/users
PUT  /auth/users/{userId}/role
```

## Supported Roles

```text
Admin
Manager
TeamLead
Support
User
```

## Role Hierarchy

Role assignment follows a hierarchy.

```text
Admin
  → can assign Manager, TeamLead, Support, User

Manager
  → can assign TeamLead, Support, User

TeamLead
  → can assign Support, User

Support
  → cannot assign roles

User
  → cannot assign roles
```

## User Access Rules

Orders are connected to the authenticated user who created them.

```text
User  → can access only their own orders
Admin → can access all orders
```

This rule applies to:

- order details
- order lists
- order status history
- order email history
- AI order explanation

## Admin-only Operations

Admin-only operations include:

```text
GET  /auth/users
PUT  /auth/users/{userId}/role

PATCH /orders/{orderId}/status

GET  /dead-letters/orders
GET  /dead-letters/emails
POST /dead-letters/orders/{sequenceNumber}/retry
POST /dead-letters/emails/{sequenceNumber}/retry

POST /ai/knowledge/documents
```

## Role Management

Role updates are handled through:

```text
PUT /auth/users/{userId}/role
```

The role permission service validates whether the current user is allowed to assign the requested target role.

## Current User Handling

The application uses a current user service abstraction to access information about the authenticated user.

This keeps identity access out of controllers and allows application services to enforce authorization rules consistently.

The current user service can provide:

- current user id
- current user role
- admin check
- authenticated user context

## Authorization in Order Features

Normal users can work only with their own orders.

Admins can access and manage all orders.

This matters especially for:

```text
GET /orders
GET /orders/{id}
GET /orders/user-history
GET /orders/{id}/status-history
GET /orders/{id}/email-history
POST /orders/{id}/cancel
POST /orders/{id}/retry-payment
POST /ai/orders/{orderId}/explain
```

## Authorization in AI Features

The AI order explainer must never expose data that the user could not access through the normal order endpoints.

The AI feature can explain an order only after the same ownership/admin checks have passed.

## Security Notes

Secrets must not be committed to source control.

Sensitive values include:

```text
JWT signing key
SQL connection strings
Azure Service Bus connection strings
Azure Storage connection strings
Azure Communication Services connection strings
OpenAI API key
Supabase secret key
Application Insights connection string
```

The Supabase secret key is backend-only and must not be exposed to client applications.

## Recommended Security Rules

- Keep JWT signing keys outside public source control.
- Keep local development secrets in ignored local files or environment variables.
- Use role-based policies for protected operations.
- Keep admin-only endpoints explicitly protected.
- Do not trust queue messages as source of truth.
- Always reload important business state from SQL Server.
- Enforce ownership checks before returning order data.
- Apply the same access rules to AI endpoints as to normal API endpoints.