using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, 
    IImageService imageService, IImageCleanupJob imageCleanupJob, ILogger<DeleteCategoryCommandHandler> logger,
    IUser user) : IRequestHandler<DeleteCategoryCommand, Result<Deleted>>
{
    private readonly ILogger<DeleteCategoryCommandHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IImageCleanupJob _imageCleanupJob = imageCleanupJob;

    public async Task<Result<Deleted>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete category requested for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
            request.categoryId, _user.Email, _user.UserId);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.categoryId, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Delete category failed: category not found for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.categoryId, _user.Email, _user.UserId);
            return Error.NotFound(
                code: "Category_NotFound",
                description: $"Category with ID {request.categoryId} was not found."
            );
        }

        _unitOfWork.Categories.Delete(category);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            try
            {
                await _imageService.DeleteAsync(category.ImageUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                // Database deletion has already succeeded; image cleanup can be retried separately.
                _logger.LogWarning(ex, "Delete category: image deletion failed, scheduled cleanup job for ImageUrl: {ImageUrl}, CategoryId: {CategoryId}",
                    category.ImageUrl, category.Id);
                await _imageCleanupJob.ScheduleAsync([category.ImageUrl], cancellationToken);
            }
        }

        //use background job to delete the image from the storage, if delete image fails

        _logger.LogInformation("Category deleted successfully for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
            request.categoryId, _user.Email, _user.UserId);

        return Result.Deleted;
    }
}
