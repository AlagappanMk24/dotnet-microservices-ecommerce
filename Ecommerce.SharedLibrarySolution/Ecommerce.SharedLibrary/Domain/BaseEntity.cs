namespace Ecommerce.SharedLibrary.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; protected set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; protected set; }

    public bool IsDeleted { get; protected set; }
    public void MarkUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    public void MarkDeleted()
    {
        IsDeleted = true;
        MarkUpdated();
    }
}