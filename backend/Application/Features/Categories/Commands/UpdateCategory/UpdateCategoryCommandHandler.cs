using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper,
    IImageCleanupJob imageCleanupJob, ILogger<UpdateCategoryCommandHandler> logger, IUser user)
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ILogger<UpdateCategoryCommandHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IMapper _mapper = mapper;
    private readonly IImageCleanupJob _imageCleanupJob = imageCleanupJob;

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update category requested for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
            request.categoryId, _user.Email, _user.UserId);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.categoryId, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Update category failed: category not found for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.categoryId, _user.Email, _user.UserId);
            return Error.NotFound(
                code: "Category_NotFound",
                description: $"Category with ID {request.categoryId} was not found."
            );
        }
        
        var imageUrl = category.ImageUrl;

        if(request.NewImageContent is not null && !string.IsNullOrEmpty(request.ImageFileName))
        {
            try
            {
                imageUrl = await _imageService.UpdateAsync(category.ImageUrl, request.NewImageContent,
                request.ImageFileName, ImageFolders.Categories , cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update category failed: unable to update image for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                    request.categoryId, _user.Email, _user.UserId);
                return ApplicationErrors.CategoryUpdateFailed;
            }
        }

        string categoryName = request.CategoryName ?? category.CategoryName;//if request.CategoryName not null use it and if null use category.CategoryName

        var updatedCategory = category.Update(categoryName, imageUrl);

        if (!updatedCategory.IsSuccess)
        {
            _logger.LogWarning("Update category failed for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.categoryId, _user.Email, _user.UserId);
            return updatedCategory.TopError;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category updated successfully for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.categoryId, _user.Email, _user.UserId);

            return _mapper.Map<CategoryDto>(category); // because updatedCategory is a updated type and when execute 
            // category.update and category is a reference type bz is a class type so is modified directly in memory
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl != category.ImageUrl)
            {
                try
                {
                    await _imageService.DeleteAsync(imageUrl, cancellationToken);
                }
                catch
                {
                    await _imageCleanupJob.ScheduleAsync([imageUrl], cancellationToken);
                }
            }

            _logger.LogError(ex, "Update category failed: unable to save changes for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.categoryId, _user.Email, _user.UserId);
            return ApplicationErrors.CategoryUpdateFailed;
        }
    }
}
