namespace PostForge.Application.Common.Interfaces;

public interface IUserAccountService
{
    Task<Guid> CreateUserAsync(string email, string password, CancellationToken ct);
    Task<string?> GetUserEmailAsync(Guid userId, CancellationToken ct);
    Task<bool> IsSuperUserAsync(Guid userId, CancellationToken ct);
    Task<bool> UserExistsAsync(string email, CancellationToken ct);
}