namespace FCG.Domain.Enums;

/// <summary>
/// Define o nível de acesso de um usuário na plataforma FCG.
/// </summary>
public enum UserRole
{
    /// <summary>Usuário comum: acessa a plataforma e sua biblioteca de jogos.</summary>
    User = 0,

    /// <summary>Administrador: gerencia jogos, usuários e promoções.</summary>
    Admin = 1
}
