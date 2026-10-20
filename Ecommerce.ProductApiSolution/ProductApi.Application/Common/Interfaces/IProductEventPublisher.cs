using Ecommerce.SharedLibrary.Events;

namespace ProductApi.Application.Common.Interfaces;

public interface IProductEventPublisher
{
    Task PublishProductCreatedAsync(
        ProductCreatedEvent productCreatedEvent,
        CancellationToken cancellationToken);
}