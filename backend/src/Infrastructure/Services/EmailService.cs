using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using FluentEmail.Core;
using FluentEmail.Smtp;
using infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace infrastructure.Services;

public sealed class EmailService(ILogger<EmailService> logger, IOptions<EmailOptions> options) : IEmailService
{
    private readonly string _smtpHost = options.Value.SmtpHost
        ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");

    private readonly int _smtpPort = options.Value.SmtpPort > 0
        ? options.Value.SmtpPort
        : throw new InvalidOperationException("Email:SmtpPort is not configured.");

    private readonly bool _enableSsl = options.Value.EnableSsl;

    private readonly string _username = options.Value.Username
        ?? throw new InvalidOperationException("Email:Username is not configured.");

    private readonly string _password = options.Value.Password
        ?? throw new InvalidOperationException("Email:Password is not configured.");

    private readonly string _fromEmail = options.Value.FromEmail
        ?? throw new InvalidOperationException("Email:FromEmail is not configured.");

    private readonly string _fromName = options.Value.FromName ?? "Clothing Store";

    private readonly string _emailAdmin = options.Value.EmailAdmin
        ?? throw new InvalidOperationException("Email:EmailAdmin is not configured.");

    public async Task SendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6;">
                <h2>Email Verification</h2>
                <p>Your verification code is:</p>
                <p style="font-size: 24px; font-weight: bold; letter-spacing: 2px;">{code}</p>
                <p>Use this code to verify your email address.</p>
            </body>
            </html>
            """;

        await SendAsync(email, "Verify your email", body, cancellationToken);
    }

    public async Task SendPurchaseNotificationAsync(Guid purchaseId, string fullName, string phoneNumber, List<PurchaseNotificationItemPayload> items,
        decimal totalAmount, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentNullException.ThrowIfNull(items);
        var itemsHtml = string.Join("", items.Select(item => $"""
            <tr>
                <td>{item.ProductName}</td>
                <td>{item.Size}</td>
                <td>{item.Color}</td>
                <td>{item.Price:N2} DZ</td>
                <td>{item.Quantity}</td>
            </tr>
        """));
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6;">
                <h2>Purchase Notification</h2>
                <p>Here are the purchase details:</p>
                <p><strong>Purchase ID:</strong> {purchaseId}</p>
                <p><strong>Customer Name:</strong> {fullName}</p>
                <p><strong>Customer Phone Number:</strong> {phoneNumber}</p>
                <table border="1" cellpadding="5" cellspacing="0" style="border-collapse: collapse; width: 100%;">
                    <thead>
                        <tr>
                            <th>Product Name</th>
                            <th>Size</th>
                            <th>Color</th>
                            <th>Price</th>
                            <th>Quantity</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>
                <p><strong>Total Amount:</strong> {totalAmount:N2} DZ</p>
            </body>
            </html>
            """;
        await SendAsync(_emailAdmin, "Your Purchase Details", body, cancellationToken);
    }

    public async Task SendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6;">
                <h2>Password Reset</h2>
                <p>Your password reset code is:</p>
                <p style="font-size: 24px; font-weight: bold; letter-spacing: 2px;">{code}</p>
                <p>Use this code to reset your password. If you did not request this, you can ignore this email.</p>
            </body>
            </html>
            """;

        await SendAsync(email, "Reset your password", body, cancellationToken);
    }

    private async Task SendAsync(string email, string subject, string body, CancellationToken cancellationToken)
    {
        Email.DefaultSender = new SmtpSender(() => new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = _enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_username, _password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        });

        try
        {
            var response = await Email
                .From(_fromEmail, _fromName)
                .To(email)
                .Subject(subject)
                .Body(body, true)
                .SendAsync(cancellationToken);

            if (!response.Successful)
            {
                var errors = string.Join("; ", response.ErrorMessages);
                logger.LogError("Failed to send email to {Email} with subject '{Subject}'. Errors: {Errors}", email, subject, errors);
                throw new InvalidOperationException(errors);
            }

            logger.LogInformation("Successfully sent email to {Email} with subject '{Subject}'.", email, subject);
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            logger.LogError(exception, "Exception occurred while sending email to {Email} with subject '{Subject}'", email, subject);
            throw;
        }
    }
}
