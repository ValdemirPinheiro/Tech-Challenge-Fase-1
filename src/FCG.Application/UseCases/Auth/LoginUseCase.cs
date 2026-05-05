using FCG.Application.Abstractions;
using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<LoginUseCase> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("E-mail e senha são obrigatórios.", ErrorType.Validation);
        }

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null)
        {
            _logger.LogWarning("Login falhou — usuário não encontrado: {Email}", request.Email);
            return Result<AuthResponse>.Failure("Credenciais inválidas.", ErrorType.Unauthorized);
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login falhou — senha incorreta: {Email}", request.Email);
            return Result<AuthResponse>.Failure("Credenciais inválidas.", ErrorType.Unauthorized);
        }

        var token = _jwtTokenService.GenerateToken(user);
        _logger.LogInformation("Login realizado: {UserId}", user.Id);

        var response = new AuthResponse(
            token.Token,
            token.ExpiresAt,
            new UserSummary(user.Id, user.Name, user.Email.Value, user.Role.ToString()));

        return Result<AuthResponse>.Success(response);
    }
}
