using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _db;

    public GameRepository(ApplicationDbContext db) => _db = db;

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Games.Include(g => g.Promotions).FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<bool> TitleExistsAsync(string title, CancellationToken ct = default)
        => _db.Games.AnyAsync(g => g.Title == title.Trim(), ct);

    public async Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct = default)
        => await _db.Games
            .Include(g => g.Promotions)
            .AsNoTracking()
            .OrderBy(g => g.Title)
            .ToListAsync(ct);

    public IQueryable<Game> Query()
        => _db.Games.Include(g => g.Promotions).AsNoTracking();

    public async Task AddAsync(Game game, CancellationToken ct = default)
        => await _db.Games.AddAsync(game, ct);

    public Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        _db.Games.Update(game);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Game game, CancellationToken ct = default)
    {
        _db.Games.Remove(game);
        return Task.CompletedTask;
    }
}
