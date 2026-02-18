using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketingSystem.Modules.Access.Application.Services;
using TicketingSystem.Modules.Access.Domain.Repositories;
using TicketingSystem.Modules.Access.Infrastructure.Persistence;
using TicketingSystem.Modules.Access.Infrastructure.Persistence.Repositories;
using TicketingSystem.Modules.Access.Infrastructure.Services;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.Modules.Access.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAccessModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AccessDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("AccessDb"),
                    b => b.MigrationsAssembly("TicketingSystem.Modules.Access.Infrastructure")));

            // Repositories
            services.AddScoped<IScanLogRepository, ScanLogRepository>();

            // Services
            services.AddScoped<ITicketValidationService, TicketValidationService>();

            services.AddScoped<ITicketStatusService, TicketStatusService>();
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();


            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(Application.Commands.ScanTicketCommand).Assembly));

            return services;
        }
    }
}
