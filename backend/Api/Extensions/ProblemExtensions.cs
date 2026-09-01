using Domain.Common.Results;
using Domain.Common.Results.Enum;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Extensions;

public static class ProblemExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        if (errors.All(error => error.Type == ErrorKind.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(
            statusCode: statusCode,
            title: error.Description,
            detail: error.Code 
        );
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        var errorsDict = errors.GroupBy(e => e.Code)
        .ToDictionary(
            g => g.Key,
            g => g.Select(e => e.Description).ToArray()
        );

        var problemDetails = new ValidationProblemDetails(errorsDict) // here ValidationProblemDetails is used to formats validation
                                                                      // failures into the standardized RFC 7807 Problem Details JSON schema.
        {
            Status = StatusCodes.Status400BadRequest
        };

        return Results.ValidationProblem(errorsDict, statusCode: StatusCodes.Status400BadRequest);
    }
}
