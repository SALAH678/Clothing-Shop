using Api.Services;
using Application.Common.Interfaces;
using Asp.Versioning;
using Chargily.Pay.AspNet;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastEndpoints.Swagger;
using Serilog;
using System.Threading.RateLimiting;
using TickerQ.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddTickerQ();
        services.AddChargilyPayWebhookValidationMiddleware();
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

        static string GetClientIp(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        static string GetEmailKey(HttpContext ctx) => ctx.Request.Query["email"].FirstOrDefault() ?? GetClientIp(ctx);
        static string GetUserId(HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? GetClientIp(ctx);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("standard", context =>
                 RateLimitPartition.GetSlidingWindowLimiter(
                     partitionKey: GetClientIp(context),
                     factory: _ => new SlidingWindowRateLimiterOptions
                     {
                         PermitLimit = 100,
                         Window = TimeSpan.FromMinutes(1),
                         SegmentsPerWindow = 4,
                         QueueLimit = 0
                     }
                 )
            );

            // Register — tightest IP limit, each request is costly (account + email)
            options.AddPolicy("auth-ip-create", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetClientIp(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            // ExternalAuthLogin, ExternalAuthRegister, Refresh — cost/DoS only, no guessing risk
            options.AddPolicy("auth-ip-relaxed", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetClientIp(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            // LogIn's IP leg — spray-attack guard, stacked with the email-based leg below
            options.AddPolicy("auth-ip-spray-guard", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetClientIp(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            // LogIn, ResetPassword, VerifyEmail — email-partitioned, guards against credential/code guessing
            options.AddPolicy("auth-email-strict", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetEmailKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            // ForgotPassword, ResendCode — target-partitioned, guards against inbox/SMS bombing
            options.AddPolicy("auth-target-strict", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetEmailKey(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 3, Window = TimeSpan.FromMinutes(15), SegmentsPerWindow = 3, QueueLimit = 0 }));

            options.AddPolicy("cart-read", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            options.AddPolicy("cart-write", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            options.AddPolicy("admin-read", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            options.AddPolicy("admin-write", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

            options.AddPolicy("purchase-strict", context =>
                RateLimitPartition.GetSlidingWindowLimiter(GetUserId(context),
                    _ => new SlidingWindowRateLimiterOptions
                    { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), SegmentsPerWindow = 4, QueueLimit = 0 }));

        });

        return services;
    }
}
