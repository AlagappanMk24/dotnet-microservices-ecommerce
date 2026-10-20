namespace AuthenticationApi.Application.Auth.Contracts;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserProfile User);