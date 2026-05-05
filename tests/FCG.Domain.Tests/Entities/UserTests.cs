using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Exceptions;
using FCG.Domain.ValueObjects;
using FluentAssertions;

namespace FCG.Domain.Tests.Entities;

public class UserTests
{
    private static Email AnyEmail() => Email.Create("user@fcg.com");

    [Fact]
    public void Create_ComDadosValidos_DeveCriarUsuarioPadraoComoUser()
    {
        var user = User.Create("José", AnyEmail(), "hash");
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("José");
        user.Role.Should().Be(UserRole.User);
        user.IsAdmin().Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    public void Create_ComNomeInvalido_DeveLancar(string name)
    {
        var act = () => User.Create(name, AnyEmail(), "hash");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_SemHash_DeveLancar()
    {
        var act = () => User.Create("José", AnyEmail(), "");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PromoteToAdmin_DeveAlterarRole()
    {
        var user = User.Create("Ana", AnyEmail(), "hash");
        user.PromoteToAdmin();
        user.Role.Should().Be(UserRole.Admin);
        user.IsAdmin().Should().BeTrue();
    }

    [Fact]
    public void DemoteToUser_DeveVoltarParaUser()
    {
        var user = User.Create("Ana", AnyEmail(), "hash", UserRole.Admin);
        user.DemoteToUser();
        user.Role.Should().Be(UserRole.User);
    }

    [Fact]
    public void ChangePasswordHash_DeveAtualizarHashEUpdatedAt()
    {
        var user = User.Create("Ana", AnyEmail(), "hash-old");
        user.ChangePasswordHash("hash-new");
        user.PasswordHash.Should().Be("hash-new");
        user.UpdatedAt.Should().NotBeNull();
    }
}
