using System.Net;
using System.Net.Http.Json;
using Ecommerce.SharedLibrary.Responses;
using Microsoft.Extensions.Logging;
using OrderApi.Application.Common.Interfaces;

namespace OrderApi.Infrastructure.Clients;

public sealed class ProductCatalogClient(HttpClient httpClient, ILogger<ProductCatalogClient> logger) : IProductCatalogClient
{
    public async Task<ProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/v1/products/{productId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductSnapshot>>(cancellationToken: cancellationToken);

        if (apiResponse is null || !apiResponse.Success || apiResponse.Data is null)
        {
            logger.LogWarning("Product API returned an unsuccessful response for product {ProductId}", productId);
            return null;
        }

        return apiResponse.Data;
    }
}