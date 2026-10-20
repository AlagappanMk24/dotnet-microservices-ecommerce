namespace AuthenticationApi.Application.Auth.Contracts
{
    public sealed record UserProfile(
        Guid Id,
        string Email,
        string Role);
}