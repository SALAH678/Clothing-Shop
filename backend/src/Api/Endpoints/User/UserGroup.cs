using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.User;
public class UserGroup : Group
{
    public UserGroup()
    {
        Configure("api/users", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStoreApi")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .WithTags("Users")
                .WithSummary("User endpoints")
                .WithDescription("Handles user operations such as retrieving user details, listing users, and managing user accounts."));
        });
    }
}
