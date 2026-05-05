using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Repositories;

namespace FCG.Application.UseCases.Library;

public class GetUserLibraryUseCase
{
    private readonly ILibraryRepository _libraryRepository;

    public GetUserLibraryUseCase(ILibraryRepository libraryRepository)
        => _libraryRepository = libraryRepository;

    public async Task<Result<IReadOnlyList<LibraryItemResponse>>> ExecuteAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _libraryRepository.ListByUserAsync(userId, ct);
        var response = items
            .Select(i => new LibraryItemResponse(i.Id, i.GameId, i.GameTitle, i.PricePaid, i.AcquiredAt))
            .ToList();
        return Result<IReadOnlyList<LibraryItemResponse>>.Success(response);
    }
}
