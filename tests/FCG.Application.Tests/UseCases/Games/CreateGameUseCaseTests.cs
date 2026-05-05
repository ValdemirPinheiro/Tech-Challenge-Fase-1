using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Games;
using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FCG.Application.Tests.UseCases.Games;

public class CreateGameUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateGameUseCase BuildSut() => new(
        _gameRepository.Object,
        _unitOfWork.Object,
        NullLogger<CreateGameUseCase>.Instance);

    [Fact]
    public async Task Criar_ComDadosValidos_DevePersistir()
    {
        _gameRepository.Setup(r => r.TitleExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateGameRequest("The Witcher 3", "RPG épico", "RPG", 99.90m, new DateTime(2015, 5, 19));

        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("The Witcher 3");
        _gameRepository.Verify(r => r.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_ComTituloDuplicado_DeveRetornarConflito()
    {
        _gameRepository.Setup(r => r.TitleExistsAsync("Cyberpunk 2077", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateGameRequest("Cyberpunk 2077", "Open world", "RPG", 199m, DateTime.UtcNow);
        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Criar_ComPrecoNegativo_DeveFalhar()
    {
        _gameRepository.Setup(r => r.TitleExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateGameRequest("Jogo", "desc", "Action", -1m, DateTime.UtcNow);
        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }
}
