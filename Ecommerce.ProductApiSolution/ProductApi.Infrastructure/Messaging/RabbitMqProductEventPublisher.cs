using Ecommerce.SharedLibrary.Events;
using MassTransit;
using ProductApi.Application.Common.Interfaces;

namespace ProductApi.Infrastructure.Messaging
{
    public class RabbitMqProductEventPublisher(IPublishEndpoint publishEndpoint) : IProductEventPublisher
    {
        public Task PublishProductCreatedAsync(
            ProductCreatedEvent productCreatedEvent,
            CancellationToken cancellationToken)
        {
            return publishEndpoint.Publish(
                productCreatedEvent,
                cancellationToken);
        }
    }
}