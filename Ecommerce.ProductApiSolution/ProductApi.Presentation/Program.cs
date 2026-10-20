using Ecommerce.SharedLibrary.Middleware;
using ProductApi.Application;
using ProductApi.Infrastructure;
using ProductApi.Presentation.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProductApiPresentation(builder.Configuration);

var app = builder.Build();

// Shared exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Adds an interactive UI at /scalar/v1
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live");

app.MapHealthChecks("/health/ready");

app.Run();

public partial class Program
{
}