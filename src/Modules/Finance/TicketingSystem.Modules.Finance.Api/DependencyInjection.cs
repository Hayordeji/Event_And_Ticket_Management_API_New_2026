using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Repositories;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence.Repositories;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFinanceModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Database
            services.AddDbContext<FinanceDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("FinanceDb"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(FinanceDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    }));

            // Repositories
            services.AddScoped<ILedgerAccountRepository, LedgerAccountRepository>();
            services.AddScoped<ILedgerTransactionRepository, LedgerTransactionRepository>();
            services.AddScoped<IUnitOfWork, FinanceUnitOfWork>();

            // MediatR (Commands & Queries)
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    Assembly.Load("TicketingSystem.Modules.Finance.Application"));
            });

            return services;
        }
    }
}
