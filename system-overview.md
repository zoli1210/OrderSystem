# System Overview

OrderSystem is a modular .NET 8 backend system built around event-driven order processing.

The system has two runtime projects:

```text
OrderSystem
OrderSystem.AzureFunctions
```

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
- SQL persistence
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

## High-level Architecture

```text
Client
  ↓
ASP.NET Core Web API
  ↓
SQL Server
  ↓
Azure Service Bus
  ↓
Azure Functions
  ↓
Durable Functions
  ↓
Azure Communication Services
```

## Runtime Communication

The API does not directly execute every long-running process.

Instead, it saves the business state to SQL Server and publishes messages to Azure Service Bus.

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

The fulfillment workflow listens to this queue.

### email-notification

Used when the system needs to send an email.

The email processor listens to this queue.

## Source of Truth

SQL Server is the source of truth for order data.

The database stores:

- orders
- current order status
- order status history
- email notification history
- identity users
- identity roles

Azure Service Bus messages are not the source of truth.

They are transport messages used to trigger background processing.

Supabase/vector data is not the source of truth either.

It is used only as supporting documentation context for AI features.

## Architectural Style

The project is designed as a modular monolith.

That means:

- the system is not split into multiple microservices
- the codebase stays understandable
- feature boundaries are still respected
- infrastructure concerns are separated from business rules
- async processing is used where it gives real value

This is a practical architecture for a portfolio backend because it demonstrates real backend concepts without creating unnecessary distributed-system complexity.

## Recommended Internal Layering

The API project should be structured around these logical areas:

```text
Api
Application
Domain
Infrastructure
SharedKernel
```

### Api

Contains HTTP-specific code:

- controllers
- middleware
- request/response handling
- API-level configuration

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

### Infrastructure

Contains external technical implementations:

- EF Core DbContext
- repositories
- Azure Service Bus
- Azure Communication Services
- OpenAI
- Supabase
- health checks
- configuration options

### SharedKernel

Contains small shared building blocks that are not owned by a specific feature:

- common exceptions
- pagination models
- shared result structures

## Main System Flow

```text
1. User creates an order through the API.
2. API validates the request.
3. API saves the order with Pending status.
4. API publishes OrderCreatedMessage.
5. PaymentProcessorFunction receives the message.
6. PaymentProcessor moves the order to PaymentProcessing.
7. PaymentService processes the payment.
8. Order moves to Paid or Failed.
9. If payment succeeds, OrderStatusChangedMessage is published.
10. Fulfillment workflow starts when the order reaches Paid.
11. Admin updates fulfillment statuses through the API.
12. Status changes are sent as Service Bus messages.
13. Durable workflow receives external events.
14. Email messages are published when customer/admin communication is needed.
15. EmailNotificationFunction sends the emails.
```

## Important Design Decisions

### API remains the owner of manual actions

Manual operations such as cancellation, payment retry, and status updates are performed through the API.

Functions react to events but should not become the main public command surface.

### SQL state is always more important than queue state

A message can be duplicated, retried, or dead-lettered.

Because of this, processors must load the latest order state from SQL Server before making decisions.

### Long-running workflow uses Durable Functions

The fulfillment process can take hours or days.

Durable Functions are used to wait for status changes and trigger timeout alerts without blocking normal application threads.

### Email sending is asynchronous

Emails are not sent inside the HTTP request flow.

This keeps API requests fast and avoids coupling user actions directly to email provider availability.

### AI is supporting functionality

AI features explain the system and orders, but they do not control business state.

The AI assistant must not invent order state.

Actual order state always comes from SQL Server.