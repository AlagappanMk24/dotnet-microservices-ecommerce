using Ecommerce.SharedLibrary.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Features.Orders.Commands.CreateOrder;
using OrderApi.Application.Features.Orders.DTOs;
using OrderApi.Application.Features.Orders.Queries.GetOrderById;
using OrderApi.Application.Features.Orders.Queries.GetOrders;

namespace OrderApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            ApiResponse<OrderDto>.Ok(order, "Order created successfully.", HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);

        return Ok(ApiResponse<OrderDto>.Ok(order, "Order retrieved successfully.", HttpContext.TraceIdentifier));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersQuery(pageNumber, pageSize);
        var result = await sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(
            result,
            "Orders retrieved successfully.",
            HttpContext.TraceIdentifier));
    }
}