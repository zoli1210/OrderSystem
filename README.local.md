# OrderSystem

OrderSystem is a .NET 8 learning project that demonstrates an event-driven order processing flow using ASP.NET Core Web API, SQL Server, Azure Service Bus, and Azure Functions.

The goal of the project is to keep the system small and understandable while still following a realistic backend architecture.

## Projects

```txt
OrderSystem.sln
│
├── OrderSystem
│   └── ASP.NET Core Web API
│
└── OrderSystem.AzureFunctions
    └── Azure Functions worker
```

## Responsibilities

### OrderSystem

The API project is responsible for:

- creating orders
- validating incoming requests
- saving orders to SQL Server
- publishing messages to Azure Service Bus
- exposing order endpoints
- exposing dead-letter inspection and retry endpoints

### OrderSystem.AzureFunctions

The Azure Functions project is responsible for:

- processing payment messages
- updating order status
- publishing email notification messages
- processing email notification messages
- simulating email sending

## Main Flow

```txt
POST /orders
→ Order saved to SQL Server with Pending status
→ OrderCreatedMessage sent to order-created queue
→ PaymentProcessorFunction processes the message
→ Order status changes to Paid or PaymentFailed
→ EmailNotificationMessage sent to email-notification queue
→ EmailNotificationFunction processes the email message
```

## Architecture

```txt
Client
  ↓
OrderSystem API
  ↓
SQL Server
  ↓
Azure Service Bus: order-created
  ↓
PaymentProcessorFunction
  ↓
Azure Service Bus: email-notification
  ↓
EmailNotificationFunction
```

## Main Components

### API Project

```txt
Controllers
Domain
Infrastructure
Modules
Shared
```

### Function Project

```txt
Functions
Services
DependencyInjection
```

## Domain

The main entity is `Order`.

An order can have the following statuses:

```txt
Pending
PaymentProcessing
Paid
PaymentFailed
Cancelled
```

## Messaging

The system uses two Azure Service Bus queues:

```txt
order-created
email-notification
```

`order-created` is used to start payment processing.

`email-notification` is used after successful payment processing.

## Dead-letter Handling

Azure Service Bus dead-letter functionality is used for failed messages.

The API can:

- list dead-letter messages
- retry a dead-letter message by sequence number

Available endpoints:

```txt
GET /dead-letters
POST /dead-letters/{sequenceNumber}/retry
```

## API Endpoints

```txt
POST /orders
GET /orders/{id}

GET /dead-letters
POST /dead-letters/{sequenceNumber}/retry
```

## Configuration

Secrets must not be committed to source control.

Use local development config files for secrets:

```txt
appsettings.Development.json
local.settings.json
```

Required configuration values:

```txt
SQLConnection
AzureServiceBus:ConnectionString
AzureServiceBus:OrderCreatedQueueName
AzureServiceBus:EmailNotificationQueueName
AzureServiceBusConnection
SqlConnectionString
```

## Local Development

Both projects must run during local testing:

```txt
OrderSystem
OrderSystem.AzureFunctions
```

The expected order status flow is:

```txt
Pending → PaymentProcessing → Paid
```

If payment processing fails, Azure Service Bus retries the message and eventually moves it to the dead-letter queue.

## Current Limitations

- payment processing is simulated
- email sending is simulated
- no authentication yet
- no real payment provider
- no real email provider
- LocalDB is used for development

## Future Improvements

Possible next steps:

```txt
Authentication
Real email provider
Real payment provider
Azure Key Vault
Application Insights
Idempotency
Outbox pattern
Order cancellation
Admin UI for dead-letter handling
```