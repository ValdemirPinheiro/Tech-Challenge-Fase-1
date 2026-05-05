using FCG.Domain.Exceptions;

namespace FCG.Domain.Entities;

/// <summary>
/// Item da biblioteca pessoal de um usuário — registro de aquisição de um jogo.
/// Persistido no MongoDB (modelagem orientada a documento).
/// </summary>
public class LibraryItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public string GameTitle { get; private set; } = string.Empty;
    public decimal PricePaid { get; private set; }
    public DateTime AcquiredAt { get; private set; }

    private LibraryItem() { }

    private LibraryItem(Guid id, Guid userId, Guid gameId, string gameTitle, decimal pricePaid)
    {
        Id = id;
        UserId = userId;
        GameId = gameId;
        GameTitle = gameTitle;
        PricePaid = pricePaid;
        AcquiredAt = DateTime.UtcNow;
    }

    public static LibraryItem Create(Guid userId, Guid gameId, string gameTitle, decimal pricePaid)
    {
        DomainException.ThrowIf(userId == Guid.Empty, "UserId é obrigatório.");
        DomainException.ThrowIf(gameId == Guid.Empty, "GameId é obrigatório.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(gameTitle), "O título do jogo é obrigatório.");
        DomainException.ThrowIf(pricePaid < 0, "O preço pago não pode ser negativo.");

        return new LibraryItem(Guid.NewGuid(), userId, gameId, gameTitle.Trim(), pricePaid);
    }
}
