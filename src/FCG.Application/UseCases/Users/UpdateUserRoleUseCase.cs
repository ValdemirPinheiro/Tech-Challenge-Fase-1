using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Enums;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Users;

public class UpdateUserRoleUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserRoleUseCase> _logger;

    public UpdateUserRoleUseCase(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserRoleUseCase> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> ExecuteAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return Result<UserResponse>.Failure("Usuário não encontrado.", ErrorType.NotFound);
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
        {
            return Result<UserResponse>.Failure("Role inválida. Use 'User' ou 'Admin'.", ErrorType.Validation);
        }

        if (newRole == UserRole.Admin) user.PromoteToAdmin();
        else user.DemoteToUser();

        await _userRepository.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Role do usuário {UserId} alterada para {Role}", user.Id, newRole);

        var response = new UserResponse(user.Id, user.Name, user.Email.Value, user.Role.ToString(), user.CreatedAt);
        return Result<UserResponse>.Success(response);
    }
}
