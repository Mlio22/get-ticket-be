namespace EventManagement.Infrastructures;

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        IReadOnlyCollection<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default
    );
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = [];
}
