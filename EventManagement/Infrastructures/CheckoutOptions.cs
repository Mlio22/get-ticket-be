namespace EventManagement.Infrastructures;

public class CheckoutOptions
{
    public int HoldDurationMinutes { get; set; } = 15;
    public int ExpirationSweepIntervalSeconds { get; set; } = 5;
    public int ExpirationSweepBatchSize { get; set; } = 100;
}
