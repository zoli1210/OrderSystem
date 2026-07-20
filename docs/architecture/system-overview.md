# System Overview

OrderSystem is a modular .NET 8 backend system built around event-driven order processing.

The system has two runtime projects:

```text
OrderSystem
OrderSystem.AzureFunctions
```

The solution also contains a shared class library:

```text
OrderSystem.Shared
```

`OrderSystem.Shared` is not a runtime project. It contains code used by both the API and Azure Functions.

Current project dependency direction:

```text
OrderSystem                -> OrderSystem.Shared
OrderSystem.AzureFunctions -> OrderSystem.Shared
```

Azure Functions must not reference the API project directly.

## Project Responsibilities

### OrderSystem

The API project is responsible for synchronous operations.

It handles:

- HTTP API endpoints
- request validation
- authentication
- authorization
- order creation
- order querying
- order cancellation
- payment retry requests
- manual order status updates
- order status history
- email notification history
- Azure Service Bus message publishing
- dead-letter inspection and retry endpoints
- health checks
- AI knowledge assistant endpoints
- AI order process explanation endpoints

### OrderSystem.AzureFunctions

The Azure Functions project is responsible for asynchronous background processing.

It handles:

- processing payment messages
- updating order status after payment processing
- publishing order status changed messages
- publishing email notification messages
- sending emails
- starting Durable fulfillment workflows
- raising Durable external events
- sending fulfillment timeout alerts

### OrderSystem.Shared

The shared project contains code used by both runtime projects.

It contains:

- domain entities
- domain enums
- EF Core `AppDbContext`
- EF Core entity configurations
- repository interfaces and implementations
- Azure Service Bus message contracts
- Azure Service Bus message senders
- dead-letter service
- payment abstractions and payment service
- email service abstractions
- Azure Communication Services email implementation
- shared authentication persistence model required by `AppDbContext`

`OrderSystem.Shared` is built and deployed together with the API and the Functions app. It is not deployed as an independent service.

## High-level Architecture

```text
Client
  ↓
OrderSystem API
  ↓
OrderSystem.Shared
  ↓
Azure SQL Database
  ↓
Azure Service Bus
  ↓
OrderSystem.AzureFunctions
  ↓
Durable Functions / Azure Communication Services
```

Runtime deployment:

```text
OrderSystem API            -> Azure App Service
OrderSystem.AzureFunctions -> Azure Function App
OrderSystem.Shared         -> shared library, deployed with both runtimes
Database                   -> Azure SQL Database
Messaging                  -> Azure Service Bus
Durable state              -> Azure Storage Account
Monitoring                 -> Application Insights
Email                      -> Azure Communication Services
```

## Runtime Communication

The API does not directly execute every long-running process.

Instead, it saves the business state to SQL Server or Azure SQL Database and publishes messages to Azure Service Bus.

Azure Functions consume these messages and perform background work.

```text
API request
  ↓
SQL state change
  ↓
Service Bus message
  ↓
Function processing
  ↓
SQL state change
  ↓
Optional next message
```

## Main Queues

```text
order-created
order-status-changed
email-notification
```

### order-created

Used when a new order is created or when payment is retried.

The payment processor listens to this queue.

### order-status-changed

Used when an order reaches a status that should trigger follow-up workflow behavior.

The order status changed function listens to this queue.

It can:

- start the fulfillment workflow when the order reaches `Paid`
- raise Durable external events for fulfillment statuses
- publish status email notification messages

### email-notification

Used when the system needs to send an email.

The email processor listens to this queue.

## Source of Truth

SQL Server or Azure SQL Database is the source of truth for order data.

The database stores:

- orders
- current order status
- order status history
- email notification history
- identity users
- identity roles

Azure Service Bus messages are not the source of truth.

They are transport messages used to trigger background processing.

Because Service Bus messages can be duplicated, retried, or dead-lettered, processors must always reload the latest business state from SQL before making decisions.

Supabase/vector data is not the source of truth either.

It is used only as supporting documentation context for AI features.

## Architectural Style

The project is designed as a modular monolith with a separate background-processing runtime.

That means:

- the system is not split into multiple microservices
- the API and Functions are deployed separately
- shared business and infrastructure code is placed in `OrderSystem.Shared`
- feature boundaries are still respected
- infrastructure concerns are separated from business rules
- async processing is used where it gives real value

This is a practical architecture for a portfolio backend because it demonstrates real backend concepts without creating unnecessary distributed-system complexity.

## Recommended Internal Layering

The solution is structured around these logical areas:

```text
Api
Application
Domain
Infrastructure
Common
OrderSystem.Shared
```

### Api

Contains HTTP-specific code:

- controllers
- middleware
- request/response handling
- API-level configuration
- Swagger configuration
- health check endpoint mapping

### Application

Contains use-case logic:

- services
- validators
- DTOs
- request/response contracts
- application abstractions
- permission checks
- orchestration between domain and infrastructure

### Domain

Contains business models and business rules:

- entities
- enums
- valid status transitions
- domain-level validation

Domain code that is needed by both the API and Functions lives in `OrderSystem.Shared`.

### Infrastructure

Contains external technical implementations:

- EF Core DbContext
- repositories
- Azure Service Bus
- Azure Communication Services
- OpenAI
- Supabase
- configuration options

Infrastructure code that is needed by both the API and Functions lives in `OrderSystem.Shared`.

API-only infrastructure registration remains in the API project.

Function-specific dependency registration remains in the Azure Functions project.

### Common

Contains small shared building blocks used by the API layer, such as:

