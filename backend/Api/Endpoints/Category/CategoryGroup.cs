using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.Category;

public class CategoryGroup : Group
{
    public CategoryGroup()
    {
        Configure("api/categories", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStoreApi")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .WithTags("Categories")
                .WithSummary("Category endpoints")
                .WithDescription("Handles category operations such as retrieving categories, adding new categories, and updating existing categories."));
        });
    }
}
