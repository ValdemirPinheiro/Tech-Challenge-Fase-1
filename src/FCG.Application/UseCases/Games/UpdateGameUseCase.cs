using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Exceptions;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Games;

public class UpdateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateGameUseCase> _logger;

    public UpdateGameUseCase(IGameRepository gameRepository, IUnitOfWork unitOfWork, ILogger<UpdateGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GameResponse>> ExecuteAsync(Guid gameId, UpdateGameRequest request, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct);
        if (game is null) return Result<GameResponse>.Failure("Jogo não encontrado.", ErrorType.NotFound);

        try
        {
            game.Update(request.Title, request.Description, request.Genre, request.Price, request.ReleaseDate);
            await _gameRepository.UpdateAsync(game, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Jogo {GameId} atualizado.", game.Id);

            var response = new GameResponse(
                game.Id, game.Title, game.Description, game.Genre,
                game.Price, game.GetCurrentPrice(DateTime.UtcNow),
                game.ReleaseDate, game.IsActive);
            return Result<GameResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<GameResponse>.Failure(ex.Message, ErrorType.Validation);
        }
    }
}
