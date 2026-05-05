using FCG.Domain.Enums;
using FCG.Domain.Exceptions;
using FCG.Domain.ValueObjects;

namespace FCG.Domain.Entities;

/// <summary>
/// Aggregate Root que representa um usuário da plataforma FCG.
/// Encapsula nome, e-mail, senha (hashed) e papel (Role).
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core
    private User() { }

    private User(Guid id, string name, Email email, string passwordHash, UserRole role)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory para criação de novo usuário. O hash da senha deve ser calculado por um serviço de infraestrutura.
    /// </summary>
    public static User Create(string name, Email email, string passwordHash, UserRole role = UserRole.User)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "O nome não pode ser vazio.");
        DomainException.ThrowIf(name.Trim().Length < 2, "O nome deve ter pelo menos 2 caracteres.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(passwordHash), "O hash de senha é obrigatório.");

        return new User(Guid.NewGuid(), name.Trim(), email, passwordHash, role);
    }

    public void UpdateName(string newName)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(newName), "O nome não pode ser vazio.");
        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(newPasswordHash), "Hash de senha inválido.");
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DemoteToUser()
    {
        Role = UserRole.User;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAdmin() => Role == UserRole.Admin;
}
