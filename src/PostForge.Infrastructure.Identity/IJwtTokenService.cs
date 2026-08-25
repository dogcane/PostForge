namespace PostForge.Infrastructure.Identity;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user);
    string CreateRefreshToken();
    string HashToken(string token);
}