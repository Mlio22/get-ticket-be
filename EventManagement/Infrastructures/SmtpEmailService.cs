using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace EventManagement.Infrastructures;

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> emailOptions, ILogger<SmtpEmailService> logger)
    {
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        IReadOnlyCollection<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!_emailOptions.IsEnabled)
            return;

        using var message = new MailMessage
        {
            From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        message.To.Add(new MailAddress(toEmail));

        var streams = new List<MemoryStream>();
        try
        {
            if (attachments is not null)
            {
                foreach (var attachment in attachments.Where(a => a.Content.Length > 0))
                {
                    var stream = new MemoryStream(attachment.Content);
                    streams.Add(stream);
                    message.Attachments.Add(
                        new Attachment(stream, attachment.FileName, attachment.ContentType)
                    );
                }
            }

            using var smtpClient = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
            {
                EnableSsl = _emailOptions.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_emailOptions.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_emailOptions.Username, _emailOptions.Password),
            };

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {Email}", toEmail);
        }
        finally
        {
            foreach (var stream in streams)
                stream.Dispose();
        }
    }
}
