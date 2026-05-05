using FCG.Application.Common;
using FCG.Application.UseCases.Games;
using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FCG.Application.Tests.UseCases.Games;

public class AcquireGameUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepository = new();
    private readonly Mock<ILibraryRepository> _libraryRepository = new();

    private AcquireGameUseCase BuildSut() => new(
        _gameRepository.Object,
        _libraryRepository.Object,
        NullLogger<AcquireGameUseCase>.Instance);

    [Fact(DisplayName = "Dado um jogo ativo e usuário sem o jogo, Quando adquirir, Então salva na biblioteca")]
    public async Task Adquirir_QuandoUsuarioNaoPossuiOJogo_DevePersistir()
    {
        var game = Game.Create("Stardew Valley", "Sim", "Indie", 39.90m, DateTime.UtcNow);
        var userId = Guid.NewGuid();

        _gameRepository.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>())).ReturnsAsync(game);
        _libraryRepository.Setup(r => r.UserOwnsGameAsync(userId, game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await BuildSut().ExecuteAsync(userId, game.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.GameTitle.Should().Be("Stardew Valley");
        _libraryRepository.Verify(r => r.AddAsync(It.IsAny<LibraryItem>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Dado um jogo já adquirido, Quando adquirir novamente, Então retorna conflito")]
    public async Task Adquirir_QuandoJaPossui_DeveRetornarConflito()
    {
        var game = Game.Create("Stardew Valley", "Sim", "Indie", 39.90m, DateTime.UtcNow);
        var userId = Guid.NewGuid();

        _gameRepository.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>())).ReturnsAsync(game);
        _libraryRepository.Setup(r => r.UserOwnsGameAsync(userId, game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await BuildSut().ExecuteAsync(userId, game.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact(DisplayName = "Dado um jogo desativado, Quando adquirir, Então retorna NotFound")]
    public async Task Adquirir_QuandoJogoInativo_DeveRetornarNotFound()
    {
        var game = Game.Create("Velho Jogo", "old", "Action", 1m, DateTime.UtcNow);
        game.Deactivate();

        _gameRepository.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var result = await BuildSut().ExecuteAsync(Guid.NewGuid(), game.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
