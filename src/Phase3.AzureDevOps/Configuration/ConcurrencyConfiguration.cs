namespace Phase3.AzureDevOps.Configuration;

public class ConcurrencyConfiguration
{
    public int ClaimDurationMinutes { get; set; } = 15;
    public int StaleClaimCheckIntervalMinutes { get; set; } = 5;
}
