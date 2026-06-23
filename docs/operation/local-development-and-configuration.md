# Local Development and Configuration

This document describes how to run OrderSystem locally and which configuration values are required.

## Required Projects

Both runtime projects must run during local testing:

```text
OrderSystem
OrderSystem.AzureFunctions
```

`OrderSystem.Shared` is not started directly.

Visual Studio multiple startup projects should be:

```text
OrderSystem                -> Start
OrderSystem.AzureFunctions -> Start
OrderSystem.Shared         -> None
```

The API project handles HTTP requests.

The Azure Functions project handles asynchronous background processing.

The shared project contains code used by both runtime projects.

## Required Infrastructure

The project requires:

- SQL Server or Azure SQL Database
- Azure Service Bus
- Azure Storage for Durable Functions
- Azure Communication Services
- OpenAI
- Supabase with pgvector
- Application Insights configuration if telemetry is enabled

## Required Service Bus Queues

Create these queues:

```text
order-created
order-status-changed
email-notification
```

## API Configuration Files

The API project uses:

```text
appsettings.json
appsettings.Development.json
environment variables
```

`appsettings.json` should contain safe structure/defaults.

`appsettings.Development.json` can contain local secret values, but it must not be committed.

## Azure Functions Configuration Files

The Azure Functions project uses:

```text
local.settings.json
environment variables
Azure Function App configuration
```

`local.settings.json` can contain local secret values, but it must not be committed.

## Database Configuration

The API and Azure Functions must point to the same database.

For local-only development this can be LocalDB or SQL Server.

For Azure integration testing this should be Azure SQL Database.

API configuration:

```text
ConnectionStrings:SQLConnection
```

Azure Functions configuration:

```text
SqlConnectionString
ConnectionStrings:SQLConnection
```

Both values should point to the same database.

Example Azure SQL connection string shape:

```text
Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=OrderSystemDb;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

When testing Azure SQL from a local machine, the current outbound IP address must be allowed on the Azure SQL logical server firewall.

If the local network uses VPN/proxy routing, the outbound IP can change. In that case, update the Azure SQL firewall rule or allow the required IP range for development testing.

## JWT Configuration

Required JWT values:

```text
Jwt:Issuer
Jwt:Audience
Jwt:Key
```

`Jwt:Key` is sensitive and must not be committed.

## Admin User Seed Configuration

Required admin seed values:

```text
AdminUser:Email
AdminUser:Password
```

The admin password is sensitive and must not be committed.

## Azure Service Bus Configuration

The API and Azure Functions both use Azure Service Bus.

API configuration:

```text
AzureServiceBus:ConnectionString
AzureServiceBus:OrderCreatedQueueName
AzureServiceBus:OrderStatusChangedQueueName
AzureServiceBus:EmailNotificationQueueName
```

Azure Functions configuration:

```text
AzureServiceBusConnection
AzureServiceBus:ConnectionString
AzureServiceBus:OrderCreatedQueueName
AzureServiceBus:OrderStatusChangedQueueName
AzureServiceBus:EmailNotificationQueueName
```

`AzureServiceBusConnection` is required by the Service Bus trigger attributes.

`AzureServiceBus:ConnectionString` is required by shared sender services.

Required queue names:

```text
order-created
order-status-changed
email-notification
```

## Azure Functions Local Configuration Example

`OrderSystem.AzureFunctions/local.settings.json` should contain local values similar to:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<storage-connection-string>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    "SqlConnectionString": "<sql-connection-string>",
    "ConnectionStrings:SQLConnection": "<sql-connection-string>",

    "AzureServiceBusConnection": "<service-bus-connection-string>",
    "AzureServiceBus:ConnectionString": "<service-bus-connection-string>",

    "AzureServiceBus:OrderCreatedQueueName": "order-created",
    "AzureServiceBus:OrderStatusChangedQueueName": "order-status-changed",
    "AzureServiceBus:EmailNotificationQueueName": "email-notification",

    "CommunicationServices:ConnectionString": "<communication-services-connection-string>",
    "CommunicationServices:SenderAddress": "<sender-email-address>",

    "FulfillmentAlertEmail": "<admin-alert-email>"
  }
}
```

## Durable Functions Storage

Durable Functions require Azure Storage.

Required value:

```text
AzureWebJobsStorage
```

This is used for orchestration state management.

For local development, this can point to:

- a real Azure Storage account
- a local Azure Storage emulator

## Fulfillment Alert Configuration

Fulfillment timeout alert emails use:

```text
FulfillmentAlertEmail
```

This email address receives admin alerts when fulfillment status changes are delayed.

## Azure Communication Services Configuration

Email sending uses Azure Communication Services.

Required values:

```text
CommunicationServices:ConnectionString
CommunicationServices:SenderAddress
```

The connection string is sensitive and must not be committed.

## Application Insights Configuration

Application Insights is used for telemetry.

Possible values:

```text
ApplicationInsights:ConnectionString
APPLICATIONINSIGHTS_CONNECTION_STRING
```

The exact key depends on project/runtime configuration.

## OpenAI Configuration

Required values:

```text
OpenAI:ApiKey
OpenAI:EmbeddingModel
OpenAI:ChatModel
OpenAI:DefaultMatchCount
```

`OpenAI:ApiKey` is sensitive and must not be committed.

## Supabase Configuration

