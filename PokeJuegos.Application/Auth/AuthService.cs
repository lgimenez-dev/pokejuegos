using System.Security.Cryptography;
using System.Text;
using PokeJuegos.Application.Helpers;
using PokeJuegos.Application.Services;
using PokeJuegos.Common.Enums;
using PokeJuegos.Domain;
using PokeJuegos.Domain.Entities;
using PokeJuegos.DTO;
using Microsoft.Extensions.Configuration;

namespace PokeJuegos.Application.Auth;

/// <summary>
/// Implements <see cref="IAuthService"/> using <see cref="IUnitOfWork"/>, <see cref="IJwtService"/>,
/// <see cref="IPasswordService"/>, and <see cref="IEmailService"/>.
/// Session tokens are stored as SHA-256 hex hashes; activation and recovery tokens are random GUIDs.
/// </summary>
public sealed class AuthService( IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordService passwordService, IEmailService emailService, IConfiguration configuration) : IAuthService
{
    /// <inheritdoc/>
    public async Task<Result<bool>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        User? existing = await unitOfWork.Users.GetByEmailAsync(request.Email!, ct);
        if (existing is not null)
        {
            return ErrorMessages.EmailAlreadyRegistered;
        }

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            User? existingByUsername = await unitOfWork.Users.GetByUsernameAsync(request.Username, ct);
            if (existingByUsername is not null)
            {
                return ErrorMessages.UsernameAlreadyRegistered;
            }
        }

        string passwordHash = passwordService.Hash(request.Password!);
        User user = User.Create(request.Email!, passwordHash, UserRole.Member, UserProvider.Local, username: request.Username);

        string activationToken = Guid.NewGuid().ToString("N");
        user.UpdateToken(activationToken, DateTime.UtcNow.AddHours(24));

        Person person = Person.Create(user.Id, request.Name!, request.NationalId, request.Phone, request.BirthDate);

        await unitOfWork.Users.AddAsync(user, ct);
        await unitOfWork.Persons.AddAsync(person, ct);
        await unitOfWork.SaveChangesAsync(ct);

        SmtpConfig? smtpConfig = await unitOfWork.SmtpConfigs.GetAsync(ct);
        if (smtpConfig is not null)
        {
            string frontendUrl = configuration["App:FrontendUrl"]!.TrimEnd('/');
            string appName = configuration["App:Name"]!;
            string link = $"{frontendUrl}/activate?token={activationToken}";
            string body = EmailHelper.AccountActivation(request.Name!, link, appName);
            await emailService.SendAsync(smtpConfig, request.Email!, $"Activate your account in {appName}", body, ct);
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        User? user = !string.IsNullOrWhiteSpace(request.Email)
            ? await unitOfWork.Users.GetByEmailAsync(request.Email, ct)
            : await unitOfWork.Users.GetByUsernameAsync(request.Username!, ct);
        if (user is null)
        {
            return ErrorMessages.InvalidCredentials;
        }

        if (user.Status == UserStatus.Pending)
        {
            return ErrorMessages.AccountPending;
        }

        if (user.Status == UserStatus.Inactive)
        {
            return ErrorMessages.AccountInactive;
        }

        // OAuth2 accounts have no password until they create one through the recovery flow.
        if (user.PasswordHash is null || !passwordService.Verify(request.Password!, user.PasswordHash))
        {
            return ErrorMessages.InvalidCredentials;
        }

        IReadOnlyList<UserSession> activeSessions = await unitOfWork.UserSessions.ListActiveByUserIdAsync(user.Id, ct);
        foreach (UserSession session in activeSessions)
        {
            session.Revoke();
        }

        Person? person = await unitOfWork.Persons.GetByUserIdAsync(user.Id, ct);

        string token = jwtService.GenerateToken(user.Id, person!.Name, user.Email, user.Role);
        string tokenHash = HashToken(token);

        UserSession newSession = UserSession.Create(
            user.Id,
            tokenHash,
            userAgent: null,
            ip: null,
            expirationDate: DateTime.UtcNow.AddMinutes(jwtService.ExpirationMinutes));

        await unitOfWork.UserSessions.AddAsync(newSession, ct);

        if (user.RefreshToken is not null)
        {
            user.RevokeToken();
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(token, user.Email, user.Role.ToString(), ProfileComplete: user.Username is not null);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> LogoutAsync(string token, CancellationToken ct = default)
    {
        string tokenHash = HashToken(token);
        UserSession? session = await unitOfWork.UserSessions.GetByTokenHashAsync(tokenHash, ct);

        if (session is null || !session.IsValid)
        {
            return ErrorMessages.SessionNotFound;
        }

        session.Revoke();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc/>
    public async Task<Result<ValidateTokenResponse>> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        string tokenHash = HashToken(token);
        UserSession? session = await unitOfWork.UserSessions.GetByTokenHashAsync(tokenHash, ct);

        if (session is null || !session.IsValid)
        {
            return ErrorMessages.SessionNotFound;
        }

        return new ValidateTokenResponse(token);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByEmailAsync(request.Email!, ct);
        if (user is null || user.Status == UserStatus.Pending)
        {
            return true;
        }

        Person? person = await unitOfWork.Persons.GetByUserIdAsync(user.Id, ct);

        string recoveryToken = Guid.NewGuid().ToString("N");
        user.UpdateToken(recoveryToken, DateTime.UtcNow.AddMinutes(30));
        await unitOfWork.SaveChangesAsync(ct);

        SmtpConfig? smtpConfig = await unitOfWork.SmtpConfigs.GetAsync(ct);
        if (smtpConfig is not null)
        {
            string frontendUrl = configuration["App:FrontendUrl"]!.TrimEnd('/');
            string appName = configuration["App:Name"]!;
            string link = $"{frontendUrl}/reset-password?token={recoveryToken}";
            string name = person?.Name ?? user.Email;

            // OAuth2 accounts (e.g. Google) start without a password: the same link creates one.
            (string subject, string body) = user.PasswordHash is null
                ? ($"Create a password for {appName}", EmailHelper.PasswordCreation(name, link, appName))
                : ($"Reset your password in {appName}", EmailHelper.PasswordRecovery(name, link, appName));
            await emailService.SendAsync(smtpConfig, user.Email, subject, body, ct);
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ActivateAsync(ActivateAccountRequest request, CancellationToken ct = default)
    {
        // Recovery tokens live in the same column: only a Pending account's token activates.
        User? user = await unitOfWork.Users.GetByRefreshTokenAsync(request.Token!, ct);
        if (user is null || user.ExpirationDate < DateTime.UtcNow || user.Status != UserStatus.Pending)
        {
            return ErrorMessages.InvalidOrExpiredToken;
        }

        user.Activate();
        user.RevokeToken();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByRefreshTokenAsync(request.Token!, ct);
        if (user is null || user.ExpirationDate < DateTime.UtcNow)
        {
            return ErrorMessages.InvalidOrExpiredToken;
        }

        string passwordHash = passwordService.Hash(request.Password!);
        user.ChangePassword(passwordHash);
        user.RevokeToken();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> CompleteProfileAsync(Guid userId, CompleteProfileRequest request, CancellationToken ct = default)
    {
        User? user = await unitOfWork.Users.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return ErrorMessages.UserNotFound;
        }

        if (user.Username is not null)
        {
            return ErrorMessages.ProfileAlreadyComplete;
        }

        User? existingByUsername = await unitOfWork.Users.GetByUsernameAsync(request.Username!, ct);
        if (existingByUsername is not null)
        {
            return ErrorMessages.UsernameAlreadyRegistered;
        }

        user.SetUsername(request.Username!);

        Person? person = await unitOfWork.Persons.GetByUserIdAsync(user.Id, ct);
        if (person is not null && person.Name != request.Name)
        {
            person.UpdateName(request.Name!);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    /// <summary>
    /// Returns the SHA-256 hex hash of the given token string.
    /// Used to store and look up JWT session tokens without persisting the raw token.
    /// </summary>
    /// <param name="token">The raw JWT string to hash.</param>
    /// <returns>An uppercase hexadecimal SHA-256 digest.</returns>
    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
