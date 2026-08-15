using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces.Services;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.Extensions.Configuration;

namespace infrastructure.Services;

public sealed class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly string _smtpHost = configuration["Email:SmtpHost"]
        ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");

    private readonly int _smtpPort = int.TryParse(configuration["Email:SmtpPort"], out var port)
        ? port
        : throw new InvalidOperationException("Email:SmtpPort is not configured.");

    private readonly bool _enableSsl = bool.TryParse(configuration["Email:EnableSsl"], out var enableSsl) && enableSsl;

    private readonly string _username = configuration["Email:Username"]
        ?? throw new InvalidOperationException("Email:Username is not configured.");

    private readonly string _password = configuration["Email:Password"]
        ?? throw new InvalidOperationException("Email:Password is not configured.");

    private readonly string _fromEmail = configuration["Email:FromEmail"]
        ?? throw new InvalidOperationException("Email:FromEmail is not configured.");

    private readonly string _fromName = configuration["Email:FromName"] ?? "Clothing Store";

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
            Credentials = new NetworkCredential(_username, _password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        });

        var response = await Email
            .From(_fromEmail, _fromName)
            .To(email)
            .Subject(subject)
            .Body(body, true)
            .SendAsync(cancellationToken);

        if (!response.Successful)
            throw new InvalidOperationException(string.Join("; ", response.ErrorMessages)); 
    }
}
