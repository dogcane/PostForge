using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PostForge.Application.Common.Interfaces;

namespace PostForge.Infrastructure.Identity;

public sealed class UserAccountService(UserManager<ApplicationUser> userManager) : IUserAccountService
{
    public async Task<Guid> CreateUserAsync(string email, string password, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public Task<string?> GetUserEmailAsync(Guid userId, CancellationToken ct)
        => userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);

    public Task<bool> IsSuperUserAsync(Guid userId, CancellationToken ct)
        => userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IsSuperUser)
            .FirstOrDefaultAsync(ct);

    public Task<bool> UserExistsAsync(string email, CancellationToken ct)
        => userManager.Users
            .AnyAsync(u => u.Email == email, ct);
}