namespace OzunuInkisaf.Domain.Common;

/// <summary>
/// Base class for every aggregate/entity in the domain. Gives every entity a
/// stable GUID identity and a creation timestamp so we never rely on
/// database-generated integer identities leaking into the domain model.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
