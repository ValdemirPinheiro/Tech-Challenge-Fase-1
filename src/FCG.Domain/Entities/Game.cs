using FCG.Domain.Exceptions;

namespace FCG.Domain.Entities;

/// <summary>
/// Aggregate Root que representa um jogo no catálogo da FCG.
/// </summary>
public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Promotion> _promotions = new();
    public IReadOnlyCollection<Promotion> Promotions => _promotions.AsReadOnly();

    private Game() { }

    private Game(Guid id, string title, string description, string genre, decimal price, DateTime releaseDate)
    {
        Id = id;
        Title = title;
        Description = description;
        Genre = genre;
        Price = price;
        ReleaseDate = releaseDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Game Create(string title, string description, string genre, decimal price, DateTime releaseDate)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "O título do jogo é obrigatório.");
        DomainException.ThrowIf(title.Trim().Length < 2, "O título deve ter pelo menos 2 caracteres.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(genre), "O gênero do jogo é obrigatório.");
        DomainException.ThrowIf(price < 0, "O preço não pode ser negativo.");

        return new Game(Guid.NewGuid(), title.Trim(), description?.Trim() ?? string.Empty, genre.Trim(), price, releaseDate);
    }

    public void Update(string title, string description, string genre, decimal price, DateTime releaseDate)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "O título do jogo é obrigatório.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(genre), "O gênero do jogo é obrigatório.");
        DomainException.ThrowIf(price < 0, "O preço não pode ser negativo.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Genre = genre.Trim();
        Price = price;
        ReleaseDate = releaseDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcula o preço atual considerando promoções ativas para a data fornecida.
    /// </summary>
    public decimal GetCurrentPrice(DateTime atUtc)
    {
        var bestDiscount = _promotions
            .Where(p => p.IsValidAt(atUtc))
            .Select(p => p.DiscountPercentage)
            .DefaultIfEmpty(0)
            .Max();

        return Math.Round(Price * (1 - (bestDiscount / 100m)), 2);
    }
}
