using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel.Authorization;

namespace TicketingSystem.Modules.Identity.Infrastructure.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (!await roleManager.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    Roles.Admin,
                    Roles.Host,
                    Roles.Customer,
                    Roles.Scanner
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
            
        }
    }
}
