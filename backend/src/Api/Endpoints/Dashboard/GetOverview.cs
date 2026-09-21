using Application.Features.Dashboard.Dtos;
using Application.Features.Dashboard.Query.GetOverview;
using FastEndpoints;
using MediatR;

namespace Api.Endpoints.Dashboard;

public class GetOverview(IMediator mediator) : EndpointWithoutRequest<OverviewDto>
{
    public override void Configure()
    {
        Get("overview");
        Group<DashboardGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("authenticated-read"));

        Summary(s =>
        {
            s.Summary = "Get dashboard overview";
            s.Description =
                "Returns aggregated statistics for the admin dashboard, including totals such as " +
                "revenue, orders, and customer counts. \n" +
                "This endpoint is restricted to administrators.";

            s.Responses[200] = "Overview data retrieved successfully.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[403] = "The authenticated user does not have permission to access this resource.";
        });

        Description(x => x
            .Produces<OverviewDto>(200)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(403));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GetOverviewQuery(), ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}
