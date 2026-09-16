using Application.Common.Interfaces;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Chargily.Pay;
using infrastructure.BackgroundJobs;
using infrastructure.Data;
using infrastructure.Data.Interceptors;
using infrastructure.HealthChecks;
using infrastructure.Identity;
using infrastructure.Options;
using infrastructure.Repositories;
using infrastructure.Services;
using infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        //configure dbcontext to use postgresql and add interceptor to it
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        //Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPurchaseItemRepository, PurchaseItemRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVariantRepository, VariantRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
        services.AddScoped<IImageCleanupJob, ImageCleanupJob>();
        services.AddScoped<IEmailJob, EmailJob>();

        //UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Image Service
        services.AddScoped<IImageService, ImageService>();

        services.AddScoped<IPaymentGatewayService, ChargilyPaymentGatewayService>();
        
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IOAuthService, OAuthService>();

        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddScoped<ICodeGenerator, CodeGenerator>();

        services.AddScoped<ITokenHasherService, TokenHasherService>();

        services.AddScoped<IPasswordService, PasswordService>();

        services.AddSingleton<IBackgroundJobTracker, BackgroundJobTracker>();

        //Options configuration
        services
            .AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<ImageStorageOptions>()
            .BindConfiguration(ImageStorageOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<ChargilyOptions>()
            .BindConfiguration(ChargilyOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        //configure authentication
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()!;
            
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {

            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        //email configuration
        var emailSettings = configuration
            .GetSection(EmailOptions.SectionName)
            .Get<EmailOptions>()!;

        services
            .AddFluentEmail(emailSettings.FromEmail, emailSettings.FromName)
            .AddSmtpSender(new SmtpClient(emailSettings.SmtpHost)
            {
                Port = emailSettings.SmtpPort,
                Credentials = new NetworkCredential(
                    emailSettings.Username,
                    emailSettings.Password),
                EnableSsl = emailSettings.EnableSsl
            });

        //configure health checks
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                tags: ["ready"]) // check DBContext and ef core config plus the connection with DB
            .AddTypeActivatedCheck<BackgroundJobHealthCheck>(
                name: "email_job_check",
                failureStatus: null,
                tags: ["ready", "jobs"],
                args: [nameof(EmailJob)]) // check email job
            .AddTypeActivatedCheck<BackgroundJobHealthCheck>(
                name: "image_cleanup_job_check",
                failureStatus: null,
                tags: ["ready", "jobs"],
                args: [nameof(ImageCleanupJob)]); // check image cleanup job

        services.AddAuthorization();

        //configure chargily pay client
        var chargilyOptions = configuration
            .GetSection(ChargilyOptions.SectionName)
            .Get<ChargilyOptions>()!;

        services.AddGlobalChargilyPayClient(config =>
        {
            config.IsLiveMode = chargilyOptions.IsLiveMode; // switch to true when you go live
            config.ApiSecretKey = chargilyOptions.ApiSecretKey;
        });

        return services;
    }
}
