using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger; 
using Swashbuckle.AspNetCore.SwaggerGen; 
using Swashbuckle.AspNetCore.SwaggerUI;
using TicketingSystem.Api.Middleware;
using TicketingSystem.Modules.Identity.Infrastructure;

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
    builder.Services.AddSwaggerGen();
    builder.Services.AddOpenApi();
    builder.Services.AddDbContext<IdentityDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("IdentityDb"),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            }));
    builder.Services.AddDbContext<IdentityDbContext>(options =>
       options.UseSqlServer(
           builder.Configuration.GetConnectionString("FinanceDb"),
           sqlOptions =>
           {
               sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
               sqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 3,
                   maxRetryDelay: TimeSpan.FromSeconds(5),
                   errorNumbersToAdd: null);
           }));
    builder.Services.AddDbContext<IdentityDbContext>(options =>
       options.UseSqlServer(
           builder.Configuration.GetConnectionString("CatalogDb"),
           sqlOptions =>
           {
               sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
               sqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 3,
                   maxRetryDelay: TimeSpan.FromSeconds(5),
                   errorNumbersToAdd: null);
           }));
    builder.Services.AddDbContext<IdentityDbContext>(options =>
       options.UseSqlServer(
           builder.Configuration.GetConnectionString("SalesDb"),
           sqlOptions =>
           {
               sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
               sqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 3,
                   maxRetryDelay: TimeSpan.FromSeconds(5),
                   errorNumbersToAdd: null);
           }));
    builder.Services.AddDbContext<IdentityDbContext>(options =>
       options.UseSqlServer(
           builder.Configuration.GetConnectionString("FulfillmentDb"),
           sqlOptions =>
           {
               sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
               sqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 3,
                   maxRetryDelay: TimeSpan.FromSeconds(5),
                   errorNumbersToAdd: null);
           }));
    builder.Services.AddDbContext<IdentityDbContext>(options =>
       options.UseSqlServer(
           builder.Configuration.GetConnectionString("AccessDb"),
           sqlOptions =>
           {
               sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
               sqlOptions.EnableRetryOnFailure(
                   maxRetryCount: 3,
                   maxRetryDelay: TimeSpan.FromSeconds(5),
                   errorNumbersToAdd: null);
           }));

    //CORS CONFIG
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // React/Next.js typical ports
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });








    var app = builder.Build();

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

