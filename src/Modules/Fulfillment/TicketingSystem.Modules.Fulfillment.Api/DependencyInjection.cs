using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.Services;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence.Repositories;
using TicketingSystem.SharedKernel.Outbox;

namespace TicketingSystem.Modules.Fulfillment.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFulfillmentModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // Database
            services.AddDbContext<FulfillmentDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("FulfillmentDb"),
                    b => b.MigrationsAssembly("TicketingSystem.Modules.Fulfillment.Infrastructure")));

            // Repositories
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketDeliveryRepository, TicketDeliveryRepository>();
            services.AddScoped<IOrderDataService, OrderDataService>();
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();


            // Services
            services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();
            services.AddScoped<IPdfTicketGenerator, PdfTicketGenerator>();
            services.AddScoped<IEmailService, EmailService>();

            // MediatR (Application Layer)
            services.AddMediatR(cfg =>
             cfg.RegisterServicesFromAssemblies(
                 Assembly.Load("TicketingSystem.Modules.Fulfillment.Application"),
                 Assembly.Load("TicketingSystem.Modules.Fulfillment.Infrastructure")));

            return services;
        }
    }
}
