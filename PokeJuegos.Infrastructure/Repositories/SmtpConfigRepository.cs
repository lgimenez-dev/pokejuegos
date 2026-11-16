using PokeJuegos.Domain.Entities;
using PokeJuegos.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PokeJuegos.Infrastructure.Repositories;

internal sealed class SmtpConfigRepository : ISmtpConfigRepository
{
    private readonly AppDbContext _context;

    public SmtpConfigRepository(AppDbContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<SmtpConfig?> GetAsync(CancellationToken ct = default) =>
        await _context.SmtpConfigs.FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(SmtpConfig smtpConfig, CancellationToken ct = default) =>
        await _context.SmtpConfigs.AddAsync(smtpConfig, ct);
}
