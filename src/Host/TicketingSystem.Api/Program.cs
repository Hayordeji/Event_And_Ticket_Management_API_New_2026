using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;
using TicketingSystem.Api.Middleware;
using TicketingSystem.Api.Services;
using TicketingSystem.Modules.Access.Api;
using TicketingSystem.Modules.Catalog.Api;
using TicketingSystem.Modules.Finance.Api;
using TicketingSystem.Modules.Finance.Infrastructure.Persistence;
using TicketingSystem.Modules.Fulfillment.Api;
using TicketingSystem.Modules.Identity.Api;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;
using TicketingSystem.Modules.Identity.Infrastructure.Seeders;
using TicketingSystem.Modules.Sales.Api;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.Modules.Sales.Application.Services.Flutterwave;
using TicketingSystem.Modules.Sales.Application.Services.Paystack;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Authorization;
using TicketingSystem.SharedKernel.Extensions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/ticketing-api-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();
try
{
    Log.Information("Starting Ticketing System API");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer(); 

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Ticketing System API",
            Version = "v1",
            Description = "Enterprise event ticketing and management system"
        }); 

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'your token in the text input below.\r\n\r\nExample: \"12345abcdef\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });

    builder.Services.AddRateLimiter(rateLimiterOptions =>
    {
        rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        rateLimiterOptions.AddFixedWindowLimiter("fixed_auth_login", options =>
        {
            options.PermitLimit = 5;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 0;
           
        });

        rateLimiterOptions.AddFixedWindowLimiter("fixed_auth_register", options =>
        {
            options.PermitLimit = 3;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 0;

        });

        rateLimiterOptions.AddFixedWindowLimiter("fixed_create_endpoints", options =>
        {
            options.PermitLimit = 10;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 0;

        });

        rateLimiterOptions.AddFixedWindowLimiter("fixed_get_endpoints", options =>
        {
            options.PermitLimit = 30;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 0;

        });

        rateLimiterOptions.AddFixedWindowLimiter("fixed_access_scan", options =>
        {
            options.PermitLimit = 60;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 0;

        });

        
    });



    builder.Services.AddScoped<DomainEventDispatcher>();
    builder.Services.AddHostedService<OutboxProcessorService>();

    builder.Services.AddHttpClient<IPaymentGatewayService, PaystackService>();
    builder.Services.AddHttpClient<IPaymentGatewayService, FlutterwaveService>();

    //builder.Services.AddMediatR(cfg =>
    //cfg.RegisterServicesFromAssemblyContaining<OrderPaidEvent>());

    builder.Services.Configure<PaystackConfig>(
    builder.Configuration.GetSection("PaymentGateways:Paystack"));
    builder.Services.AddOptions<PaystackConfig>()
    .ValidateOnStart();
    builder.Services.Configure<FlutterwaveConfig>(
    builder.Configuration.GetSection("PaymentGateways:Flutterwave"));
    builder.Services.AddOptions<FlutterwaveConfig>()
    .ValidateOnStart();


    //REGISTER MODULES
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddFinanceModule(builder.Configuration);
    builder.Services.AddCatalogModule(builder.Configuration);
    builder.Services.AddSalesModule(builder.Configuration);
    builder.Services.AddFulfillmentModule(builder.Configuration);
    builder.Services.AddAccessModule(builder.Configuration);
    builder.Services.AddSharedKernel(builder.Configuration);

    //CORS CONFIG
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000") 
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT Secret not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero // Remove default 5 min clock skew
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst("userId")?.Value;
                    Log.Information("JWT validated for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorization();
    builder.Services.AddTicketingAuthorizationPolicies();





    var app = builder.Build();


    //SEED ROLES
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await RoleSeeder.SeedAsync(roleManager);
    }


    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    //HEALTH CHECK
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));
       //.WithName("HealthCheck")
       //.WithOpenApi();

    app.MapControllers();

    


    Log.Information("Ticketing System API started successfully");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
       Log.CloseAndFlush();
}

