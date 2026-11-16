using PokeJuegos.Domain;
using PokeJuegos.Domain.Entities;
using PokeJuegos.DTO;

namespace PokeJuegos.Application.Sessions;

/// <summary>
/// Implements <see cref="IUserSessionService"/> using <see cref="IUnitOfWork"/>.
/// All session queries are scoped to an existing user; a missing user returns a failure result.
/// </summary>
public sealed class UserSessionService(IUnitOfWork unitOfWork) : IUserSessionService
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<UserSessionResponse>>> GetActiveSessionsByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return ErrorMessages.UserNotFound;
        }

        IReadOnlyList<UserSession> sessions = await unitOfWork.UserSessions.ListActiveByUserIdAsync(userId, ct);
        List<UserSessionResponse> mapped = sessions.Select(MapToResponse).ToList();
        return Result<IReadOnlyList<UserSessionResponse>>.Success(mapped);
    }

    /// <inheritdoc/>
    public async Task<Result<RevokeSessionsResponse>> RevokeSessionsByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return ErrorMessages.UserNotFound;
        }

        IReadOnlyList<UserSession> sessions = await unitOfWork.UserSessions.ListActiveByUserIdAsync(userId, ct);
        foreach (UserSession session in sessions)
        {
            session.Revoke();
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new RevokeSessionsResponse(sessions.Count);
    }

    /// <summary>
    /// Maps a <see cref="UserSession"/> entity to a <see cref="UserSessionResponse"/> DTO.
    /// </summary>
    /// <param name="session">The session entity to map.</param>
    /// <returns>A <see cref="UserSessionResponse"/> with the session's public fields.</returns>
    private static UserSessionResponse MapToResponse(UserSession session) =>
        new UserSessionResponse(
            session.Id,
            session.UserId,
            session.UserAgent,
            session.Ip,
            session.ExpirationDate,
            session.CreatedAt);
}
