using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Repositories;

namespace FCG.Application.UseCases.Users;

public class ListUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public ListUsersUseCase(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<Result<IReadOnlyList<UserResponse>>> ExecuteAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.ListAsync(ct);
        var response = users
            .Select(u => new UserResponse(u.Id, u.Name, u.Email.Value, u.Role.ToString(), u.CreatedAt))
            .ToList();
        return Result<IReadOnlyList<UserResponse>>.Success(response);
    }
}
