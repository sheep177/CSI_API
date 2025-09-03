using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CivicFlow.Infrastructure;

public class EmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _smtpHost = config["Email:SmtpHost"] ?? throw new ArgumentNullException("Email:SmtpHost");
        _smtpPort = int.TryParse(config["Email:SmtpPort"], out var port) ? port : 587;
        _smtpUser = config["Email:SmtpUser"] ?? throw new ArgumentNullException("Email:SmtpUser");
        _smtpPass = config["Email:SmtpPass"] ?? throw new ArgumentNullException("Email:SmtpPass");
        _fromEmail = config["Email:From"] ?? "noreply@civicflow.com";
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass)
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Recipient}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            throw; // 可选：你可以选择吞掉异常，或在调用方处理
        }
    }
}