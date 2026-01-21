using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Identity.Application.Services;
using TicketingSystem.Modules.Identity.Domain.Repositories;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence.Repositories;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Api
{
    ///<summary>
/// Identity module dependency injection configuration
/// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Database
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentityDb"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    }));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();

            // Services
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // MediatR (Commands & Queries)
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    Assembly.Load("TicketingSystem.Modules.Identity.Application"));
            });

            return services;
        }
    }
}
