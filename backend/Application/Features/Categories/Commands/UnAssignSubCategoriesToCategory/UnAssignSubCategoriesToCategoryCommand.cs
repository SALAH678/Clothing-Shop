using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.UnAssignSubCategoriesToCategory;

public sealed record UnAssignSubCategoriesToCategoryCommand(
    Guid CategoryId,
    List<Guid> SubCategoryIds
) : IRequest<Result<Success>>;
