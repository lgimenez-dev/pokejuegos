using PokeJuegos.Common.Enums;
using PokeJuegos.Domain.Entities;
using PokeJuegos.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PokeJuegos.Infrastructure.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    /// <inheritdoc/>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    /// <inheritdoc/>
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

    /// <inheritdoc/>
    public async Task<User?> GetByProviderAsync(UserProvider provider, string providerId, CancellationToken ct = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderId == providerId, ct);

    /// <inheritdoc/>
    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _context.Users.AddAsync(user, ct);
}
