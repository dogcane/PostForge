using PostForge.Application.Auth.DTOs;

namespace PostForge.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken ct);
}