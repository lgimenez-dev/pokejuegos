using System.Security.Cryptography;
using System.Text;
using PokeJuegos.Application.Services;
using PokeJuegos.Common.Enums;
using PokeJuegos.Domain;
using PokeJuegos.Domain.Entities;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.OAuth2;

/// <summary>
/// Implements <see cref="IOAuth2Service"/> using <see cref="IUnitOfWork"/> and <see cref="IJwtService"/>.
/// </summary>
public sealed class OAuth2Service(IUnitOfWork unitOfWork, IJwtService jwtService) : IOAuth2Service
{
    /// <inheritdoc/>
    public async Task<Result<LoginResponse>> HandleGoogleCallbackAsync(string email, string name, string providerId, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByProviderAsync(UserProvider.Google, providerId, ct);

        if (user is null)
        {
            User? existingByEmail = await unitOfWork.Users.GetByEmailAsync(email, ct);
            if (existingByEmail is not null)
            {
                return ErrorMessages.ProviderMismatch;
            }

            user = User.CreateOAuth2(email, UserRole.Member, UserProvider.Google, providerId);
            Person person = Person.Create(user.Id, name, null, null, null);

            await unitOfWork.Users.AddAsync(user, ct);
            await unitOfWork.Persons.AddAsync(person, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            Person? person = await unitOfWork.Persons.GetByUserIdAsync(user.Id, ct);
            if (person is not null && person.Name != name)
            {
                person.UpdateName(name);
            }
        }

        IReadOnlyList<UserSession> activeSessions = await unitOfWork.UserSessions.ListActiveByUserIdAsync(user.Id, ct);
        foreach (UserSession session in activeSessions)
        {
            session.Revoke();
        }

        string token = jwtService.GenerateToken(user.Id, name, user.Email, user.Role);
        string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        UserSession newSession = UserSession.Create(
            user.Id,
            tokenHash,
            userAgent: null,
            ip: null,
            expirationDate: DateTime.UtcNow.AddMinutes(jwtService.ExpirationMinutes));

        await unitOfWork.UserSessions.AddAsync(newSession, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(token, user.Email, user.Role.ToString(), ProfileComplete: user.Username is not null);
    }
}
