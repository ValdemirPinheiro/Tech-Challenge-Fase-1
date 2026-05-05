using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Auth;
using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FCG.Application.Tests.UseCases.Auth;

/// <summary>
/// Testes de comportamento (BDD-style Given/When/Then) para o use case de cadastro.
/// Demonstra o requisito "Aplicar TDD ou BDD em pelo menos um módulo".
/// </summary>
public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private RegisterUserUseCase BuildSut() => new(
        _userRepository.Object,
        _passwordHasher.Object,
        _unitOfWork.Object,
        NullLogger<RegisterUserUseCase>.Instance);

    [Fact(DisplayName = "Dado dados válidos e e-mail inédito, Quando registrar, Então cria usuário e retorna sucesso")]
    public async Task Cadastro_ComDadosValidos_DeveCriarUsuario()
    {
        // Given
        var request = new RegisterUserRequest("José", "jose@fcg.com", "Senha@123");
        _userRepository.Setup(r => r.EmailExistsAsync("jose@fcg.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("Senha@123")).Returns("hashed");

        // When
        var result = await BuildSut().ExecuteAsync(request);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("jose@fcg.com");
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Dado e-mail já cadastrado, Quando registrar, Então retorna conflito")]
    public async Task Cadastro_ComEmailDuplicado_DeveRetornarConflito()
    {
        var request = new RegisterUserRequest("José", "jose@fcg.com", "Senha@123");
        _userRepository.Setup(r => r.EmailExistsAsync("jose@fcg.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Theory(DisplayName = "Dado e-mail mal-formatado, Quando registrar, Então retorna erro de validação")]
    [InlineData("nao-tem-arroba")]
    [InlineData("teste@")]
    public async Task Cadastro_ComEmailInvalido_DeveFalhar(string invalid)
    {
        var request = new RegisterUserRequest("José", invalid, "Senha@123");
        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }

    [Theory(DisplayName = "Dado senha fraca, Quando registrar, Então retorna erro de validação")]
    [InlineData("curta")]
    [InlineData("Abcdefgh")]
    [InlineData("12345678")]
    public async Task Cadastro_ComSenhaFraca_DeveFalhar(string weak)
    {
        var request = new RegisterUserRequest("José", "jose@fcg.com", weak);
        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }
}
