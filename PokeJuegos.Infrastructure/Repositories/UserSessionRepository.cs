using PokeJuegos.Domain.Entities;
using PokeJuegos.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PokeJuegos.Infrastructure.Repositories;

internal sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly AppDbContext _context;

    public UserSessionRepository(AppDbContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        await _context.UserSessions.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserSession>> ListActiveByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpirationDate > DateTime.UtcNow)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(UserSession session, CancellationToken ct = default) =>
        await _context.UserSessions.AddAsync(session, ct);
}
