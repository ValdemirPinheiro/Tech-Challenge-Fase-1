using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Promotions;
using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FCG.Application.Tests.UseCases.Promotions;

public class CreatePromotionUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepository = new();
    private readonly Mock<IPromotionRepository> _promotionRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreatePromotionUseCase BuildSut() => new(
        _gameRepository.Object,
        _promotionRepository.Object,
        _unitOfWork.Object,
        NullLogger<CreatePromotionUseCase>.Instance);

    [Fact]
    public async Task CriarPromocao_ComJogoExistente_DeveSalvar()
    {
        var game = Game.Create("Hades", "Indie roguelike", "Action", 49.90m, DateTime.UtcNow);
        _gameRepository.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var request = new CreatePromotionRequest(game.Id, 30m, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.DiscountPercentage.Should().Be(30m);
        _promotionRepository.Verify(r => r.AddAsync(It.IsAny<Promotion>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarPromocao_ComJogoInexistente_DeveRetornarNotFound()
    {
        _gameRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var request = new CreatePromotionRequest(Guid.NewGuid(), 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var result = await BuildSut().ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
