using Ecommerce.SharedLibrary.Domain;

namespace AuthenticationApi.Domain.Entities;

/// <summary>
/// Persisted local account. Passwords are stored only as Identity password hashes.
/// </summary>
public sealed class UserAccount : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Customer";
    public bool IsActive { get; private set; } = true;
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockoutEndsAt { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }

    private UserAccount() { }

    public UserAccount(string email, string passwordHash)
    {
        Email = email.Trim();
        NormalizedEmail = email.Trim().ToUpperInvariant();
        PasswordHash = passwordHash;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch();
    }

    public void RecordFailedLogin(DateTimeOffset now)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= 5)
        {
            LockoutEndsAt = now.AddMinutes(15);
            FailedLoginCount = 0;
        }
        Touch();
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginCount = 0;
        LockoutEndsAt = null;
        Touch();
    }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEndsAt.HasValue && LockoutEndsAt.Value > now;

    public void RotateRefreshToken(string tokenHash, DateTimeOffset expiresAt)
    {
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = expiresAt;
        Touch();
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        RevokeRefreshToken();
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}