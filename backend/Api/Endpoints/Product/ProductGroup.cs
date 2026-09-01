using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.Product;
public class ProductGroup : Group
{
    public ProductGroup()
    {
        Configure("api/products", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStoreApi")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .WithTags("Products")
                .WithSummary("Product endpoints")
                .WithDescription("Handles product operations such as retrieving product details, listing products, and managing product inventory."));
        });
    }
}
