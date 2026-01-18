using Microsoft.EntityFrameworkCore;
using TicketingSystem.Modules.Identity.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen; 
using Swashbuckle.AspNetCore.SwaggerUI; 
using Swashbuckle.AspNetCore.Swagger; 
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

