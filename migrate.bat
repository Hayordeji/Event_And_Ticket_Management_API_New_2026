@echo off
echo Running migrations...

dotnet ef database update ^
  --project src/Modules/Identity/TicketingSystem.Modules.Identity.Infrastructure ^
  --startup-project src/Host/TicketingSystem.Api ^
  --context IdentityAppDbContext

dotnet ef database update ^
  --project src/Modules/Finance/TicketingSystem.Modules.Finance.Infrastructure ^
  --startup-project src/Host/TicketingSystem.Api ^
  --context FinanceDbContext

dotnet ef database update ^
  --project src/Modules/Catalog/TicketingSystem.Modules.Catalog.Infrastructure ^
  --startup-project src/Host/TicketingSystem.Api ^
  --context CatalogDbContext

dotnet ef database update ^
  --project src/Modules/Sales/TicketingSystem.Modules.Sales.Infrastructure ^
  --startup-project src/Host/TicketingSystem.Api ^
  --context SalesDbContext

dotnet ef database update ^
  --project src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Infrastructure ^
  --startup-project src/Host/TicketingSystem.Api ^
  --context FulfillmentDbContext

dotnet ef database update ^
  --project src/Modules/Access/TicketingSystem.Modules.Access.Infrastructure ^
  --startup-project src/Host/TicketingSystem.Api ^
  --context AccessDbContext

echo All migrations complete!