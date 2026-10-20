using System.ComponentModel.DataAnnotations;
namespace AuthenticationApi.Application.Auth.Contracts;

public sealed record RefreshRequest(
    [property: Required, MinLength(32)] string RefreshToken);
