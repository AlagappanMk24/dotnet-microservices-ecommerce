// File: ProductApi.Presentation/Extensions/PresentationExtensions.cs
using Ecommerce.SharedLibrary.Security;

namespace ProductApi.Presentation.Extensions;

public static class PresentationExtensions
{
    public static IServiceCollection AddProductApiPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtBearerValidation(configuration);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CatalogWrite", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole("Admin", "CatalogManager", "Customer"));
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        //services.AddSwaggerGen();
        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}