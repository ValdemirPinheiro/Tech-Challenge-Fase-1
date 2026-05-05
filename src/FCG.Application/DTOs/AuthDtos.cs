namespace FCG.Application.DTOs;

public record RegisterUserRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserSummary User);

public record UserSummary(Guid Id, string Name, string Email, string Role);
