using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Identity.Application.Services;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Domain.Repositories;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations;
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
            services.AddDbContext<IdentityAppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentityDb"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(IdentityAppDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    }));

            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                // Password rules
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Lockout
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // User
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityAppDbContext>()
            .AddDefaultTokenProviders();               // ← For password reset, email confirm later

            // Services
            services.AddOptions<JwtConfig>()
                .BindConfiguration(JwtConfig.SectionName)
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<JwtConfig>, JwtConfigValidator>(); services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

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
