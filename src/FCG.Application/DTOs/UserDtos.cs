namespace FCG.Application.DTOs;

public record UserResponse(Guid Id, string Name, string Email, string Role, DateTime CreatedAt);

public record UpdateUserRoleRequest(string Role);
