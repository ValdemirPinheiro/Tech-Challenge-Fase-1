using FCG.API.Common;
using FCG.Application.Abstractions;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Games;
using FCG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/games")]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly CreateGameUseCase _createGame;
    private readonly UpdateGameUseCase _updateGame;
    private readonly DeleteGameUseCase _deleteGame;
    private readonly ListGamesUseCase _listGames;
    private readonly AcquireGameUseCase _acquireGame;
    private readonly ICurrentUserService _currentUser;

    public GamesController(
        CreateGameUseCase createGame,
        UpdateGameUseCase updateGame,
        DeleteGameUseCase deleteGame,
        ListGamesUseCase listGames,
        AcquireGameUseCase acquireGame,
        ICurrentUserService currentUser)
    {
        _createGame = createGame;
        _updateGame = updateGame;
        _deleteGame = deleteGame;
        _listGames = listGames;
        _acquireGame = acquireGame;
        _currentUser = currentUser;
    }

    /// <summary>Lista todos os jogos ativos do catálogo (público).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<GameResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _listGames.ExecuteAsync(ct)).ToActionResult();

    /// <summary>Cria um jogo (Admin).</summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken ct)
        => (await _createGame.ExecuteAsync(request, ct)).ToCreatedActionResult();

    /// <summary>Atualiza um jogo (Admin).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
        => (await _updateGame.ExecuteAsync(id, request, ct)).ToActionResult();

    /// <summary>Desativa um jogo (Admin).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await _deleteGame.ExecuteAsync(id, ct)).ToActionResult();

    /// <summary>Adquire um jogo (autenticado).</summary>
    [HttpPost("{id:guid}/acquire")]
    [Authorize]
    [ProducesResponseType(typeof(AcquireGameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Acquire(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Usuário não autenticado.");
        var result = await _acquireGame.ExecuteAsync(userId, id, ct);
        return result.ToCreatedActionResult();
    }
}
