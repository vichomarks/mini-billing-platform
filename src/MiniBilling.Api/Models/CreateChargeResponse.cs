namespace MiniBilling.Api.Models;

public class CreateChargeResponse
{
    public string ChargeId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}