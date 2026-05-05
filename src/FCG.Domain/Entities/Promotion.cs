using FCG.Domain.Exceptions;

namespace FCG.Domain.Entities;

/// <summary>
/// Promoção aplicada a um jogo. Faz parte do agregado Game.
/// </summary>
public class Promotion
{
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public Game? Game { get; private set; }

    private Promotion() { }

    private Promotion(Guid id, Guid gameId, decimal discountPercentage, DateTime startsAt, DateTime endsAt)
    {
        Id = id;
        GameId = gameId;
        DiscountPercentage = discountPercentage;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Promotion Create(Guid gameId, decimal discountPercentage, DateTime startsAt, DateTime endsAt)
    {
        DomainException.ThrowIf(gameId == Guid.Empty, "GameId é obrigatório.");
        DomainException.ThrowIf(discountPercentage <= 0, "O desconto deve ser maior que zero.");
        DomainException.ThrowIf(discountPercentage > 90, "O desconto máximo permitido é 90%.");
        DomainException.ThrowIf(endsAt <= startsAt, "A data final deve ser posterior à inicial.");

        return new Promotion(Guid.NewGuid(), gameId, discountPercentage, startsAt, endsAt);
    }

    public bool IsValidAt(DateTime atUtc) =>
        IsActive && atUtc >= StartsAt && atUtc <= EndsAt;

    public void Deactivate()
    {
        IsActive = false;
    }
}
