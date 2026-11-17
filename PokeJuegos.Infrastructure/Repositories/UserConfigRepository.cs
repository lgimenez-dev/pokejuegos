using PokeJuegos.Domain.Entities;
using PokeJuegos.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PokeJuegos.Infrastructure.Repositories;

internal sealed class UserConfigRepository : IUserConfigRepository
{
    private readonly AppDbContext _context;

    public UserConfigRepository(AppDbContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<UserConfig?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserConfigs.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    /// <inheritdoc/>
    public async Task AddAsync(UserConfig config, CancellationToken ct = default) =>
        await _context.UserConfigs.AddAsync(config, ct);
}
