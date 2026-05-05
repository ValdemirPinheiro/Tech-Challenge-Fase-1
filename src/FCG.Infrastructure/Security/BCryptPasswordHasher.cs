using FCG.Application.Abstractions;

namespace FCG.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

    public bool Verify(string plainPassword, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(plainPassword, hash); }
        catch { return false; }
    }
}
