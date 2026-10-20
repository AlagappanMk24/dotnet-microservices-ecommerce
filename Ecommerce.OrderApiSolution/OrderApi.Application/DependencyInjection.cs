using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Application.Features.Orders.Commands.CreateOrder;

namespace OrderApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);
        return services;
    }
}