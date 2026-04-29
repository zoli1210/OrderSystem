# OrderSystem

A simple modular monolith backend system built with .NET 8.

The application handles order creation, payment processing, and notifications using a clean and scalable structure without unnecessary complexity.

---

## Project Structure

```
OrderSystem.Api

├── Modules
│   ├── Orders
│   ├── Payments
│   └── Email
│
├── Infrastructure
│   ├── Persistence
│   ├── Messaging
│   └── ExternalServices
│
├── Domain
│   ├── Entities
│   ├── Enums
│   └── ValueObjects
│
├── Controllers
│
├── Shared
│   ├── Interfaces
│   ├── Exceptions
│   └── Helpers
│
├── Program.cs
└── appsettings.json
```

---

## Architecture

The system follows a modular monolith approach:

* One deployable application
* Business logic separated by feature (Modules)
* Infrastructure isolated from business logic
* No unnecessary layering or over-engineering

---

## Core Concepts

### Modules

Each feature is grouped into its own module:

* Orders: handles order creation and status
* Payments: handles payment-related logic
* Email: handles notifications

---

### Domain

Contains core business objects:

* Entities
* Enums
* Value Objects

No external dependencies.

---

### Infrastructure

Handles external concerns:

* Database (SQL Server)
* Messaging (queues)
* External services (payment, email)

---

### Controllers

* Handle HTTP requests
* Call into modules
* Do not contain business logic

---

### Shared

Common utilities:

* Interfaces
* Exceptions
* Helpers

---

## Basic Flow

1. Request arrives to controller
2. Controller calls module logic
3. Module handles business rules
4. Data is stored via infrastructure
5. Optional: event or message is published

---

## Running the Project

```
dotnet run --project OrderSystem.Api
```

Requirements:

* SQL Server is running
* Connection string is configured in appsettings.json

---

## Rules

* No business logic in Controllers
* Modules own the business logic
* Infrastructure should not leak into modules
* Avoid unnecessary abstraction

---

## Notes

This project is intentionally kept simple and extendable.

It can later evolve into:

* Event-driven architecture
* Background workers (Azure Functions)
* Microservices if needed

---

## Goal

Provide a clean, maintainable backend structure without overcomplicating the system.
