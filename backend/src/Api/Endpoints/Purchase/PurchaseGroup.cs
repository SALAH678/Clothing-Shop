using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.Purchase
{
    public class PurchaseGroup : Group
    {
        public PurchaseGroup()
        {
            Configure("api/purchases", ep =>
            {
                ep.Options(x => x
                    .WithVersionSet("ClothingStoreApi")
                    .MapToApiVersion(1.0));
                ep.Description(x => x
                    .WithTags("Purchase")
                    .WithSummary("Purchase endpoints")
                    .WithDescription("Handles purchase creation, payment processing, and purchase history retrieval."));
            });
        }
    }
}
