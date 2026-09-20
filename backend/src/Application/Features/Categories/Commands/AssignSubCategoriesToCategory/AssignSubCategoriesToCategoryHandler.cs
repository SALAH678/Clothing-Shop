using Application.Common.Errors;
using Application.Common.Interfaces;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Categories.Commands.AssignSubCategoriesToCategory
{
    public class AssignSubCategoriesToCategoryHandler(IUnitOfWork unitOfWork, ILogger<AssignSubCategoriesToCategoryHandler> logger, IUser user) : IRequestHandler<AssignSubCategoriesToCategoryCommand, Result<Success>>
    {
    private readonly ILogger<AssignSubCategoriesToCategoryHandler> _logger = logger;
    private readonly IUser _user = user;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<Success>> Handle(AssignSubCategoriesToCategoryCommand request, CancellationToken cancellationToken)
        {
        _logger.LogInformation("Assign subcategories requested for CategoryId: {CategoryId}, SubCategoryIds: {SubCategoryIds}, Email: {Email}, UserId: {UserId}",
            request.CategoryId, request.SubCategoryIds, _user.Email, _user.UserId);

            var parentCategory = await _unitOfWork.Categories.ExistsAsync(request.CategoryId, cancellationToken);

            if (!parentCategory)
            {
                _logger.LogWarning("Assign subcategories failed: parent category not found for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                    request.CategoryId, _user.Email, _user.UserId);
                return Error.NotFound(
                    code: "Category_NotFound",
                    description: $"Parent Category with ID {request.CategoryId} was not found."
                );
            }

           var subCategories = await _unitOfWork.Categories.GetByIdsAsync(request.SubCategoryIds, cancellationToken);

           var missingIds = request.SubCategoryIds.Except(subCategories.Select(c => c.Id)).ToList();

            if (missingIds.Count > 0)
            {
                _logger.LogWarning("Assign subcategories failed: subcategories not found for MissingIds: {MissingIds}, CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                    missingIds, request.CategoryId, _user.Email, _user.UserId);
                return ApplicationErrors.SubCategoryNotFound;
            }

            if (request.SubCategoryIds.Contains(request.CategoryId))
            {
                _logger.LogWarning("Assign subcategories failed: circular reference detected for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                    request.CategoryId, _user.Email, _user.UserId);
                return ApplicationErrors.CircularReferenceDetected;
            }

            foreach (var subCategory in subCategories)
            {
                var result = subCategory.SetParent(request.CategoryId);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Assign subcategories failed for SubCategoryId: {SubCategoryId}, CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                        subCategory.Id, request.CategoryId, _user.Email, _user.UserId);
                    return result.TopError;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subcategories assigned successfully for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.CategoryId, _user.Email, _user.UserId);

            return Result.Success;
        }
    }
}
