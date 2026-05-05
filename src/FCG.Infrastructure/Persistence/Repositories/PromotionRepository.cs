using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly ApplicationDbContext _db;

    public PromotionRepository(ApplicationDbContext db) => _db = db;

    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Promotions.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Promotion>> ListActiveAsync(DateTime atUtc, CancellationToken ct = default)
        => await _db.Promotions
            .AsNoTracking()
            .Where(p => p.IsActive && p.StartsAt <= atUtc && p.EndsAt >= atUtc)
            .OrderBy(p => p.EndsAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Promotion>> ListByGameAsync(Guid gameId, CancellationToken ct = default)
        => await _db.Promotions.AsNoTracking().Where(p => p.GameId == gameId).ToListAsync(ct);

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
        => await _db.Promotions.AddAsync(promotion, ct);

    public Task UpdateAsync(Promotion promotion, CancellationToken ct = default)
    {
        _db.Promotions.Update(promotion);
        return Task.CompletedTask;
    }
}
