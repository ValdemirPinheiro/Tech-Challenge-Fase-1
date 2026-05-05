using FCG.Domain.Entities;

namespace FCG.Domain.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> ListActiveAsync(DateTime atUtc, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> ListByGameAsync(Guid gameId, CancellationToken ct = default);
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    Task UpdateAsync(Promotion promotion, CancellationToken ct = default);
}
