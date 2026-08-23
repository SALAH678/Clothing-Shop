using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;

namespace Api.Endpoints.Auth;
public class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure("api/auth", ep =>
        {
            ep.AllowAnonymous();
            ep.Options(x => x
                .WithVersionSet("ClothingStore")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .AutoTagOverride("Authentication")
                .WithSummary("Authentication endpoints")
                .WithDescription("Handles registration, login, token refresh, password reset, and account verification flows."));
        });
    }
}
