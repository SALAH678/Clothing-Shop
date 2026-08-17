using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper)
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.categoryId, cancellationToken);

        if (category is null)
        {
            return Error.NotFound(
                code: "Category_NotFound",
                description: $"Category with ID {request.categoryId} was not found."
            );
        }
        
        var imageUrl = category.ImageUrl;

        if(request.NewImageContent is not null && !string.IsNullOrEmpty(request.ImageFileName))
        {
            var updateImageResult = await _imageService.UpdateAsync(category.ImageUrl, request.NewImageContent,
                request.ImageFileName, ImageFolders.Categories , cancellationToken);

            if (!updateImageResult.IsSuccess)
                return updateImageResult.TopError;

            imageUrl = updateImageResult.Value;
        }

        string categoryName = request.CategoryName ?? category.CategoryName;//if request.CategoryName not null use it and if null use category.CategoryName

        var updatedCategory = category.Update(categoryName, imageUrl);

        if (!updatedCategory.IsSuccess)
            return updatedCategory.TopError;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<CategoryDto>(updatedCategory.Value);
        }
        catch
        {
            if (imageUrl != category.ImageUrl)
                _ = await _imageService.DeleteAsync(imageUrl, cancellationToken);

            return ApplicationErrors.CategoryUpdateFailed;
        }
    }
}
