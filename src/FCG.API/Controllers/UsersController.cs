using FCG.API.Common;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Auth;
using FCG.Application.UseCases.Users;
using FCG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly ListUsersUseCase _listUsersUseCase;
    private readonly UpdateUserRoleUseCase _updateUserRoleUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;
    private readonly RegisterUserUseCase _registerUserUseCase;

    public UsersController(
        ListUsersUseCase listUsersUseCase,
        UpdateUserRoleUseCase updateUserRoleUseCase,
        DeleteUserUseCase deleteUserUseCase,
        RegisterUserUseCase registerUserUseCase)
    {
        _listUsersUseCase = listUsersUseCase;
        _updateUserRoleUseCase = updateUserRoleUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _registerUserUseCase = registerUserUseCase;
    }

    /// <summary>Lista todos os usuários (Admin).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _listUsersUseCase.ExecuteAsync(ct)).ToActionResult();

    /// <summary>Cria um administrador (Admin).</summary>
    [HttpPost("admin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAdmin([FromBody] RegisterUserRequest request, CancellationToken ct)
        => (await _registerUserUseCase.ExecuteAsync(request, UserRole.Admin, ct)).ToCreatedActionResult();

    /// <summary>Altera a role de um usuário (Admin).</summary>
    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
        => (await _updateUserRoleUseCase.ExecuteAsync(id, request, ct)).ToActionResult();

    /// <summary>Remove um usuário (Admin).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await _deleteUserUseCase.ExecuteAsync(id, ct)).ToActionResult();
}
