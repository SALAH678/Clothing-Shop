using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.Auth;
public class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure("api/auth", ep =>
        {
            ep.AllowAnonymous();
            ep.Options(x => x
                .WithVersionSet("ClothingStoreApi")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .WithTags("Authentication")
                .WithSummary("Authentication endpoints")
                .WithDescription("Handles registration, login, token refresh, password reset, and account verification flows."));
        });
    }
}
