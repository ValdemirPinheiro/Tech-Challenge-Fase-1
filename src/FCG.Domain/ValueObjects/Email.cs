using System.Text.RegularExpressions;
using FCG.Domain.Exceptions;

namespace FCG.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um e-mail válido.
/// Garante a invariante de que toda instância de Email respeita o formato esperado.
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>
    /// Cria um Email validado. Lança <see cref="DomainException"/> se o valor for inválido.
    /// </summary>
    public static Email Create(string value)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(value), "O e-mail não pode ser vazio.");

        var normalized = value.Trim().ToLowerInvariant();

        DomainException.ThrowIf(
            !EmailRegex.IsMatch(normalized),
            "Formato de e-mail inválido.");

        return new Email(normalized);
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
