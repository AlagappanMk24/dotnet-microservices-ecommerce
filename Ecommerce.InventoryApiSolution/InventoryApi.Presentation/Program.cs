using Ecommerce.SharedLibrary.Middleware;
using Ecommerce.SharedLibrary.Security;
using InventoryApi.Infrastructure;
using InventoryApi.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddJwtBearerValidation(builder.Configuration);
//builder.Services.AddAuthorization(options =>
//    options.AddPolicy("InventoryWrite", p => p.RequireAuthenticatedUser().RequireRole("Admin", "InventoryManager")));
builder.Services.AddAuthorization(options =>
    options.AddPolicy("InventoryWrite", p => p.RequireAuthenticatedUser().RequireRole("Admin", "InventoryManager", "Customer")));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<InventoryDbContext>("inventory-database");

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/ready");
app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));

app.Run();

public partial class Program { }