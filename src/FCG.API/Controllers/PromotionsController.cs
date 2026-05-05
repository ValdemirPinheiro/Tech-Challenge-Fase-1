using FCG.API.Common;
using FCG.Application.DTOs;
using FCG.Application.UseCases.Promotions;
using FCG.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/promotions")]
[Produces("application/json")]
public class PromotionsController : ControllerBase
{
    private readonly CreatePromotionUseCase _createPromotion;
    private readonly ListActivePromotionsUseCase _listActivePromotions;

    public PromotionsController(
        CreatePromotionUseCase createPromotion,
        ListActivePromotionsUseCase listActivePromotions)
    {
        _createPromotion = createPromotion;
        _listActivePromotions = listActivePromotions;
    }

    /// <summary>Lista promoções ativas (público).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PromotionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListActive(CancellationToken ct)
        => (await _listActivePromotions.ExecuteAsync(ct)).ToActionResult();

    /// <summary>Cria uma promoção (Admin).</summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request, CancellationToken ct)
        => (await _createPromotion.ExecuteAsync(request, ct)).ToCreatedActionResult();
}
