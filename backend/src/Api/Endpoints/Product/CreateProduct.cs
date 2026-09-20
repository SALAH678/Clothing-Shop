using Api.Extensions;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Dtos;
using CreateImageDto = Application.Features.Products.Commands.CreateProduct.ImageDto;
using FastEndpoints;
using MediatR;
using System.Text.Json;
using static Api.Endpoints.Product.CreateProduct;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Api.Endpoints.Product;

public class CreateProduct(IMediator mediator) : Endpoint<CreateProductRequest, IResult>
{
    private readonly IMediator _mediator = mediator;

    public override void Configure()
    {
        Post("");
        Group<ProductGroup>();
        Roles("Admin");
        AllowFileUploads();
        Options(x => x
            .RequireRateLimiting("admin-write")
            .RequireRateLimiting("upload-concurrency")
        );

        Summary(s =>
        {
            s.Summary = "Create a product";
            s.Description = "Creates a product with optional variants and images. \n" +
                     "Variants must be sent as a JSON array string in the 'VariantsJson' field, \n" +
                     "e.g. [{\"size\":\"M\",\"color\":\"Black\",\"stockQuantity\":25}]. \n" +
                     "Accepted image formats: .jpg, .jpeg, .png, .webp.";
            s.ExampleRequest = new CreateProductRequest
            {
                Name = "Basic T-Shirt",
                Description = "Cotton t-shirt for daily use",
                BasePrice = 49.99m,
                Discount = 5m,
                CategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                VariantsJson = JsonSerializer.Serialize(new List<ProductVariantDto>{ new() { Size = "M", Color = "Black", StockQuantity = 25 }}),
                Images = null
            };
            s.Responses[200] = "Product created successfully.";
            s.Responses[400] = "Product payload is invalid.";
            s.Responses[500] = "Product creation failed.";
        });

        Description(x => x
            .Produces<ProductDto>(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task<IResult> ExecuteAsync(CreateProductRequest req, CancellationToken ct)
    {
        List<ProductVariantDto>? variants = null;
        if (!string.IsNullOrWhiteSpace(req.VariantsJson))
        {
            try
            {
                variants = JsonSerializer.Deserialize<List<ProductVariantDto>>(
                    req.VariantsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["variantsJson"] = ["Invalid JSON format for variants."]
                });
            }
        }

        List<CreateImageDto>? images = null;
        if (req.Images is { Count: > 0 })
        {
            images = req.Images
            .Select((file, index) => new CreateImageDto(
                    file.FileName,
                    file.OpenReadStream(),
                    IsMain: req.MainImageIndex == index))
                .ToList();
        }

        var command = new CreateProductCommand(req.Name, req.Description, req.BasePrice, req.Discount, req.CategoryId, variants, images);

        var result = await _mediator.Send(command, ct);

        return result.Match(
            onSuccess: value => Results.Ok(value),
            onError: errors => errors.ToProblem()
        );
    }

    public sealed class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? Discount { get; set; }
        public Guid CategoryId { get; set; }

        public string? VariantsJson { get; set; }

        // Multiple files under the same form field name
        public List<IFormFile>? Images { get; set; }

        // Index into Images indicating which one is main (e.g. "0"); null if none
        public int? MainImageIndex { get; set; }
    }
}
