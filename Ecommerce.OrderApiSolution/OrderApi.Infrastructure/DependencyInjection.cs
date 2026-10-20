using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Common.Interfaces;
using OrderApi.Infrastructure.Clients;
using OrderApi.Infrastructure.Messaging;
using OrderApi.Infrastructure.Persistence.Context;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderDatabase")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderEventPublisher, RabbitMqOrderEventPublisher>();

        var productBaseUrl = configuration["Services:ProductApi:BaseUrl"]
            ?? throw new InvalidOperationException("Services:ProductApi:BaseUrl is not configured.");

        services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
        {
            client.BaseAddress = new Uri(productBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddMassTransit(x =>
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var user = configuration["RabbitMq:Username"] ?? "guest";
                var pass = configuration["RabbitMq:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(user);
                    h.Password(pass);
                });
            }));

        return services;
    }
}