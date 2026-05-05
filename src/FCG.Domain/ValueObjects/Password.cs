using FCG.Domain.Exceptions;

namespace FCG.Domain.ValueObjects;

/// <summary>
/// Value Object que representa uma senha em texto puro validada quanto à força.
/// Regra: mínimo de 8 caracteres, contendo pelo menos uma letra, um número e um caractere especial.
/// O Value Object não armazena a senha de forma persistente — ele é descartado após o hash.
/// </summary>
public sealed record Password
{
    public const int MinLength = 8;

    public string PlainText { get; }

    private Password(string plainText) => PlainText = plainText;

    public static Password Create(string plainText)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(plainText),
            "A senha não pode ser vazia.");

        DomainException.ThrowIf(
            plainText.Length < MinLength,
            $"A senha deve ter no mínimo {MinLength} caracteres.");

        DomainException.ThrowIf(
            !plainText.Any(char.IsLetter),
            "A senha deve conter pelo menos uma letra.");

        DomainException.ThrowIf(
            !plainText.Any(char.IsDigit),
            "A senha deve conter pelo menos um número.");

        DomainException.ThrowIf(
            !plainText.Any(IsSpecial),
            "A senha deve conter pelo menos um caractere especial.");

        return new Password(plainText);
    }

    private static bool IsSpecial(char c) =>
        !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c);

    public override string ToString() => "***";
}
