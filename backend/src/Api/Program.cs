using Chargily.Pay.AspNet;
using FastEndpoints;
using FastEndpoints.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Serilog;
using TickerQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// add middleware to the HTTP request pipeline.

app.UseDefaultExceptionHandler();

app.UseSerilogRequestLogging();

app.UseHsts();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
    app.UseCors("clothingStoreDevCors");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["ImageStorage:BasePath"]!)),
    RequestPath = builder.Configuration["ImageStorage:BaseUrl"]!
});

app.UseTickerQ();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.UseChargilyPayWebhookValidation();

app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
    app.UseSwaggerGen();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
