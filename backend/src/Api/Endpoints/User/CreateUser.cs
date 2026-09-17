using Api.Extensions;
using Application.Features.Users.Command.CreateUser;
using Application.Features.Users.Dtos;
using FastEndpoints;
using MediatR;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.User;

public class CreateUser(IMediator mediator) : Endpoint<CreateUserCommand, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("");
        Group<UserGroup>();
        Roles("Admin");

        Summary(s =>
        {
            s.Summary = "Create a user";
            s.Description = "Creates a user account.";
            s.ExampleRequest = new CreateUserCommand(
                "John",
                "Doe",
                "0123456789",
                "john.doe@example.com",
                "Password123!",
                "Customer | Admin");
            s.Responses[201] = "User created successfully.";
            s.Responses[400] = "User payload is invalid.";
            s.Responses[401] = "Authentication is required.";
            s.Responses[403] = "Administrator access is required.";
            s.Responses[409] = "A user with the provided email already exists.";
            s.Responses[500] = "User creation failed.";
        });

        Description(x => x
            .Produces<UserDto>(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(401)
            .ProducesProblemDetails(403)
            .ProducesProblemDetails(409)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(CreateUserCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);

        return result.Match(
            onSuccess: value => Results.Created($"/api/users/{value.UserId}", value),
            onError: errors => errors.ToProblem());
    }
}
