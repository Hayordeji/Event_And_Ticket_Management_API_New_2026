# Ticketing System - Backend API

Enterprise-grade event ticketing and management system built with .NET 10.

## Architecture

**Modular Monolith** with 6 bounded contexts:
- **Identity**: User management, authentication, device fingerprinting
- **Finance**: Double-entry ledger, payouts, commission calculation
- **Catalog**: Events, ticket types, versioning
- **Sales**: Cart, orders, payment gateway integration
- **Fulfillment**: Ticket generation, QR code signing
- **Access**: Gate validation, scanning, anti-fraud

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or Docker)
- Postman for API testing

## Getting Started
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API
dotnet run --project src/Host/TicketingSystem.Api
```

## Project Structure

Each module follows Clean Architecture:
- **Domain**: Entities, aggregates, domain events (pure business logic)
- **Application**: Use cases, commands, queries (CQRS)
- **Infrastructure**: EF Core, external services, repositories
- **Api**: Endpoints, DTOs, controllers

## Development Status

**In Active Development** 

---

