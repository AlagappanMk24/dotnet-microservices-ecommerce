namespace Ecommerce.SharedLibrary.Domain;

public abstract record BaseEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt { get; init; }
        = DateTimeOffset.UtcNow;
}