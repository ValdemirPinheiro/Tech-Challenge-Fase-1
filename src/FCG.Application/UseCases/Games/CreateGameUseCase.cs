using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Entities;
using FCG.Domain.Exceptions;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Games;

public class CreateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateGameUseCase> _logger;

    public CreateGameUseCase(IGameRepository gameRepository, IUnitOfWork unitOfWork, ILogger<CreateGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GameResponse>> ExecuteAsync(CreateGameRequest request, CancellationToken ct = default)
    {
        try
        {
            if (await _gameRepository.TitleExistsAsync(request.Title, ct))
            {
                return Result<GameResponse>.Failure("Já existe um jogo com este título.", ErrorType.Conflict);
            }

            var game = Game.Create(request.Title, request.Description, request.Genre, request.Price, request.ReleaseDate);
            await _gameRepository.AddAsync(game, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Jogo criado: {GameId} ({Title})", game.Id, game.Title);

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
