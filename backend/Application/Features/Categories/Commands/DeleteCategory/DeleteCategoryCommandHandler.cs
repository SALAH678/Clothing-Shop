using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, 
    IImageService imageService) : IRequestHandler<DeleteCategoryCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;

    public async Task<Result<Deleted>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.categoryId, cancellationToken);

        if (category is null)
        {
            return Error.NotFound(
                code: "Category_NotFound",
                description: $"Category with ID {request.categoryId} was not found."
            );
        }

        _unitOfWork.Categories.Delete(category);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.CategoryDeletFailed;
        }

        if(!string.IsNullOrEmpty(category.ImageUrl))
            _ = await _imageService.DeleteAsync(category.ImageUrl, cancellationToken);

        //use background job to delete the image from the storage, if delete image fails

        return Result.Deleted;
    }
}
