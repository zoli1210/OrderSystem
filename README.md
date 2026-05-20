# OrderSystem

OrderSystem is a .NET 8 learning project that demonstrates an event-driven order processing flow using ASP.NET Core Web API, SQL Server, Azure Service Bus, Azure Functions, and Azure Communication Services.

The goal of the project is to keep the system small and understandable while still following realistic backend architecture patterns.

## Projects

    OrderSystem.sln
    │
    ├── OrderSystem
    │   └── ASP.NET Core Web API
    │
    └── OrderSystem.AzureFunctions
        └── Azure Functions worker

## Responsibilities

### OrderSystem

The API project is responsible for:

- creating orders
- validating incoming requests
- saving orders to SQL Server
- exposing order endpoints
- handling authentication and authorization
- managing user roles
- publishing messages to Azure Service Bus
- exposing dead-letter inspection and retry endpoints
- exposing health check endpoints

### OrderSystem.AzureFunctions

The Azure Functions project is responsible for:

- processing payment messages
- updating order status
- publishing email notification messages
- processing email notification messages
- sending emails through Azure Communication Services

## Main Flow

    POST /orders
    → Order saved to SQL Server with Pending status
    → OrderCreatedMessage sent to order-created queue
    → PaymentProcessorFunction processes the message
    → Order status changes to PaymentProcessing
    → Order status changes to Paid or PaymentFailed
    → EmailNotificationMessage sent to email-notification queue
    → EmailNotificationFunction sends the email

## Architecture

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
      ↓
    Azure Communication Services

## Main Components

### API Project

    Controllers
    Domain
    Infrastructure
    Modules
    Shared

### Function Project

    Functions
    Services
    DependencyInjection

## Domain

The main entity is `Order`.

An order can have the following statuses:

    Pending
    PaymentProcessing
    Paid
    PaymentFailed
    Cancelled

Order status changes are tracked separately through status history.

Example:

    Pending → PaymentProcessing → Paid

or:

    Pending → PaymentProcessing → PaymentFailed → Pending → PaymentProcessing → Paid

## Authentication and Authorization

The API uses JWT Bearer authentication with ASP.NET Core Identity.

Supported roles:

    Admin
    Manager
    TeamLead
    Support
    User

Role hierarchy:

    Admin    → can assign Manager, TeamLead, Support, User
    Manager  → can assign TeamLead, Support, User
    TeamLead → can assign Support, User
    Support  → cannot assign roles
    User     → cannot assign roles

Orders are connected to the authenticated user.

    User  → can access only their own orders
    Admin → can access all orders

## Messaging

The system uses two Azure Service Bus queues:

    order-created
    email-notification

`order-created` is used to start payment processing.

`email-notification` is used after successful payment processing.

## Dead-letter Handling

Azure Service Bus dead-letter functionality is used for failed messages.

The API can:

- list order dead-letter messages
- list email dead-letter messages
- retry dead-letter messages by sequence number

Available endpoints:

    GET  /dead-letters/orders
    GET  /dead-letters/emails

    POST /dead-letters/orders/{sequenceNumber}/retry
    POST /dead-letters/emails/{sequenceNumber}/retry

## API Endpoints

    POST   /auth/register
    POST   /auth/login
    GET    /auth/users
    PUT    /auth/users/{userId}/role

    POST   /orders
    GET    /orders
    GET    /orders/{id}
    POST   /orders/{id}/cancel
    POST   /orders/{id}/retry-payment
    GET    /orders/{id}/status-history

    GET    /dead-letters/orders
    GET    /dead-letters/emails
    POST   /dead-letters/orders/{sequenceNumber}/retry
    POST   /dead-letters/emails/{sequenceNumber}/retry

    GET    /health

## Health Checks

The API exposes a detailed health check endpoint:

    GET /health

It checks:

    SQL database
    Azure Service Bus configuration
    Application Insights configuration

## Observability

Application Insights is used for telemetry and logging.

It tracks:

    API requests
    Azure Function executions
    traces
    exceptions
    dependency calls

## Configuration

Secrets must not be committed to source control.

Use local development config files for secrets:

    appsettings.Development.json
    local.settings.json

Required configuration values include:

    SQLConnection
    Jwt:Issuer
    Jwt:Audience
    Jwt:Key
    AdminUser:Email
    AdminUser:Password
    AzureServiceBus:ConnectionString
    AzureServiceBus:OrderCreatedQueueName
    AzureServiceBus:EmailNotificationQueueName
    AzureServiceBusConnection
    SqlConnectionString
    CommunicationServices:ConnectionString
    CommunicationServices:SenderAddress
    ApplicationInsights:ConnectionString
    APPLICATIONINSIGHTS_CONNECTION_STRING

## Local Development

Both projects must run during local testing:

    OrderSystem
    OrderSystem.AzureFunctions

The expected successful order status flow is:

    Pending → PaymentProcessing → Paid

If payment processing fails, Azure Service Bus retries the message and eventually moves it to the dead-letter queue.
