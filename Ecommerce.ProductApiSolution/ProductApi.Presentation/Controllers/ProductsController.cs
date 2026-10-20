// File: ProductApi.Presentation/Controllers/ProductsController.cs
using Ecommerce.SharedLibrary.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.Features.Products.Commands.CreateProduct;
using ProductApi.Application.Features.Products.Commands.DeleteProduct;
using ProductApi.Application.Features.Products.Commands.UpdateProduct;
using ProductApi.Application.Features.Products.DTOs;
using ProductApi.Application.Features.Products.Queries.GetProductById;
using ProductApi.Application.Features.Products.Queries.GetProducts;
using ProductApi.Presentation.Models;

namespace ProductApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CatalogWrite")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await sender.Send(command, cancellationToken);

        var response = ApiResponse<ProductDto>.Ok(product, "Product created successfully.", HttpContext.TraceIdentifier);

        return CreatedAtAction(
            nameof(GetById), 
            new { id = product.Id },
            response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await sender.Send(
             new GetProductByIdQuery(id),
             cancellationToken);

        return Ok(ApiResponse<ProductDto>.Ok(
            product, 
            "Product retrieved successfully.", 
            HttpContext.TraceIdentifier));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(
         search,
         category,
         isActive,
         pageNumber,
         pageSize,
         sortBy,
         descending);

        var result = await sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(
            result, 
            "Products retrieved successfully.", 
            HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CatalogWrite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Category,
            request.Price,
            request.StockQuantity,
            request.Currency,
            request.IsActive);

        var product = await sender.Send(command, cancellationToken);

        return Ok(ApiResponse<ProductDto>.Ok(
              product,
              "Product updated successfully.",
              HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CatalogWrite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteProductCommand(id),
            cancellationToken);

        return NoContent();
    }
}