namespace AuthenticationApi.Application.Auth.Exceptions
{
    public sealed class DuplicateAccountException() : Exception("An account with this email already exists.");
}
