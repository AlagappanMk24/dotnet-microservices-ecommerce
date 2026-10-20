using System.ComponentModel.DataAnnotations;
namespace AuthenticationApi.Application.Auth.Contracts;
public sealed record RegisterRequest(
    [param: Required, EmailAddress, MaxLength(320)] string Email,
    [param: Required, MinLength(12), MaxLength(128)] string Password);