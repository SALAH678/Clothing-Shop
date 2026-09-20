using Application.Common.Models;
using Application.Features.Purchases.Dtos;
using Application.Features.Purchases.Queries.GetPurchases;
using FastEndpoints;
using MediatR;

namespace Api.Endpoints.Purchase;

public class GetAllPurchases(IMediator mediator) : Endpoint<GetPurchasesQuery, PaginatedList<PurchaseDto>>
{
    public override void Configure()
    {
        Get("");
        Group<PurchaseGroup>();
        Roles("Admin");
        Options(x => x.RequireRateLimiting("admin-read"));

        Summary(s =>
        {
            s.Summary = "Get all purchases";
            s.Description =
                "Returns a paginated list of all purchases across all customers. \n" +
                "Each purchase includes customer details, delivery address, purchased items " +
                "(with product name, quantity, and unit price), total amount, and payment status. \n" +
                "Results are ordered by creation date and paginated using PageNumber and PageSize. \n" +
                "This endpoint is restricted to administrators.";

            s.Responses[200] = "Purchases retrieved successfully.";
            s.Responses[400] = "The pagination parameters are invalid.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[403] = "The authenticated user does not have permission to access this resource.";
        });

        Description(x => x
            .Produces<PaginatedList<PurchaseDto>>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(403));
    }

    public override async Task HandleAsync(GetPurchasesQuery req, CancellationToken ct)
    {
        var result = await mediator.Send(req, ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}
