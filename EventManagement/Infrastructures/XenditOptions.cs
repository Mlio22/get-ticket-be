namespace EventManagement.Infrastructures;

public class XenditOptions
{
    public string BaseUrl { get; set; } = "https://api.xendit.co";
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookToken { get; set; } = string.Empty;
    public int InvoiceDurationSeconds { get; set; } = 900;
    public string Currency { get; set; } = "IDR";
}
