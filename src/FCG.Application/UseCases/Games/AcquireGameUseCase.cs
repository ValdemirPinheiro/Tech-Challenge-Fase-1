using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Games;

/// <summary>
/// Caso de uso onde o usuário autenticado adquire um jogo do catálogo.
/// Persiste o item em sua biblioteca (MongoDB).
/// </summary>
public class AcquireGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILibraryRepository _libraryRepository;
    private readonly ILogger<AcquireGameUseCase> _logger;

    public AcquireGameUseCase(
        IGameRepository gameRepository,
        ILibraryRepository libraryRepository,
        ILogger<AcquireGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _libraryRepository = libraryRepository;
        _logger = logger;
    }

    public async Task<Result<AcquireGameResponse>> ExecuteAsync(Guid userId, Guid gameId, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct);
        if (game is null || !game.IsActive)
        {
            return Result<AcquireGameResponse>.Failure("Jogo não disponível.", ErrorType.NotFound);
        }

        if (await _libraryRepository.UserOwnsGameAsync(userId, gameId, ct))
        {
            return Result<AcquireGameResponse>.Failure("Você já possui este jogo em sua biblioteca.", ErrorType.Conflict);
        }

        var price = game.GetCurrentPrice(DateTime.UtcNow);
        var item = LibraryItem.Create(userId, game.Id, game.Title, price);
        await _libraryRepository.AddAsync(item, ct);

        _logger.LogInformation("Usuário {UserId} adquiriu o jogo {GameId} por {Price}", userId, gameId, price);

        var response = new AcquireGameResponse(item.Id, item.GameId, item.GameTitle, item.PricePaid, item.AcquiredAt);
        return Result<AcquireGameResponse>.Success(response);
    }
}
