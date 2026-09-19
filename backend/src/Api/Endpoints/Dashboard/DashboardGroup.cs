using FastEndpoints;
using FastEndpoints.AspVersioning;

namespace Api.Endpoints.Dashboard;

public class DashboardGroup : Group
{
    public DashboardGroup()
    {
        Configure("api/dashboard", ep =>
        {
            ep.Options(x => x
                .WithVersionSet("ClothingStoreApi")
                .MapToApiVersion(1.0));
            ep.Description(x => x
                .WithTags("Dashboard")
                .WithSummary("Dashboard endpoints")
                .WithDescription("Handles dashboard operations such as retrieving overview data."));
        });
    }
}
