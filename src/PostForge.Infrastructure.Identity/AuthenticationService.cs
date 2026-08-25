using Microsoft.AspNetCore.Identity;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Common.Interfaces;

namespace PostForge.Infrastructure.Identity;

public sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService) : IAuthenticationService
{
    public async Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
            return null;

        var (token, expiresAtUtc) = jwtTokenService.CreateToken(user);
        var (plainRefresh, entity) = await refreshTokenService.CreateAsync(user.Id, ct);

        return new LoginResultDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            RefreshToken = plainRefresh,
            RefreshExpiresAtUtc = entity.ExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email ?? email,
            IsSuperUser = user.IsSuperUser
        };
    }

    public async Task<LoginResultDto?> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hash = jwtTokenService.HashToken(refreshToken);
        var stored = await refreshTokenService.FindByHashAsync(hash, ct);

        if (stored is null)
            return null;

        if (stored.IsRevoked)
        {
            if (stored.ReplacedByTokenHash is not null)
                await refreshTokenService.RevokeAllForUserAsync(stored.UserId, ct);
            return null;
        }

        if (stored.IsExpired)
            return null;

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
            return null;

        var (newToken, newExpiresAt) = jwtTokenService.CreateToken(user);
        var (newPlainRefresh, newEntity) = await refreshTokenService.CreateAsync(user.Id, ct);
        var newHash = jwtTokenService.HashToken(newPlainRefresh);

        await refreshTokenService.RevokeAsync(stored, newHash, ct);

        return new LoginResultDto
        {
            Token = newToken,
            ExpiresAtUtc = newExpiresAt,
            RefreshToken = newPlainRefresh,
            RefreshExpiresAtUtc = newEntity.ExpiresAtUtc,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            IsSuperUser = user.IsSuperUser
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        var hash = jwtTokenService.HashToken(refreshToken);
        var stored = await refreshTokenService.FindByHashAsync(hash, ct);

        if (stored is null || stored.IsRevoked)
            return false;

        await refreshTokenService.RevokeAsync(stored, null, ct);
        return true;
    }
}