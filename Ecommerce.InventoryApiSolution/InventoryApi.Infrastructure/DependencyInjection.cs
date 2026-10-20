//using InventoryApi.Application.Abstractions;
//using InventoryApi.Application.Services;
//using InventoryApi.Infrastructure.Messaging;
//using InventoryApi.Infrastructure.Persistence;
//using InventoryApi.Infrastructure.Repositories;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace InventoryApi.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInventoryInfrastructure(
//        this IServiceCollection services,
//        IConfiguration configuration)
//    {
//        services.AddDbContext<InventoryDbContext>(options =>
//            options.UseSqlServer(configuration.GetConnectionString("InventoryDatabase")));

//        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

//        services.AddScoped<IInventoryRepository, InventoryRepository>();
//        services.AddScoped<IInventoryService, InventoryService>();

//        services.AddHostedService<OrderPlacedConsumer>();

//        return services;
//    }
//}


using InventoryApi.Application.Abstractions;
using InventoryApi.Application.Services;
using InventoryApi.Infrastructure.Messaging;
using InventoryApi.Infrastructure.Persistence;
using InventoryApi.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("InventoryDatabase")));

        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryService, InventoryService>();

        var rabbitMq = configuration
            .GetSection("RabbitMq")
            .Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException(
                "RabbitMq configuration is missing.");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ProductCreatedConsumer>();

            x.AddConsumer<OrderPlacedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                // Single host configuration for the broker
                cfg.Host(
                    rabbitMq.HostName,
                    rabbitMq.VirtualHost,
                    host =>
                    {
                        host.Username(rabbitMq.UserName);
                        host.Password(rabbitMq.Password);
                    });

                // Endpoint for ProductCreatedEvent
                cfg.ReceiveEndpoint(
                    "inventory.product-created",
                    endpoint =>
                    {
                        endpoint.ConfigureConsumer<ProductCreatedConsumer>(
                            context);
                    });

                // Endpoint for OrderPlacedEvent
                cfg.ReceiveEndpoint(
                    rabbitMq.Queue, 
                    endpoint =>
                    {
                        endpoint.ConfigureConsumer<OrderPlacedConsumer>(
                            context);
                    });
            });
        });

        return services;
    }
}
