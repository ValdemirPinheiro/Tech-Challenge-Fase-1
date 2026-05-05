using FCG.Application.Common;
using FCG.Application.DTOs;
using FCG.Domain.Entities;
using FCG.Domain.Exceptions;
using FCG.Domain.Repositories;
using FCG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.UseCases.Promotions;

public class CreatePromotionUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePromotionUseCase> _logger;

    public CreatePromotionUseCase(
        IGameRepository gameRepository,
        IPromotionRepository promotionRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePromotionUseCase> logger)
    {
        _gameRepository = gameRepository;
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PromotionResponse>> ExecuteAsync(CreatePromotionRequest request, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId, ct);
        if (game is null) return Result<PromotionResponse>.Failure("Jogo não encontrado.", ErrorType.NotFound);

        try
        {
            var promotion = Promotion.Create(request.GameId, request.DiscountPercentage, request.StartsAt, request.EndsAt);
            await _promotionRepository.AddAsync(promotion, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Promoção criada para o jogo {GameId} ({Discount}%)", game.Id, request.DiscountPercentage);

            var response = new PromotionResponse(
                promotion.Id, promotion.GameId, promotion.DiscountPercentage,
                promotion.StartsAt, promotion.EndsAt, promotion.IsActive);

            return Result<PromotionResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<PromotionResponse>.Failure(ex.Message, ErrorType.Validation);
        }
    }
}
