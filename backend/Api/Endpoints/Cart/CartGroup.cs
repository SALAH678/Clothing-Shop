using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.Cart;
public class CartGroup : Group
{
    public CartGroup()
    {
        Configure("api/carts", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStoreApi")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .WithTags("Carts")
                .WithSummary("Cart endpoints")
                .WithDescription("Handles cart operations such as adding items, removing items, and viewing the cart."));
        });
    }
}
