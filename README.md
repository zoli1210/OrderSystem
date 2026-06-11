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
- AI Knowledge Assistant with RAG
- Supabase pgvector based vector search
- Automatic documentation source seeding
- Automatic documentation ingestion and embedding generation
- AI Order Process Explainer using SQL order data, status history, email history, and vector-based documentation retrieval
- Supabase Row Level Security for AI knowledge tables

### OrderSystem.AzureFunctions

The Azure Functions project is responsible for:

- processing payment messages
- updating order status
- publishing email notification messages
- processing email notification messages
- sending emails through Azure Communication Services
- starting and monitoring the Durable fulfillment workflow
- handling fulfillment timeout alerts

## Main Flow

    POST /orders
    → Order saved to SQL Server with Pending status
    → OrderCreatedMessage sent to order-created queue
    → PaymentProcessorFunction processes the message
    → Order status changes to PaymentProcessing
    → Order status changes to Paid or PaymentFailed
    → OrderStatusChangedMessage sent to order-status-changed queue
    → Durable fulfillment workflow starts when the order reaches Paid status
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
    Azure Service Bus: order-status-changed
      ↓
    OrderStatusChangedFunction
      ↓
    Durable Fulfillment Workflow
      ↓
    Azure Service Bus: email-notification
      ↓
    EmailNotificationFunction
      ↓
    Azure Communication Services

## Durable Fulfillment Workflow

The project includes a Durable Functions based fulfillment workflow that monitors the order lifecycle after a successful payment.

The workflow starts automatically when an order reaches the `Paid` status. From that point, the orchestration waits for human/admin-driven status changes and reacts to them through Durable external events.

    Payment successful
      ↓
    Order status changes to Paid
      ↓
    OrderStatusChangedMessage sent to order-status-changed queue
      ↓
    OrderStatusChangedFunction starts OrderFulfillmentOrchestrator
      ↓
    Workflow waits for admin-driven fulfillment status changes

The monitored fulfillment flow is:

    Paid
      ↓
    Preparing
      ↓
    ReadyForShipment
      ↓
    Shipped
      ↓
    Delivered

### Human Interaction Pattern

The fulfillment workflow uses the Durable Functions human interaction pattern.

Admin or seller actions still happen through the existing order status endpoint:

    PATCH /orders/{orderId}/status

When a status change happens, the API publishes an `OrderStatusChangedMessage`. The `OrderStatusChangedFunction` then raises the matching Durable external event for the running workflow instance.

Example:

    Admin changes order status to Preparing
      ↓
    OrderStatusChangedFunction receives the status changed message
      ↓
    Durable external event "Preparing" is raised
      ↓
    OrderFulfillmentOrchestrator continues to the next step

The API and domain layer remain the source of truth for order status changes. Durable Functions only coordinate the long-running fulfillment workflow.

### Timeout Alerts

The workflow contains timeout handling for fulfillment steps.

If an expected status change does not happen within the configured time window, an alert activity is triggered and an admin alert email is queued through the existing email notification pipeline.

Configured timeout rules:

    Paid → Preparing: 48 hours
    Preparing → ReadyForShipment: 24 hours
    ReadyForShipment → Shipped: 24 hours
    Shipped → Delivered: 5 days

Alert flow:

    Expected status not received in time
      ↓
    OrderFulfillmentAlertActivity runs
      ↓
    Admin alert email is sent through the email-notification queue
      ↓
    Workflow continues waiting for the expected human action

### Durable Components

    OrderFulfillmentOrchestrator
    → Durable orchestration that waits for fulfillment status events

    OrderFulfillmentAlertActivity
    → Sends admin alert emails when a fulfillment step times out

    OrderStatusChangedFunction
    → Starts the workflow on Paid status
    → Raises Durable external events for fulfillment status changes
    → Keeps the existing email notification flow running

    StartOrderFulfillmentWorkflowFunction
    → Optional HTTP-triggered starter function for manual/admin testing

    OrderFulfillmentWorkflowInput
    → Input model for the orchestration

    OrderFulfillmentStatusEvent
    → External event payload used by Durable Functions

    OrderFulfillmentAlert
    → Alert payload used by the activity

### Storage Requirement

Durable Functions require Azure Storage for orchestration state management.

The Azure Functions project must have a valid `AzureWebJobsStorage` value configured in `local.settings.json` or in the deployed Function App configuration.

The `FulfillmentAlertEmail` setting is used as the recipient for timeout alert emails.

## AI / RAG Flow

Documentation ingestion flow:

    knowledge-sources.json
      ↓
    Automatic source seeding
      ↓
    Supabase: knowledge_sources
      ↓
    Background documentation ingestion
      ↓
    HTML extraction
      ↓
    Text chunking
      ↓
    OpenAI embeddings
      ↓
    Supabase: knowledge_documents with pgvector

Knowledge question flow:

    User question
      ↓
    OpenAI embedding
      ↓
    Supabase vector search
      ↓
    Relevant documentation chunks
      ↓
    OpenAI response generation
      ↓
    AI answer with sources

