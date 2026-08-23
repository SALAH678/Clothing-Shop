using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;

namespace Api.Endpoints.Cart;
public class CartGroup : Group
{
    public CartGroup()
    {
        Configure("api/carts", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStore")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .AutoTagOverride("Cart")
                .WithSummary("Cart endpoints")
                .WithDescription("Handles cart operations such as adding items, removing items, and viewing the cart."));
        });
    }
}
