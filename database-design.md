# Database Design

## Schema Strategy

Each module has its own database schema for logical separation:

| Module | Schema Name | Database | Purpose |
|--------|-------------|----------|---------|
| Identity | `identity` | TicketingSystem_Identity | Users, auth, devices |
| Finance | `finance` | TicketingSystem_Finance | Ledgers, transactions |
| Catalog | `catalog` | TicketingSystem_Catalog | Events, ticket types |
| Sales | `sales` | TicketingSystem_Sales | Orders, payments |
| Fulfillment | `fulfillment` | TicketingSystem_Fulfillment | Tickets, QR codes |
| Access | `access` | TicketingSystem_Access | Scans, validations |

## Naming Conventions

- **Tables**: PascalCase (e.g., `Users`, `LedgerAccounts`)
- **Columns**: PascalCase (e.g., `FirstName`, `CreatedAt`)
- **Primary Keys**: `Id` (Guid)
- **Foreign Keys**: `{EntityName}Id` (e.g., `UserId`, `OrderId`)
- **Indexes**: `IX_{TableName}_{ColumnName}` (e.g., `IX_Users_Email`)

## Standard Audit Fields

All entities inherit these from `Entity` base class:

| Field | Type | Purpose |
|-------|------|---------|
| Id | uniqueidentifier | Primary key |
| CreatedAt | datetime2 | When created (UTC) |
| UpdatedAt | datetime2 | When last updated (UTC) |
| CreatedBy | uniqueidentifier | User who created |
| UpdatedBy | uniqueidentifier | User who updated |
| IsDeleted | bit | Soft delete flag |
| DeletedAt | datetime2 | When deleted (UTC) |
| DeletedBy | uniqueidentifier | User who deleted |

## Concurrency Control

- Uses `RowVersion` (timestamp) for optimistic concurrency
- Applied to critical entities: LedgerAccount, Order, TicketType

## Migration Strategy

- Each module has its own migrations
- Migrations are namespaced: `Identity_InitialCreate`, `Finance_AddLedgerTables`
- Apply migrations independently per module
- Never create cross-module foreign keys