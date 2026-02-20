using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.Services;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence.Repositories;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.Modules.Catalog.Api
{
    /// <summary>
    /// Catalog API module dependency injection configuration
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add Catalog API module to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    Assembly.Load("TicketingSystem.Modules.Catalog.Application"));
            });

            services.AddDbContext<CatalogDbContext>(options =>
               options.UseSqlServer(
                   configuration.GetConnectionString("CatalogDb"),
                   sqlOptions =>
                   {
                       sqlOptions.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName);
                       sqlOptions.EnableRetryOnFailure(
                           maxRetryCount: 3,
                           maxRetryDelay: TimeSpan.FromSeconds(5),
                           errorNumbersToAdd: null);
                   }));

            // Register Repositories
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
            services.AddScoped<IOrderDataService, OrderDataService>();

            services.AddScoped<IEventRepository, EventRepository>();
            return services;
        }
    }
}