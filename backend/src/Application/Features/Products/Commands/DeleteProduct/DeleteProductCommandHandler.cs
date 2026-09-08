using Application.Common.Interfaces;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteProductCommandHandler> logger, IUser user)
    : IRequestHandler<DeleteProductCommand, Result<Deleted>>
{
    private readonly ILogger<DeleteProductCommandHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Deleted>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete product requested for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
            request.ProductId, _user.Email, _user.UserId);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Delete product failed: product not found for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
                request.ProductId, _user.Email, _user.UserId);
            return Error.NotFound(
                code: "Product_Not_Found",
                description: $"Product with ID {request.ProductId} was not found.");
        }

        _unitOfWork.Products.Delete(product);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product deleted successfully for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
                request.ProductId, _user.Email, _user.UserId);

            return Result.Deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete product failed: unable to save changes for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
                request.ProductId, _user.Email, _user.UserId);
            return Error.Failure(
                code: "Product_Delete_Failed",
                description: "Product delete failed.");
        }
    }
}
