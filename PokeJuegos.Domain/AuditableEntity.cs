namespace PokeJuegos.Domain;

/// <summary>
/// Base class for all domain entities. Provides identity and audit timestamp tracking.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>Gets the unique identifier of the entity.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the UTC date and time when the entity was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Gets the UTC date and time of the last update, or <see langword="null"/> if never updated.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes <see cref="Id"/> and <see cref="CreatedAt"/> for a newly created entity.
    /// Must be called once inside the derived entity's <c>Create</c> factory method.
    /// </summary>
    protected void InitAudit()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets <see cref="UpdatedAt"/> to the current UTC time.
    /// Must be called at the end of every mutating method on a derived entity.
    /// </summary>
    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
