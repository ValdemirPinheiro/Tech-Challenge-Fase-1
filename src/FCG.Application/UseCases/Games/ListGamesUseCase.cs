using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Repositories;

namespace FCG.Application.UseCases.Games;

public class ListGamesUseCase
{
    private readonly IGameRepository _gameRepository;

    public ListGamesUseCase(IGameRepository gameRepository) => _gameRepository = gameRepository;

    public async Task<Result<IReadOnlyList<GameResponse>>> ExecuteAsync(CancellationToken ct = default)
    {
        var games = await _gameRepository.ListAsync(ct);
        var now = DateTime.UtcNow;
        var response = games
            .Where(g => g.IsActive)
            .Select(g => new GameResponse(
                g.Id, g.Title, g.Description, g.Genre, g.Price,
                g.GetCurrentPrice(now), g.ReleaseDate, g.IsActive))
            .ToList();
        return Result<IReadOnlyList<GameResponse>>.Success(response);
    }
}
