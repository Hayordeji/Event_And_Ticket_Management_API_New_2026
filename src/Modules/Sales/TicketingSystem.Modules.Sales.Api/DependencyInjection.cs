using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.Modules.Sales.Application.Services.Flutterwave;
using TicketingSystem.Modules.Sales.Application.Services.Paystack;
using TicketingSystem.Modules.Sales.Domain.Events;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence.Repositories;
using TicketingSystem.SharedKernel.Outbox;

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
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IEventValidationService, EventValidationService>();
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
            services.AddHttpClient<IPaymentGatewayRefundService, PaystackRefundService>(client =>
            {
                client.BaseAddress = new Uri("https://api.paystack.co/");
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["Paystack:SecretKey"]);

            });

            services.AddHttpClient<IPaymentGatewayRefundService, FlutterwaveRefundService>(client =>
            {
                client.BaseAddress = new Uri("https://api.flutterwave.com/");
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["Flutterwave:SecretKey"]);

            });
            services.AddHttpClient<IPaymentGatewayService, PaystackService>(client =>
            {
                client.BaseAddress = new Uri("https://api.paystack.co/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["Paystack:SecretKey"]);

            });

            services.AddHttpClient<IPaymentGatewayService, FlutterwaveService>(client =>
            {
                client.BaseAddress = new Uri("https://api.flutterwave.com/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["Flutterwave:SecretKey"]);

               
            });
            services.AddScoped<IOrderDataService, OrderDataService>();


            // MediatR (Commands & Queries)
            services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(
                Assembly.Load("TicketingSystem.Modules.Sales.Application"),
                Assembly.Load("TicketingSystem.Modules.Sales.Infrastructure")));

            return services;
        }
    }
}