Required values:

```text
Supabase:Url
Supabase:SecretKey
```

`Supabase:SecretKey` is backend-only and must not be exposed to client applications.

## Local Run Order

Recommended local run order:

```text
1. Start SQL Server or confirm Azure SQL Database access.
2. Confirm Azure Service Bus queues exist.
3. Confirm Azure Storage is available for Durable Functions.
4. Start OrderSystem API.
5. Start OrderSystem.AzureFunctions.
6. Test GET /health.
7. Create or log in with a user.
8. Create an order.
9. Watch the queue/function processing.
10. Verify order data and email history in SQL.
```

## Expected Full Happy Path Test

```text
1. Register or log in.
2. Create an order through POST /orders.
3. Confirm the order is saved with Pending status.
4. Confirm OrderCreatedMessage is published.
5. Let PaymentProcessorFunction process the message.
6. Confirm the order moves to PaymentProcessing.
7. Confirm the order moves to Paid.
8. Confirm OrderStatusChangedMessage is published.
9. Confirm fulfillment workflow starts.
10. Update order status through PATCH /orders/{orderId}/status.
11. Move order through Preparing, ReadyForShipment, Shipped, Delivered.
12. Confirm email messages are processed.
13. Confirm status history and email history endpoints return data.
14. Confirm the records are visible in the configured SQL database.
```

## Expected Payment Failure Test

```text
Pending
  ↓
PaymentProcessing
  ↓
Failed
```

Then retry:

```text
POST /orders/{id}/retry-payment
```

Expected retry flow:

```text
Failed
  ↓
Pending
  ↓
PaymentProcessing
  ↓
Paid or Failed
```

## Health Check

The API exposes:

```text
GET /health
```

Use this endpoint to verify important runtime dependencies.

The health check validates the currently configured SQL database and Service Bus access.

If the API connection string points to LocalDB, the health check validates LocalDB.

If the API connection string points to Azure SQL Database, the health check validates Azure SQL Database.

## Common Local Problems

### API can connect to Azure SQL but Functions cannot find the order

This usually means the API and Functions are using different SQL connection strings.

Check:

```text
ConnectionStrings:SQLConnection
SqlConnectionString
ConnectionStrings:SQLConnection in local.settings.json
```

Both runtime projects must point to the same database.

### Azure SQL firewall blocks local execution

Error shape:

```text
Client with IP address '<ip>' is not allowed to access the server.
```

Fix:

```text
Azure Portal
-> SQL servers
-> <server>
-> Networking
-> Firewall rules
-> Add current client IP address
-> Save
```

If the IP changes often, the machine is probably behind VPN/proxy routing.

### Azure Functions does not process messages

Check:

```text
AzureServiceBusConnection
queue names
queue existence
Function App startup logs
message dead-letter count
```

### Function receives Service Bus message but fails sending another message

Check that Functions configuration contains:

```text
AzureServiceBus:ConnectionString
AzureServiceBus:OrderCreatedQueueName
AzureServiceBus:OrderStatusChangedQueueName
AzureServiceBus:EmailNotificationQueueName
```

The trigger connection and sender connection are separate config needs.

### Durable workflow does not start

Check:

```text
AzureWebJobsStorage
order-status-changed queue
OrderStatusChangedFunction logs
whether the order actually reached Paid
whether a workflow instance already exists
```

### Emails are not sent

Check:

```text
email-notification queue
CommunicationServices:ConnectionString
CommunicationServices:SenderAddress
EmailNotificationFunction logs
email notification history
```

### Status emails are skipped

Check the email history table.

Status email messages must set their own `EmailType`.

Expected email types:

```text
PaymentConfirmation
Preparing
ReadyForShipment
Shipped
Delivered
```

If `EmailType` is missing, the email processor treats the message as `PaymentConfirmation`.

### AI assistant does not return useful answers

Check:

```text
OpenAI configuration
Supabase configuration
knowledge_sources table
knowledge_documents table
background ingestion logs
pgvector migration
```

## Secret Handling Rules

Do not commit real values for:

```text
SQLConnection
SqlConnectionString
ConnectionStrings:SQLConnection
Jwt:Key
AdminUser:Password
AzureServiceBus:ConnectionString
AzureServiceBusConnection
AzureWebJobsStorage
CommunicationServices:ConnectionString
ApplicationInsights:ConnectionString
APPLICATIONINSIGHTS_CONNECTION_STRING
OpenAI:ApiKey
Supabase:SecretKey
```

## Files That Should Not Be Shared Publicly

Do not include these files or folders in public uploads:

```text
.git
.vs
bin
obj
*.user
local.settings.json
appsettings.Development.json
serviceDependencies.local.json.user
```

## Recommended Local Secret Strategy

Use this structure:

```text
appsettings.json
  -> safe defaults and non-secret configuration shape

appsettings.Development.json
  -> local API secrets, ignored by git

local.settings.json
  -> local Azure Functions secrets, ignored by git

environment variables
  -> preferred for deployed environments
```

## Before Sharing the Project

Before sending the project ZIP to anyone, remove:

```text
.git
.vs
bin
obj
*.user
local.settings.json
appsettings.Development.json
serviceDependencies.local.json.user
```

Also check that no secret values are accidentally present in:

```text
README.md
docs
launchSettings.json
service dependency files
deployment files
```