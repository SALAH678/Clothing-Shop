using Application.Common.Errors;
using Application.Common.Interfaces;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.UnAssignSubCategoriesToCategory
{
    public class UnAssignSubCategoriesToCategoryCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UnAssignSubCategoriesToCategoryCommand, Result<Success>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<Success>> Handle(UnAssignSubCategoriesToCategoryCommand request, CancellationToken cancellationToken)
        {
            var parentCategory = await _unitOfWork.Categories.ExistsAsync(request.CategoryId, cancellationToken);

            if (!parentCategory)
                return Error.NotFound(
                    code: "Category_NotFound",
                    description: $"Parent Category with ID {request.CategoryId} was not found."
                );

            var subCategories = await _unitOfWork.Categories.GetByIdsAsync(request.SubCategoryIds, cancellationToken);

            var missingIds = request.SubCategoryIds.Except(subCategories.Select(c => c.Id)).ToList();

            if (missingIds.Count > 0)
                return ApplicationErrors.SubCategoryNotFound;

            if (request.SubCategoryIds.Contains(request.CategoryId))
                return ApplicationErrors.CircularReferenceDetected;

            foreach (var subCategory in subCategories)
            {
                var result = subCategory.RemoveParent();

                if (!result.IsSuccess)
                    return result.TopError;
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success;
            }
            catch
            {
                return ApplicationErrors.AssignFailed;
            }
        }
    }
}