- pagination models
- shared response structures
- common API helpers

### OrderSystem.Shared

Contains code shared by both runtime projects:

```text
Domain
Infrastructure/Persistence
Infrastructure/Messaging
Application/Payments
Application/EmailNotifications
Application/Auth/Domain
```

The namespace names do not need to include `Shared`.

For example, code inside `OrderSystem.Shared` can still use namespaces such as:

```text
OrderSystem.Domain.Entities
OrderSystem.Repository.Persistence
OrderSystem.Infrastructure.Messaging
OrderSystem.Modules.Email.Services
OrderSystem.Modules.Payments.Services
```

The physical project is shared, but the logical namespace can remain aligned with the existing architecture.

## Main System Flow

```text
1. User creates an order through the API.
2. API validates the request.
3. API saves the order with Pending status.
4. API publishes OrderCreatedMessage to order-created queue.
5. PaymentProcessorFunction receives the message.
6. PaymentProcessor loads the order from SQL.
7. PaymentProcessor moves the order to PaymentProcessing.
8. PaymentService processes the payment.
9. Order moves to Paid or Failed.
10. If payment succeeds, OrderStatusChangedMessage is published.
11. Payment confirmation email message is published.
12. Fulfillment workflow starts when the order reaches Paid.
13. Admin updates fulfillment statuses through the API.
14. Status changes are saved to SQL.
15. Status changes are sent as Service Bus messages.
16. Durable workflow receives external events.
17. Status email messages are published when configured.
18. EmailNotificationFunction sends the emails.
19. EmailNotificationHistory stores the result.
```

## Payment Processing

Payment processing is asynchronous.

```text
POST /orders
  ↓
Order is saved with Pending status
  ↓
OrderCreatedMessage is published
  ↓
PaymentProcessorFunction receives the message
  ↓
Order is loaded from SQL
  ↓
Pending -> PaymentProcessing
  ↓
PaymentService processes payment
  ↓
PaymentProcessing -> Paid or Failed
```

Payment processing should continue only if the order is currently in `Pending`.

```text
if order.Status != Pending
  -> skip processing
```

This protects the system from duplicate Service Bus messages and retry scenarios.

If payment succeeds but a later side effect fails, such as sending a status changed message, the processor must not move the order from `Paid` to `Failed`.

Payment failure should only be applied while the order is still in `PaymentProcessing`.

## Fulfillment Workflow

Fulfillment starts after successful payment.

```text
Paid
  ↓
Preparing
  ↓
ReadyForShipment
  ↓
Shipped
  ↓
Delivered
```

The fulfillment process is coordinated by Durable Functions.

The workflow uses a deterministic instance id based on the order id.

This prevents accidentally starting multiple fulfillment workflows for the same order.

## Durable External Events

Manual status changes are made through the API.

After the API saves the new status, it publishes an order status changed message.

The function receives the message and raises the matching Durable external event.

The workflow waits for these fulfillment status events:

```text
Preparing
ReadyForShipment
Shipped
Delivered
```

## Email Notifications

Emails are sent asynchronously through the `email-notification` queue.

```text
Business event happens
  ↓
EmailNotificationMessage is published
  ↓
EmailNotificationFunction receives the message
  ↓
EmailProcessor loads the related order
  ↓
Email is sent through Azure Communication Services
  ↓
Email notification history is saved
```

Current email types:

```text
PaymentConfirmation
Preparing
ReadyForShipment
Shipped
Delivered
```

`PaymentConfirmation` is sent after successful payment.

The fulfillment status email types are sent after manual/admin status changes.

## Email Idempotency

The email processor checks whether the same email type was already sent for the same order.

```text
OrderId + EmailType + Sent
  -> send only once
```

This means one order can receive multiple different emails:

```text
PaymentConfirmation
Preparing
ReadyForShipment
Shipped
Delivered
```

But the same email type should not be sent twice for the same order.

If `EmailType` is missing, the email processor treats the message as `PaymentConfirmation`.

Therefore status email messages must always set their own `EmailType`.

## Health Check

The API exposes:

```text
GET /health
```

The health check validates the currently configured infrastructure.

It checks:

- SQL database connectivity
- Azure Service Bus queue availability

The health check uses the active runtime configuration.

If the API connection string points to LocalDB, it checks LocalDB.

If the API connection string points to Azure SQL Database, it checks Azure SQL Database.

The periodic health check runs after startup and then on the configured interval.

## Important Design Decisions

### API remains the owner of manual actions

Manual operations such as cancellation, payment retry, and status updates are performed through the API.

Functions react to events but should not become the main public command surface.

### Functions do not reference the API project

Azure Functions use `OrderSystem.Shared` for shared domain, persistence, messaging, payment, and email logic.

They must not reference the API project directly.

This keeps the background worker independent from HTTP controllers, Swagger setup, middleware, and API-only configuration.

### SQL state is always more important than queue state

A message can be duplicated, retried, or dead-lettered.

Because of this, processors must load the latest order state from SQL before making decisions.

### Long-running workflow uses Durable Functions

The fulfillment process can take hours or days.

Durable Functions are used to wait for status changes and trigger timeout alerts without blocking normal application threads.

### Email sending is asynchronous

Emails are not sent inside the HTTP request flow.

The API or Functions publish an email notification message, and the email processor sends the email in the background.

### Shared project is a deployment dependency, not a service

`OrderSystem.Shared` is a class library.

It has no runtime host, no endpoint, and no independent Azure resource.

It is included when building and publishing the API and Functions.