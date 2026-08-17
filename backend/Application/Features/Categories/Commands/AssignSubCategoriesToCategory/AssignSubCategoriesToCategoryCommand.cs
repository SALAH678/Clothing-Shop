using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.AssignSubCategoriesToCategory;

public sealed record AssignSubCategoriesToCategoryCommand(
    Guid CategoryId,
    List<Guid> SubCategoryIds
) : IRequest<Result<Success>>;
