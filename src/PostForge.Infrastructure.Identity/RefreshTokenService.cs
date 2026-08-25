using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace PostForge.Infrastructure.Identity;

public sealed class RefreshTokenService(AppIdentityDbContext dbContext, IJwtTokenService jwtTokenService, IOptions<AuthOptions> options) : IRefreshTokenService
{
    private readonly AppIdentityDbContext _dbContext = dbContext;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly AuthOptions _options = options.Value;

    public async Task<(string PlainToken, RefreshToken Entity)> CreateAsync(Guid userId, CancellationToken ct)
    {
        var plain = _jwtTokenService.CreateRefreshToken();
        var hash = _jwtTokenService.HashToken(plain);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiresInDays)
        };

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        return (plain, entity);
    }

    public Task<RefreshToken?> FindByHashAsync(string hash, CancellationToken ct)
        => _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

    public async Task RevokeAsync(RefreshToken token, string? replacedByHash, CancellationToken ct)
    {
        token.RevokedAtUtc = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByHash;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var t in activeTokens)
            t.RevokedAtUtc = DateTime.UtcNow;

        if (activeTokens.Count > 0)
            await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var expired = await _dbContext.RefreshTokens
            .Where(x => x.ExpiresAtUtc < cutoff || (x.RevokedAtUtc != null && x.RevokedAtUtc < cutoff))
            .ToListAsync(ct);

        if (expired.Count == 0)
            return 0;

        _dbContext.RefreshTokens.RemoveRange(expired);
        await _dbContext.SaveChangesAsync(ct);
        return expired.Count;
    }
}
