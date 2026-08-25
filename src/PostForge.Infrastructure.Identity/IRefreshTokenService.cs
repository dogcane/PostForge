namespace PostForge.Infrastructure.Identity;

public interface IRefreshTokenService
{
    Task<(string PlainToken, RefreshToken Entity)> CreateAsync(Guid userId, CancellationToken ct);
    Task<RefreshToken?> FindByHashAsync(string hash, CancellationToken ct);
    Task RevokeAsync(RefreshToken token, string? replacedByHash, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
    Task<int> CleanupExpiredAsync(CancellationToken ct);
}
