using Ecommerce.SharedLibrary.Events;
using MassTransit;
using OrderApi.Application.Common.Interfaces;

namespace OrderApi.Infrastructure.Messaging;

public sealed class RabbitMqOrderEventPublisher(IPublishEndpoint publishEndpoint) : IOrderEventPublisher
{
    public Task PublishAsync(OrderPlacedEvent @event, CancellationToken cancellationToken) =>
        publishEndpoint.Publish(@event, cancellationToken);
}