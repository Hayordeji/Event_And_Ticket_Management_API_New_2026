using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Sales.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSalesModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Database
            services.AddDbContext<SalesDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("SalesDb"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(SalesDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    }));

            // Repositories
            

            // MediatR (Commands & Queries)
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    Assembly.Load("TicketingSystem.Modules.Sales.Application"));
            });

            return services;
        }
    }
}
