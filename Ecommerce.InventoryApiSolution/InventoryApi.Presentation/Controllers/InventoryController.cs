using Ecommerce.SharedLibrary.Responses;
using InventoryApi.Application.Abstractions;
using InventoryApi.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/inventory")]
public sealed class InventoryController(IInventoryService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "InventoryWrite")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(
            nameof(GetByProductId),
            new { productId = result.ProductId },
            ApiResponse<InventoryDto>.Ok(result, "Inventory created.", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<InventoryDto>>.Ok(
            await service.GetAllAsync(ct),
            "Inventory retrieved.",
            HttpContext.TraceIdentifier));

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProductId(Guid productId, CancellationToken ct)
    {
        var result = await service.GetByProductIdAsync(productId, ct);

        return result is null
            ? NotFound(ApiResponse<InventoryDto>.Fail("Inventory not found.", HttpContext.TraceIdentifier))
            : Ok(ApiResponse<InventoryDto>.Ok(result, "Inventory retrieved.", HttpContext.TraceIdentifier));
    }

    [HttpPost("product/{productId:guid}/adjustments")]
    [Authorize(Policy = "InventoryWrite")]
    public async Task<IActionResult> AddStock(Guid productId, [FromBody] AdjustStockRequest request, CancellationToken ct) =>
        Ok(ApiResponse<InventoryDto>.Ok(
            await service.AddStockAsync(productId, request.Quantity, ct),
            "Stock adjusted.",
            HttpContext.TraceIdentifier));

    [HttpGet("orders/{orderId:guid}/reservations")]
    [Authorize]
    public async Task<IActionResult> GetReservations(Guid orderId, CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<ReservationDto>>.Ok(
            await service.GetReservationsAsync(orderId, ct),
            "Reservations retrieved.",
            HttpContext.TraceIdentifier));

    [HttpPost("orders/{orderId:guid}/release")]
    [Authorize(Policy = "InventoryWrite")]
    public async Task<IActionResult> Release(Guid orderId, CancellationToken ct)
    {
        await service.ReleaseOrderReservationsAsync(orderId, ct);
        return Ok(ApiResponse<object>.Ok(new { orderId }, "Reservations released.", HttpContext.TraceIdentifier));
    }

    [HttpPost("orders/{orderId:guid}/fulfill")]
    [Authorize(Policy = "InventoryWrite")]
    public async Task<IActionResult> Fulfill(Guid orderId, CancellationToken ct)
    {
        await service.FulfillOrderReservationsAsync(orderId, ct);
        return Ok(ApiResponse<object>.Ok(new { orderId }, "Reservations fulfilled.", HttpContext.TraceIdentifier));
    }
}