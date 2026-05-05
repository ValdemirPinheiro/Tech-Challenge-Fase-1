namespace FCG.Application.DTOs;

public record LibraryItemResponse(
    Guid LibraryItemId,
    Guid GameId,
    string GameTitle,
    decimal PricePaid,
    DateTime AcquiredAt);
