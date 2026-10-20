using AuthenticationApi.Application.Auth;
using AuthenticationApi.Application.Auth.Abstractions;
using AuthenticationApi.Infrastructure.Persistence;
using AuthenticationApi.Infrastructure.Persistence.Context;
using AuthenticationApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

        services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

        // Register built-in TimeProvider for time-dependent operations and testing support (.NET 8+)
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}