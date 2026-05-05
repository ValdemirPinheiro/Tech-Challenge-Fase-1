using FCG.Domain.Entities;

namespace FCG.Domain.Repositories;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> TitleExistsAsync(string title, CancellationToken ct = default);
    Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct = default);

    /// <summary>Retorna IQueryable para uso em GraphQL (HotChocolate).</summary>
    IQueryable<Game> Query();

    Task AddAsync(Game game, CancellationToken ct = default);
    Task UpdateAsync(Game game, CancellationToken ct = default);
    Task DeleteAsync(Game game, CancellationToken ct = default);
}
