using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Products;
using MediatR;
using Microsoft.Extensions.Logging;
namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper,
    IImageCleanupJob imageCleanupJob, ILogger<CreateProductCommandHandler> logger, IUser user)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly ILogger<CreateProductCommandHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IImageService _imageService = imageService;
    private readonly IMapper _mapper = mapper;
    private readonly IImageCleanupJob _imageCleanupJob = imageCleanupJob;

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create product requested for Name: {ProductName}, CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
            request.Name, request.CategoryId, _user.Email, _user.UserId);

        var createProductResult = Product.Create(request.Name, request.Description ?? string.Empty, request.BasePrice, request.Discount, request.CategoryId);

        if (createProductResult.IsError)
        {
            _logger.LogWarning("Create product failed for Name: {ProductName}, Email: {Email}, UserId: {UserId}",
                request.Name, _user.Email, _user.UserId);
            return createProductResult.Errors;
        }

        if (request.Variants is not null)
        {
            foreach (var variant in request.Variants)
            {
                var variantResult = createProductResult.Value.AddVariant(variant.Size, variant.Color, variant.StockQuantity);
                if (variantResult.IsError)
                {
                    _logger.LogWarning("Create product failed: unable to add variant for ProductName: {ProductName}, Size: {Size}, Color: {Color}, Email: {Email}, UserId: {UserId}",
                        request.Name, variant.Size, variant.Color, _user.Email, _user.UserId);
                    return variantResult.Errors;
                }
            }
        }

        var savedImageUrls = new List<string>();

        if (request.Images is not null)
        {
            foreach (var image in request.Images)
            {
                string imageUrl;
                try
                {
                    imageUrl = await _imageService.SaveAsync(image.ImageContent, image.fileName, ImageFolders.Products, cancellationToken);
                }
                catch (Exception ex)
                {
                    await _imageCleanupJob.ScheduleAsync(savedImageUrls, cancellationToken);
                    _logger.LogError(ex, "Create product failed: unable to save image for ProductName: {ProductName}, Email: {Email}, UserId: {UserId}",
                        request.Name, _user.Email, _user.UserId);
                    return Error.Failure(
                        code: "Product_Creation_Failed",
                        description: "Product creation failed.");
                }

                savedImageUrls.Add(imageUrl);

                var addImageResult = createProductResult.Value.AddImage(
                    imageUrl,
                    isMain: image.IsMain);

                if (addImageResult.IsError)
                {
                    await _imageCleanupJob.ScheduleAsync(savedImageUrls, cancellationToken);
                    _logger.LogWarning("Create product failed: unable to add image for ImageUrl: {ImageUrl}, ProductName: {ProductName}, Email: {Email}, UserId: {UserId}",
                        imageUrl, request.Name, _user.Email, _user.UserId);
                    return addImageResult.Errors;
                }
            }
        }


        _unitOfWork.Products.Create(createProductResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product created successfully for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
                createProductResult.Value.Id, _user.Email, _user.UserId);

            return _mapper.Map<ProductDto>(createProductResult.Value);
        }
        catch (OperationCanceledException)
        {
            await DeleteImagesAsync(savedImageUrls, cancellationToken);
            _logger.LogWarning("Create product cancelled for ProductName: {ProductName}, UserId: {UserId}",
                request.Name, _user.UserId);
            throw;
        }
        catch (Exception ex)
        {
            await DeleteImagesAsync(savedImageUrls, cancellationToken);
            _logger.LogError(ex, "Create product failed: unable to save changes for ProductName: {ProductName}, Email: {Email}, UserId: {UserId}",
                request.Name, _user.Email, _user.UserId);
            throw;
        }
    }
    private async Task DeleteImagesAsync(IEnumerable<string> imageUrls, CancellationToken cancellationToken)
    {
        foreach (var imageUrl in imageUrls)
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
}
