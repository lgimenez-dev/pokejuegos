using PokeJuegos.Domain;
using PokeJuegos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PokeJuegos.Infrastructure;

/// <summary>
/// EF Core database context for PokeJuegos. Applies all entity configurations from this assembly
/// and automatically stamps <c>UpdatedAt</c> on modified entities before persisting.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>Gets or sets the users dataset.</summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>Gets or sets the persons dataset.</summary>
    public DbSet<Person> Persons { get; set; } = null!;

    /// <summary>Gets or sets the user sessions dataset.</summary>
    public DbSet<UserSession> UserSessions { get; set; } = null!;

    /// <summary>Gets or sets the SMTP configurations dataset.</summary>
    public DbSet<SmtpConfig> SmtpConfigs { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of <see cref="AppDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure this context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// Persists all pending changes to the database.
    /// Sets <c>UpdatedAt</c> to the current UTC time on every modified <see cref="AuditableEntity"/>
    /// before delegating to the base implementation.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (EntityEntry<AuditableEntity> entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(ct);
    }
}
