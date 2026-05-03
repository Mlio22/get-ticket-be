using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Exceptions;
using Microsoft.Extensions.Options;

namespace EventManagement.Infrastructures;

public class XenditClient : IXenditClient
{
    private readonly HttpClient _httpClient;
    private readonly XenditOptions _options;

    public XenditClient(HttpClient httpClient, IOptions<XenditOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<XenditInvoiceResponse> CreateInvoiceAsync(
        XenditCreateInvoiceRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InternalServerException("Xendit SecretKey is not configured.");

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_options.SecretKey}:"))
        );

        var payload = new Dictionary<string, object?>
        {
            ["external_id"] = request.ExternalId,
            ["amount"] = request.Amount,
            ["payer_email"] = request.PayerEmail,
            ["description"] = request.Description,
            ["currency"] = request.Currency,
            ["invoice_duration"] = request.InvoiceDurationSeconds,
        };

        if (!string.IsNullOrWhiteSpace(request.SuccessRedirectUrl))
            payload["success_redirect_url"] = request.SuccessRedirectUrl;
        if (!string.IsNullOrWhiteSpace(request.FailureRedirectUrl))
            payload["failure_redirect_url"] = request.FailureRedirectUrl;

        using var response = await _httpClient.PostAsJsonAsync(
            "v2/invoices",
            payload,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BadRequestException(
                string.IsNullOrWhiteSpace(errorBody)
                    ? "Failed to create Xendit invoice."
                    : $"Failed to create Xendit invoice. {errorBody}"
            );
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var body = await JsonSerializer.DeserializeAsync<XenditCreateInvoiceApiResponse>(
            stream,
            cancellationToken: cancellationToken
        );

        if (
            body is null
            || string.IsNullOrWhiteSpace(body.Id)
            || string.IsNullOrWhiteSpace(body.InvoiceUrl)
        )
            throw new InternalServerException("Xendit invoice response was incomplete.");

        return new XenditInvoiceResponse { Id = body.Id, InvoiceUrl = body.InvoiceUrl };
    }

    private class XenditCreateInvoiceApiResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("invoice_url")]
        public string InvoiceUrl { get; set; } = string.Empty;
    }
}
