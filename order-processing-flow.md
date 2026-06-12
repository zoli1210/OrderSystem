# Order Processing Flow

This document describes the complete order processing flow, including order lifecycle, payment processing, fulfillment workflow, email notifications, and dead-letter handling.

## Order Statuses

The system uses the following order statuses:

```text
Pending
PaymentProcessing
Paid
Preparing
ReadyForShipment
Shipped
Delivered
Failed
Cancelled
Returned
```

## Status Meaning

### Pending

The order has been created and saved, but payment has not completed yet.

This is the initial status of a new order.

### PaymentProcessing

The payment processor has started processing the order.

### Paid

Payment completed successfully.

This status starts the fulfillment workflow.

### Preparing

The paid order is being prepared.

### ReadyForShipment

The order is prepared and ready to be shipped.

### Shipped

The order has been shipped.

A tracking number is required for this status.

### Delivered

The order has been delivered to the customer.

### Failed

Payment processing failed.

The order can be retried from this status.

### Cancelled

The order has been cancelled.

A cancellation reason is required.

### Returned

The delivered order has been returned.

## Valid Status Transitions

```text
Pending
  → PaymentProcessing
  → Cancelled

PaymentProcessing
  → Paid
  → Failed

Failed
  → Pending
  → Cancelled

Paid
  → Preparing
  → Cancelled

Preparing
  → ReadyForShipment
  → Cancelled

ReadyForShipment
  → Shipped
  → Cancelled

Shipped
  → Delivered

Delivered
  → Returned

Cancelled
  → no further transition

Returned
  → no further transition
```

## Happy Path

```text
Pending
  ↓
PaymentProcessing
  ↓
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

## Payment Processing

Payment processing is asynchronous.

The API does not process the payment directly during the HTTP request.

Instead:

```text
POST /orders
  ↓
Order is saved with Pending status
  ↓
OrderCreatedMessage is published to order-created queue
  ↓
PaymentProcessorFunction receives the message
  ↓
PaymentProcessor loads the order from SQL Server
  ↓
Order status changes to PaymentProcessing
  ↓
PaymentService processes the payment
  ↓
Order status changes to Paid or Failed
```

## Payment Message

The payment message contains the minimum data needed to start payment processing.

Typical message data:

```text
OrderId
TotalAmount
CustomerEmail
```

The processor still loads the actual order from SQL Server.

The queue message is not treated as the full source of truth.

## Payment Success

If payment succeeds:

```text
PaymentProcessing
  ↓
Paid
```

Then the system:

- saves order status history
- publishes an order status changed message
- publishes a payment confirmation email message
- starts fulfillment workflow through the status changed flow

## Payment Failure

If payment fails:

```text
PaymentProcessing
  ↓
Failed
```

The order can be retried through:

```text
POST /orders/{id}/retry-payment
```

Retrying payment moves the order back to `Pending` and publishes a new payment message.

## Payment Idempotency

Payment processing should only continue if the order is currently in `Pending`.

```text
if order.Status != Pending
  → skip processing
```

This protects the system from duplicate queue messages and repeated retries.

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

## Durable Workflow Start

```text
Order reaches Paid
  ↓
OrderStatusChangedMessage is published
  ↓
OrderStatusChangedFunction receives the message
  ↓
OrderFulfillmentOrchestrator starts
```

The workflow uses a deterministic instance id based on the order id.

This prevents accidentally starting multiple fulfillment workflows for the same order.

## Durable External Events

The workflow waits for these fulfillment status events:

```text
Preparing
ReadyForShipment
Shipped
Delivered
```

Manual status changes are made through the API.

After the API saves the new status, it publishes an order status changed message.

The function receives the message and raises the matching Durable external event.

## Fulfillment Timeout Rules

The workflow monitors whether fulfillment steps happen in time.

```text
Paid → Preparing: 48 hours
Preparing → ReadyForShipment: 24 hours
ReadyForShipment → Shipped: 24 hours
Shipped → Delivered: 5 days
```

If the expected status does not arrive in time, the workflow sends an admin alert email.

The alert does not automatically cancel or fail the order.

It only notifies that manual action may be needed.

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

## Email Types

Current email use cases include:

```text
PaymentConfirmation
Fulfillment status notifications
Fulfillment timeout alerts
```

## Email Idempotency

The email processor checks whether the same email type was already sent for the same order.

```text
OrderId + EmailType
  → send only once
```

This protects the system from duplicate Service Bus messages and retry scenarios.

## Email History

Email notification history stores:

- order id
- recipient
- subject
- body
- email type
- status
- created timestamp
- sent timestamp
- failed timestamp
- error message

Supported email history statuses:

```text
Pending
Sent
Failed
```

## Dead-letter Handling

Azure Service Bus can move messages to dead-letter queues when they cannot be processed successfully.

The system exposes admin-only endpoints for inspecting and retrying dead-letter messages.

Supported dead-letter areas:

```text
orders
emails
```

These map to:

```text
order-created
email-notification
```

## Dead-letter Endpoints

```text
GET  /dead-letters/orders
GET  /dead-letters/emails

POST /dead-letters/orders/{sequenceNumber}/retry
POST /dead-letters/emails/{sequenceNumber}/retry
```

## Dead-letter Retry Flow

```text
Read dead-letter message by sequence number
  ↓
Republish the message body to the original queue
  ↓
Complete/remove the dead-letter message
  ↓
Message is processed again
```

## Retry Safety

Before retrying a dead-letter message, check:

- whether the original bug has been fixed
- whether the related order still exists
- whether the message body is valid
- whether retrying can create duplicate side effects
- whether the processor has idempotency protection

## Related Components

```text
OrdersController
OrderService
Order
OrderStatus
OrderStatusHistory
AzureServiceBusOrderMessageSender
AzureServiceBusOrderStatusMessageSender
AzureServiceBusEmailMessageSender

PaymentProcessorFunction
PaymentProcessor
PaymentService

OrderStatusChangedFunction
OrderFulfillmentOrchestrator
OrderFulfillmentAlertActivity

EmailNotificationFunction
EmailProcessor
AzureCommunicationEmailService
EmailNotificationHistory

DeadLetterController
AzureServiceBusDeadLetterService
```

## Related Endpoints

```text
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
```