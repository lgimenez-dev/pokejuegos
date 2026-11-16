using PokeJuegos.Domain.Entities;
using PokeJuegos.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PokeJuegos.Infrastructure.Repositories;

internal sealed class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _context;

    public PersonRepository(AppDbContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<Person?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.Persons.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    /// <inheritdoc/>
    public async Task<Person?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default) =>
        await _context.Persons.FirstOrDefaultAsync(p => p.NationalId == nationalId, ct);

    /// <inheritdoc/>
    public async Task AddAsync(Person person, CancellationToken ct = default) =>
        await _context.Persons.AddAsync(person, ct);
}
