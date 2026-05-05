using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Exceptions;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using FCG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Auth;

public class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserUseCase> _logger;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserUseCase> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> ExecuteAsync(
        RegisterUserRequest request,
        UserRole role = UserRole.User,
        CancellationToken ct = default)
    {
        try
        {
            var email = Email.Create(request.Email);
            var password = Password.Create(request.Password);

            if (await _userRepository.EmailExistsAsync(email.Value, ct))
            {
                _logger.LogWarning("Tentativa de cadastro com e-mail já existente: {Email}", email.Value);
                return Result<UserResponse>.Failure("Já existe um usuário com este e-mail.", ErrorType.Conflict);
            }

            var passwordHash = _passwordHasher.Hash(password.PlainText);
            var user = User.Create(request.Name, email, passwordHash, role);

            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Usuário cadastrado com sucesso: {UserId} ({Email})", user.Id, email.Value);

            var response = new UserResponse(user.Id, user.Name, user.Email.Value, user.Role.ToString(), user.CreatedAt);
            return Result<UserResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            _logger.LogInformation("Falha de validação ao cadastrar usuário: {Message}", ex.Message);
            return Result<UserResponse>.Failure(ex.Message, ErrorType.Validation);
        }
    }
}
