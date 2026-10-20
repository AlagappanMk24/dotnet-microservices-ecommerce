using Ecommerce.SharedLibrary.Events;

namespace OrderApi.Application.Common.Interfaces;

public interface IOrderEventPublisher
{
    Task PublishAsync(OrderPlacedEvent @event, CancellationToken cancellationToken);
}