Order explanation flow:

    OrderId + user question
      ↓
    SQL order lookup
      ↓
    Access control check
      ↓
    Status history lookup
      ↓
    Email notification history lookup
      ↓
    Question + order status embedding
      ↓
    Supabase vector search
      ↓
    Structured SQL context + documentation context
      ↓
    OpenAI response generation
      ↓
    AI order explanation

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
    Preparing
    ReadyForShipment
    Shipped
    Delivered
    Cancelled
    Returned

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

The system uses three Azure Service Bus queues:

    order-created
    order-status-changed
    email-notification

`order-created` is used to start payment processing.

`order-status-changed` is used to publish order status changes, start the Durable fulfillment workflow on `Paid`, and raise external events for fulfillment status changes.

`email-notification` is used for payment, fulfillment, and timeout alert emails.

## AI Knowledge Assistant

The project includes an AI Knowledge Assistant based on Retrieval-Augmented Generation.

It uses OpenAI embeddings and Supabase pgvector to answer questions based on indexed technical documentation.

The source list is stored in the project as a versioned JSON file:

    Infrastructure/Supabase/KnowledgeSources/knowledge-sources.json

During application startup, the API automatically seeds the configured knowledge sources into Supabase.

The background ingestion process then:

    reads active knowledge sources
    downloads documentation pages
    extracts readable text from HTML
    splits the content into chunks
    creates embeddings with OpenAI
    stores the chunks and embeddings in Supabase

The RAG flow uses two Supabase tables:

    knowledge_sources
    → stores the configured documentation sources

    knowledge_documents
    → stores the chunked and embedded searchable content

Row Level Security is enabled for both AI knowledge tables:

    knowledge_sources
    knowledge_documents

The application accesses these tables through the backend using a Supabase secret/service key. Client applications do not access the Supabase vector tables directly.

The main endpoint is:

    POST /ai/knowledge/ask

Example request:

    {
      "question": "What are Azure Service Bus dead-letter queues used for?"
    }

The response contains an AI-generated answer and the source documents used during retrieval.

## AI Order Process Explainer

The project also includes an AI Order Process Explainer.

This feature explains the current lifecycle state of a specific order by combining structured SQL data with vector-based documentation retrieval.

It uses SQL data as the source of truth for what actually happened:

    order details
    current order status
    order status history
    email notification history

It uses vector-retrieved documentation only as supporting context to explain:

    statuses
    transitions
    asynchronous processing
    payment behavior
    email behavior
    messaging behavior

The feature does not assume that an order has failed. It can explain completed, waiting, processing, failed, or cancelled orders.

The answer is adapted to the user's question:

    narrow question  → short, direct answer
    status question  → current status only
    email question   → email notification information
    process question → lifecycle explanation
    failure question → relevant failure or waiting reason

The main endpoint is:

    POST /ai/orders/{orderId}/explain

Example request:

    {
      "question": "What is the current status of this order?"
    }

Example request for a broader explanation:

    {
      "question": "Explain the full lifecycle of this order and whether any manual action is needed."
    }

The response contains an AI-generated explanation and the source documents used during retrieval.

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
    PATCH  /orders/{id}/status
    POST   /orders/{id}/cancel
    POST   /orders/{id}/retry-payment
    GET    /orders/{id}/status-history

    GET    /dead-letters/orders
    GET    /dead-letters/emails
    POST   /dead-letters/orders/{sequenceNumber}/retry
    POST   /dead-letters/emails/{sequenceNumber}/retry

    POST   /ai/knowledge/ask
    POST   /ai/knowledge/documents
    POST   /ai/orders/{orderId}/explain

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
    AzureServiceBus:OrderStatusChangedQueueName
    AzureServiceBusConnection
    AzureWebJobsStorage
    FulfillmentAlertEmail
    SqlConnectionString
    CommunicationServices:ConnectionString
    CommunicationServices:SenderAddress
    ApplicationInsights:ConnectionString
    APPLICATIONINSIGHTS_CONNECTION_STRING

AI configuration:

    OpenAI:ApiKey
    OpenAI:EmbeddingModel
    OpenAI:ChatModel
    OpenAI:DefaultMatchCount

Supabase configuration:

    Supabase:Url
    Supabase:SecretKey

The Supabase secret key is backend-only and must not be exposed to client applications.
Supabase Row Level Security is enabled for the AI knowledge tables.

## Local Development

Both projects must run during local testing:

    OrderSystem
    OrderSystem.AzureFunctions

The expected successful order status flow is:

    Pending → PaymentProcessing → Paid → Preparing → ReadyForShipment → Shipped → Delivered

If payment processing fails, Azure Service Bus retries the message and eventually moves it to the dead-letter queue.

Durable Functions require Azure Storage during local development. Use either a real Azure Storage connection string in `AzureWebJobsStorage` or a local storage emulator.

For local Durable fulfillment testing:

    1. Start the API project
    2. Start the Azure Functions project
    3. Create an order
    4. Let payment processing move the order to Paid
    5. Confirm that the Durable fulfillment workflow starts
    6. Update the order status through the API
    7. Confirm that the Durable external event is raised
