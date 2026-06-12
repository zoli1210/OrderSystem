# OrderSystem

OrderSystem is a .NET 8 backend learning project that demonstrates a realistic event-driven order processing system.

The project uses ASP.NET Core Web API, SQL Server, EF Core, Azure Service Bus, Azure Functions, Durable Functions, Azure Communication Services, OpenAI, and Supabase pgvector.

The goal is to show how a backend system can be structured around clean API boundaries, asynchronous processing, status-driven workflows, background workers, role-based access control, and AI-assisted documentation/order explanation.

## Main Technologies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- Azure Service Bus
- Azure Functions
- Durable Functions
- Azure Communication Services
- Application Insights
- OpenAI
- Supabase
- pgvector

## Solution Structure

```text
OrderSystem.sln
│
├── OrderSystem
│   └── ASP.NET Core Web API
│
└── OrderSystem.AzureFunctions
    └── Azure Functions worker project
```

## Main Runtime Flow

```text
POST /orders
  ↓
Order is saved to SQL Server with Pending status
  ↓
OrderCreatedMessage is published to Azure Service Bus
  ↓
PaymentProcessorFunction processes the payment message
  ↓
Order status changes to PaymentProcessing
  ↓
Order status changes to Paid or Failed
  ↓
OrderStatusChangedMessage is published
  ↓
Durable fulfillment workflow starts when the order reaches Paid
  ↓
EmailNotificationMessage is published when customer/admin communication is needed
  ↓
EmailNotificationFunction sends the email
```

## Main Features

- User registration and login
- JWT authentication
- Role-based authorization
- Admin/user access separation
- Order creation
- Order querying
- Order cancellation
- Payment retry
- Order status history
- Email notification history
- Asynchronous payment processing
- Fulfillment workflow orchestration
- Dead-letter message inspection and retry
- Health checks
- AI knowledge assistant
- AI order process explanation

## Main API Endpoints

```text
POST   /auth/register
POST   /auth/login
GET    /auth/users
PUT    /auth/users/{userId}/role

POST   /orders
GET    /orders
GET    /orders/{id}
GET    /orders/user-history
GET    /orders/{id}/status-history
GET    /orders/{id}/email-history
PATCH  /orders/{orderId}/status
POST   /orders/{id}/cancel
POST   /orders/{id}/retry-payment

GET    /dead-letters/orders
GET    /dead-letters/emails
POST   /dead-letters/orders/{sequenceNumber}/retry
POST   /dead-letters/emails/{sequenceNumber}/retry

POST   /ai/knowledge/ask
POST   /ai/knowledge/documents
POST   /ai/orders/{orderId}/explain

GET    /health
```

## Documentation

Detailed documentation is available under the `docs` folder.

- [System overview](docs/architecture/system-overview.md)
- [Order processing flow](docs/processes/order-processing-flow.md)
- [AI features](docs/ai/ai-features.md)
- [Authentication and authorization](docs/security/authentication-authorization.md)
- [Local development and configuration](docs/operations/local-development-and-configuration.md)

## Local Development

Both projects must run during local testing:

```text
OrderSystem
OrderSystem.AzureFunctions
```

The API handles synchronous HTTP requests.

The Azure Functions project handles asynchronous background processing through Azure Service Bus queues.

Required queues:

```text
order-created
order-status-changed
email-notification
```

Required infrastructure:

- SQL Server
- Azure Service Bus
- Azure Storage for Durable Functions
- Azure Communication Services
- OpenAI
- Supabase with pgvector

For detailed setup, see:

- [Local development and configuration](docs/operations/local-development-and-configuration.md)

## Security Notes

Secrets must not be committed to source control.

Do not commit real values for:

- SQL connection strings
- JWT signing keys
- Azure Service Bus connection strings
- Azure Storage connection strings
- Azure Communication Services connection strings
- OpenAI API keys
- Supabase secret keys
- Application Insights connection strings

Use local development configuration files, environment variables, or platform-level application settings for sensitive values.

## Project Status

This project is intended as a portfolio and learning backend system.

It intentionally demonstrates several production-style backend concepts in a compact solution while keeping the project understandable and extendable.
