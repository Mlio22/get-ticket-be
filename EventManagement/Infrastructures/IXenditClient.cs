namespace EventManagement.Infrastructures;

public interface IXenditClient
{
    Task<XenditInvoiceResponse> CreateInvoiceAsync(
        XenditCreateInvoiceRequest request,
        CancellationToken cancellationToken
    );
}

public class XenditCreateInvoiceRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PayerEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Currency { get; set; } = "IDR";
    public int InvoiceDurationSeconds { get; set; }
    public string? SuccessRedirectUrl { get; set; }
    public string? FailureRedirectUrl { get; set; }
}

public class XenditInvoiceResponse
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceUrl { get; set; } = string.Empty;
}
