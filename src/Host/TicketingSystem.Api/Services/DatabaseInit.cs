using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using TicketingSystem.Modules.Access.Infrastructure.Persistence;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;

namespace TicketingSystem.Api.Services
{
    // src/Host/TicketingSystem.Api/DatabaseMigrator.cs
    public static class DatabaseMigrator
    {
        public static async Task ApplyMigrationsAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Applying migrations...");

                await Task.WhenAll(
                    MigrateAsync<IdentityAppDbContext>(scope, logger),
                    MigrateAsync<FinanceDbContext>(scope, logger),
                    MigrateAsync<CatalogDbContext>(scope, logger),
                    MigrateAsync<SalesDbContext>(scope, logger),
                    MigrateAsync<FulfillmentDbContext>(scope, logger),
                    MigrateAsync<AccessDbContext>(scope, logger)
                );

                logger.LogInformation("All migrations applied successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying migrations.");
                throw;
            }
        }

        private static async Task MigrateAsync<TContext>(
            IServiceScope scope,
            ILogger logger) where TContext : DbContext
        {
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var contextName = typeof(TContext).Name;

            var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

            if (!pending.Any())
            {
                logger.LogInformation("{Context}: No pending migrations — skipping.", contextName);
                return;
            }

            logger.LogInformation("{Context}: Applying {Count} pending migration(s): {Migrations}",
                contextName, pending.Count, string.Join(", ", pending));

            await context.Database.MigrateAsync();

            logger.LogInformation("{Context}: Migrations applied successfully.", contextName);
        }
    }
}
