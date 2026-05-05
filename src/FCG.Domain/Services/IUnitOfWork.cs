namespace FCG.Domain.Services;

/// <summary>
/// Abstração de Unit of Work para persistir mudanças do agregado relacional (EF Core).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
