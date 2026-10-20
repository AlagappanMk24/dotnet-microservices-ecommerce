namespace AuthenticationApi.Application.Auth.Exceptions
{
    public sealed class InvalidRefreshTokenException() : Exception("The refresh token is invalid or expired.");
}