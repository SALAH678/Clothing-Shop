using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;

namespace Api.Endpoints.Product;
public class ProductGroup : Group
{
    public ProductGroup()
    {
        Configure("api/products", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStore")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .AutoTagOverride("Product")
                .WithSummary("Product endpoints")
                .WithDescription("Handles product operations such as retrieving product details, listing products, and managing product inventory."));
        });
    }
}
