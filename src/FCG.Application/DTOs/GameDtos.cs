namespace FCG.Application.DTOs;

public record CreateGameRequest(
    string Title,
    string Description,
    string Genre,
    decimal Price,
    DateTime ReleaseDate);

public record UpdateGameRequest(
    string Title,
    string Description,
    string Genre,
    decimal Price,
    DateTime ReleaseDate);

public record GameResponse(
    Guid Id,
    string Title,
    string Description,
    string Genre,
    decimal Price,
    decimal CurrentPrice,
    DateTime ReleaseDate,
    bool IsActive);

public record AcquireGameResponse(
    Guid LibraryItemId,
    Guid GameId,
    string GameTitle,
    decimal PricePaid,
    DateTime AcquiredAt);
