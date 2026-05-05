using FCG.Application.Common;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Games;

public class DeleteGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGameUseCase> _logger;

    public DeleteGameUseCase(IGameRepository gameRepository, IUnitOfWork unitOfWork, ILogger<DeleteGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(Guid gameId, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, ct);
        if (game is null) return Result.Failure("Jogo não encontrado.", ErrorType.NotFound);

        // Soft delete (deactivate) — preserva histórico em bibliotecas adquiridas.
        game.Deactivate();
        await _gameRepository.UpdateAsync(game, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Jogo {GameId} desativado.", gameId);
        return Result.Success();
    }
}
