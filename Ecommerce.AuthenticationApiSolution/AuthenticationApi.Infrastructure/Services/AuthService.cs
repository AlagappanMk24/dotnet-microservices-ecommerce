using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationApi.Application.Auth.Abstractions;
using AuthenticationApi.Application.Auth.Contracts;
using AuthenticationApi.Application.Auth.Exceptions;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationApi.Infrastructure.Services;

public sealed class AuthService(AuthDbContext db, IConfiguration configuration, TimeProvider timeProvider) : IAuthService
{
    private readonly PasswordHasher<UserAccount> _passwordHasher = new();

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        ValidatePassword(request.Password);

        if (await db.Users.AnyAsync(x => x.NormalizedEmail == email, cancellationToken))
            throw new DuplicateAccountException();

        var user = new UserAccount(request.Email.Trim(), "pending");
        user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new DuplicateAccountException();
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalized = NormalizeEmail(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalized, cancellationToken);

        // Same external error for unknown email and bad password prevents account enumeration.
        if (user is null || !user.IsActive)
            throw new InvalidCredentialsException();

        var now = timeProvider.GetUtcNow();

        if (user.IsLockedOut(now))
            throw new AccountLockedException(user.LockoutEndsAt!.Value);

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin(now);
            await db.SaveChangesAsync(cancellationToken);

            if (user.IsLockedOut(now))
                throw new AccountLockedException(user.LockoutEndsAt!.Value);

            throw new InvalidCredentialsException();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));

        user.RecordSuccessfulLogin();

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = HashToken(request.RefreshToken);
        var now = timeProvider.GetUtcNow();

        var user = await db.Users.SingleOrDefaultAsync(
            x => x.RefreshTokenHash == hash && x.RefreshTokenExpiresAt > now && x.IsActive,
            cancellationToken);

        if (user is null)
            throw new InvalidRefreshTokenException();

        // Rotate on every refresh. The previous token becomes invalid after this transaction commits.
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task RevokeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
            return;

        user.RevokeRefreshToken();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        return user is null ? null : new UserProfile(user.Id, user.Email, user.Role);
    }

    private async Task<AuthResult> IssueTokensAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is required.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is required.");
        var secret = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is required.");

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 UTF-8 bytes.");

        var now = timeProvider.GetUtcNow();
        var expires = now.AddMinutes(configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 15);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(issuer, audience, claims, now.UtcDateTime, expires.UtcDateTime, credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenDays = configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 14;

        user.RotateRefreshToken(HashToken(refreshToken), now.AddDays(refreshTokenDays));
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(accessToken, refreshToken, expires, new UserProfile(user.Id, user.Email, user.Role));
    }

    private static string NormalizeEmail(string email) => (email ?? string.Empty).Trim().ToUpperInvariant();

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || password.Length > 128)
            throw new ArgumentException("Password must contain between 12 and 128 characters.");
    }
}