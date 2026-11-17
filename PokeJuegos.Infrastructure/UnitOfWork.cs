using PokeJuegos.Domain;
using PokeJuegos.Domain.Repositories;
using PokeJuegos.Infrastructure.Repositories;

namespace PokeJuegos.Infrastructure;

/// <summary>
/// Coordinates changes across all repositories and persists them as a single atomic operation
/// by sharing a single <see cref="AppDbContext"/> instance.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    /// <inheritdoc/>
    public IUserRepository Users { get; }

    /// <inheritdoc/>
    public IPersonRepository Persons { get; }

    /// <inheritdoc/>
    public IUserSessionRepository UserSessions { get; }

    /// <inheritdoc/>
    public ISmtpConfigRepository SmtpConfigs { get; }

    /// <inheritdoc/>
    public IUserConfigRepository UserConfigs { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/> and creates all repository instances
    /// backed by the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Persons = new PersonRepository(context);
        UserSessions = new UserSessionRepository(context);
        SmtpConfigs = new SmtpConfigRepository(context);
        UserConfigs = new UserConfigRepository(context);
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    /// <inheritdoc/>
    public void Dispose() => _context.Dispose();
}
