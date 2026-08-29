using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Categories;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService,
    IMapper mapper, IImageCleanupJob imageCleanupJob, ILogger<CreateCategoryCommandHandler> logger,
    IUser user) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ILogger<CreateCategoryCommandHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IMapper _mapper = mapper;
    private readonly IImageCleanupJob _imageCleanupJob = imageCleanupJob;

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create category requested for CategoryName: {CategoryName}, Email: {Email}, UserId: {UserId}",
            request.CategoryName, _user.Email, _user.UserId);

        string? imageUrl = null;

        try
        {
            if (request.ImageContent is not null && request.ImageFileName is not null)
                imageUrl = await _imageService.SaveAsync(request.ImageContent, request.ImageFileName, ImageFolders.Categories, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create category failed: unable to save image for CategoryName: {CategoryName}, Email: {Email}, UserId: {UserId}",
                request.CategoryName, _user.Email, _user.UserId);
            return ApplicationErrors.CategoryCreationFailed;
        }

        var createCategoryResult = Category.Create(request.CategoryName.Trim(), imageUrl);

        if (createCategoryResult.IsError && imageUrl is not null)
        {
            await TryDeleteImageAsync(imageUrl, cancellationToken);

            _logger.LogWarning("Create category failed for CategoryName: {CategoryName}, Email: {Email}, UserId: {UserId}",
                request.CategoryName, _user.Email, _user.UserId);
            return createCategoryResult.Errors;
        }

        _unitOfWork.Categories.Create(createCategoryResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (imageUrl is not null)
                await TryDeleteImageAsync(imageUrl, cancellationToken);

            _logger.LogError(ex, "Create category failed: unable to save changes for CategoryName: {CategoryName}, Email: {Email}, UserId: {UserId}",
                request.CategoryName, _user.Email, _user.UserId);
            return ApplicationErrors.CategoryCreationFailed;
        }

        _logger.LogInformation("Category created successfully for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
            createCategoryResult.Value.Id, _user.Email, _user.UserId);

        return _mapper.Map<CategoryDto>(createCategoryResult.Value);
    }

    private async Task TryDeleteImageAsync(string imageUrl, CancellationToken cancellationToken)
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
}
