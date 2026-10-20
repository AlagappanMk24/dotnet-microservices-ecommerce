namespace AuthenticationApi.Application.Auth.Exceptions
{
    public sealed class AccountLockedException(DateTimeOffset until) : Exception("The account is temporarily locked.")
    {
        public DateTimeOffset Until { get; } = until;
    }
}
