using FCG.Domain.Exceptions;
using FCG.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("usuario@dominio.com")]
    [InlineData("Joao.Silva@fcg.com.br")]
    [InlineData("teste+tag@empresa.io")]
    public void Create_ComEmailValido_DeveNormalizarEmLowercase(string input)
    {
        var email = Email.Create(input);
        email.Value.Should().Be(input.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("semArroba")]
    [InlineData("dominio.com")]
    [InlineData("teste@")]
    [InlineData("@dominio.com")]
    [InlineData("teste@dominio")]
    public void Create_ComEmailInvalido_DeveLancarDomainException(string input)
    {
        var act = () => Email.Create(input);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equality_DeveSerBaseadaNoValor()
    {
        var a = Email.Create("user@x.com");
        var b = Email.Create("USER@x.com");
        a.Should().Be(b);
    }
}
