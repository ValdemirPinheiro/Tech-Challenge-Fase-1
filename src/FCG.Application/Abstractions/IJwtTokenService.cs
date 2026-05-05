using FCG.Domain.Entities;

namespace FCG.Application.Abstractions;

public interface IJwtTokenService
{
    /// <summary>Gera um JWT contendo claims essenciais (sub, email, role).</summary>
    JwtTokenResult GenerateToken(User user);
}

public record JwtTokenResult(string Token, DateTime ExpiresAt);
