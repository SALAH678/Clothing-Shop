using Api.Services;
using Application.Common.Interfaces;
using Asp.Versioning;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;
using Serilog;
using TickerQ.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddTickerQ();
        services.AddFastEndpoints();

        services.AddVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);

            options.AssumeDefaultVersionWhenUnspecified = true;

            options.ApiVersionReader =
                new HeaderApiVersionReader("X-Api-Version");
        });

        VersionSets.CreateApi("ClothingStoreApi", v =>
        {
            v.HasApiVersion(new ApiVersion(1.0));
        });

        services.SwaggerDocument(o =>
        {
            o.AutoTagPathSegmentIndex = 0;
            o.EnableJWTBearerAuth = true;
            o.ExcludeNonFastEndpoints = true;
            o.DocumentSettings = s =>
            {
                s.Title = "Clothing Store API";
                s.Version = "v1";
                s.Description = "API documentation for clothing shop";
                s.PostProcess = doc =>
                {
                    const string versionTag = "ClothingStoreApi";

                    foreach (var op in doc.Operations)
                        op.Operation.Tags.Remove(versionTag);

                    doc.Tags = doc.Tags
                        .Where(t => t.Name != versionTag)
                        .ToList();
                };
            };
            o.ShortSchemaNames = true;
        });

        services.AddSerilog((context, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(configuration));

        services.AddCors(options =>
        {
            options.AddPolicy("clothingStoreDevCors", policy =>
            {
                policy.WithOrigins("https://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
