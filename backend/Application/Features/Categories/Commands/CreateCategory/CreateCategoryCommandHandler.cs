using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Categories;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService,
    IMapper mapper) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        Result<string>? imageResult = null;

        if(request.ImageContent is not null && request.ImageFileName is not null)
        {
            imageResult = await _imageService.SaveAsync(request.ImageContent, request.ImageFileName, ImageFolders.Categories, cancellationToken);

            if (!imageResult.IsSuccess)
                return imageResult.TopError;
        }

        var createCategoryResult = Category.Create(request.CategoryName.Trim(), imageResult?.Value);

        if (createCategoryResult.IsError && imageResult is not null)
        {
            _ = await _imageService.DeleteAsync(imageResult.Value, cancellationToken);
            return createCategoryResult.Errors;
        }

        _unitOfWork.Categories.Create(createCategoryResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if(imageResult is not null)
                _ = await _imageService.DeleteAsync(imageResult.Value, cancellationToken);

            return ApplicationErrors.CategoryCreationFailed;
        }

        return _mapper.Map<CategoryDto>(createCategoryResult.Value);
    }
}
