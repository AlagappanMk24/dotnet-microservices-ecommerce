// File: ProductApi.Infrastructure/DependencyInjection.cs
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Application.Common.Interfaces;
using ProductApi.Infrastructure.Messaging;
using ProductApi.Infrastructure.Persistence;
using ProductApi.Infrastructure.Repositories;

namespace ProductApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("ProductDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'ProductDatabase' is missing.");

        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null)));

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IProductEventPublisher, RabbitMqProductEventPublisher>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMq:HostName"] ?? "localhost",
                    configuration["RabbitMq:VirtualHost"] ?? "/",
                    host =>
                    {
                        host.Username(
                            configuration["RabbitMq:UserName"] ?? "guest");

                        host.Password(
                            configuration["RabbitMq:Password"] ?? "guest");
                    });
            });
        });

        return services;
    }
}