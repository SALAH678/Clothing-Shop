using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;

namespace Api.Endpoints.User;
public class UserGroup : Group
{
    public UserGroup()
    {
        Configure("api/users", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStore")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .AutoTagOverride("User")
                .WithSummary("User endpoints")
                .WithDescription("Handles user operations such as retrieving user details, listing users, and managing user accounts."));
        });
    }
}
