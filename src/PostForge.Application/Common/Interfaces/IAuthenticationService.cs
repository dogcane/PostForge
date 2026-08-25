using PostForge.Application.Auth.DTOs;

namespace PostForge.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken ct);
    Task<LoginResultDto?> RefreshAsync(string refreshToken, CancellationToken ct);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken ct);
}