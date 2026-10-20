namespace AuthenticationApi.Application.Auth.Exceptions
{
    public sealed class InvalidCredentialsException() : Exception("The email or password is incorrect.");
}