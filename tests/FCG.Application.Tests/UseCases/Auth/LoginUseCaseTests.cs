using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Auth;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Repositories;
using FCG.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FCG.Application.Tests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();

    private LoginUseCase BuildSut() => new(
        _userRepository.Object,
        _passwordHasher.Object,
        _jwtTokenService.Object,
        NullLogger<LoginUseCase>.Instance);

    private static User BuildUser() =>
        User.Create("José", Email.Create("jose@fcg.com"), "hashed", UserRole.User);

    [Fact(DisplayName = "Dado credenciais corretas, Quando autenticar, Então emite token JWT")]
    public async Task Login_ComCredenciaisValidas_DeveEmitirToken()
    {
        var user = BuildUser();
        _userRepository.Setup(r => r.GetByEmailAsync("jose@fcg.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("Senha@123", "hashed")).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(user))
            .Returns(new JwtTokenResult("token-fake", DateTime.UtcNow.AddHours(2)));

        var result = await BuildSut().ExecuteAsync(new LoginRequest("jose@fcg.com", "Senha@123"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token-fake");
        result.Value.User.Email.Should().Be("jose@fcg.com");
    }

    [Fact(DisplayName = "Dado e-mail inexistente, Quando autenticar, Então retorna 401")]
    public async Task Login_ComUsuarioInexistente_DeveRetornarUnauthorized()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await BuildSut().ExecuteAsync(new LoginRequest("ninguem@fcg.com", "Senha@123"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Dado senha incorreta, Quando autenticar, Então retorna 401")]
    public async Task Login_ComSenhaIncorreta_DeveRetornarUnauthorized()
    {
        var user = BuildUser();
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await BuildSut().ExecuteAsync(new LoginRequest("jose@fcg.com", "errada"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Unauthorized);
    }
}
