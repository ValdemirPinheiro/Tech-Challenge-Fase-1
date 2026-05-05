using FCG.Domain.Exceptions;
using FCG.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Domain.Tests.ValueObjects;

public class PasswordTests
{
    [Theory]
    [InlineData("Abcdef1@")]
    [InlineData("Senha#2025")]
    [InlineData("Forte!Pass1")]
    public void Create_ComSenhaValida_DevePersistirOTextoPlano(string input)
    {
        var pwd = Password.Create(input);
        pwd.PlainText.Should().Be(input);
    }

    [Theory]
    [InlineData("", "vazia")]
    [InlineData("Abc1!", "menos de 8 caracteres")]
    [InlineData("Abcdefgh", "sem número e sem especial")]
    [InlineData("12345678", "sem letra e sem especial")]
    [InlineData("Abcdefg1", "sem caractere especial")]
    [InlineData("Abcdef!@", "sem número")]
    [InlineData("12345!@#", "sem letra")]
    public void Create_ComSenhaFraca_DeveLancarDomainException(string input, string motivo)
    {
        var act = () => Password.Create(input);
        act.Should().Throw<DomainException>(motivo);
    }

    [Fact]
    public void ToString_NaoDeveExporOTextoPlano()
    {
        var pwd = Password.Create("Senha@123");
        pwd.ToString().Should().Be("***");
    }
}
