using System.ComponentModel.DataAnnotations;
namespace AuthenticationApi.Application.Auth.Contracts;

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(320)] string Email,
    [param: Required, MinLength(1), MaxLength(128)] string Password);