using Microsoft.AspNetCore.Identity;
using PostForge.Application.Auth.DTOs;
using PostForge.Application.Common.Interfaces;

namespace PostForge.Infrastructure.Identity;

public sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService) : IAuthenticationService
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

        return new LoginResultDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            Email = user.Email ?? email,
            IsSuperUser = user.IsSuperUser
        };
    }
}