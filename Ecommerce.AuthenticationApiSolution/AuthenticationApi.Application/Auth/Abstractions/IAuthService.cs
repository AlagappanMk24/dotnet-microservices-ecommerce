using AuthenticationApi.Application.Auth.Contracts;

namespace AuthenticationApi.Application.Auth.Abstractions;
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
}
