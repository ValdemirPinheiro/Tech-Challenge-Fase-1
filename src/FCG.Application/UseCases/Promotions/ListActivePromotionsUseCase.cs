using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Repositories;

namespace FCG.Application.UseCases.Promotions;

public class ListActivePromotionsUseCase
{
    private readonly IPromotionRepository _promotionRepository;

    public ListActivePromotionsUseCase(IPromotionRepository promotionRepository)
        => _promotionRepository = promotionRepository;

    public async Task<Result<IReadOnlyList<PromotionResponse>>> ExecuteAsync(CancellationToken ct = default)
    {
        var promotions = await _promotionRepository.ListActiveAsync(DateTime.UtcNow, ct);
        var response = promotions
            .Select(p => new PromotionResponse(p.Id, p.GameId, p.DiscountPercentage, p.StartsAt, p.EndsAt, p.IsActive))
            .ToList();
        return Result<IReadOnlyList<PromotionResponse>>.Success(response);
    }
}
