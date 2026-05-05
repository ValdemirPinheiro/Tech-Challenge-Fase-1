namespace FCG.Application.DTOs;

public record CreatePromotionRequest(
    Guid GameId,
    decimal DiscountPercentage,
    DateTime StartsAt,
    DateTime EndsAt);

public record PromotionResponse(
    Guid Id,
    Guid GameId,
    decimal DiscountPercentage,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsActive);
