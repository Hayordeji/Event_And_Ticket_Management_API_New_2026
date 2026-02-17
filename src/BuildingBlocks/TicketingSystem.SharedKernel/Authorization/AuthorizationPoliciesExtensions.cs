using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Authorization
{
    public static class AuthorizationPoliciesExtensions
    {
        public static IServiceCollection AddTicketingAuthorizationPolicies(
        this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(PolicyNames.RequireAdmin, policy =>
                    policy.RequireRole(Roles.Admin))

                .AddPolicy(PolicyNames.RequireHost, policy =>
                    policy.RequireRole(Roles.Host, Roles.Admin))

                .AddPolicy(PolicyNames.RequireCustomer, policy =>
                    policy.RequireRole(Roles.Customer, Roles.Admin))

                .AddPolicy(PolicyNames.RequireScanner, policy =>
                    policy.RequireRole(Roles.Scanner, Roles.Admin))

                .AddPolicy(PolicyNames.RequireHostOrScanner, policy =>
                    policy.RequireRole(Roles.Host, Roles.Scanner, Roles.Admin));

            return services;
        }
    }
}
