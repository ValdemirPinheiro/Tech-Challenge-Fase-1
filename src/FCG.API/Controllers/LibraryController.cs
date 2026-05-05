using FCG.API.Common;
using FCG.Application.Abstractions;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
[Produces("application/json")]
public class LibraryController : ControllerBase
{
    private readonly GetUserLibraryUseCase _getLibrary;
    private readonly ICurrentUserService _currentUser;

    public LibraryController(GetUserLibraryUseCase getLibrary, ICurrentUserService currentUser)
    {
        _getLibrary = getLibrary;
        _currentUser = currentUser;
    }

    /// <summary>Retorna a biblioteca de jogos do usuário autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<LibraryItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLibrary(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Usuário não autenticado.");
        return (await _getLibrary.ExecuteAsync(userId, ct)).ToActionResult();
    }
}
