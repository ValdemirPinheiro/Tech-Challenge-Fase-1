using FCG.Domain.Entities;

namespace FCG.Domain.Repositories;

/// <summary>
/// Repositório da biblioteca de jogos do usuário (persistido em MongoDB).
/// </summary>
public interface ILibraryRepository
{
    Task<bool> UserOwnsGameAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task<IReadOnlyList<LibraryItem>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(LibraryItem item, CancellationToken ct = default);
}
