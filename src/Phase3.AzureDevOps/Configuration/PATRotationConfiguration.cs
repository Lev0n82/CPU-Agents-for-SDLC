namespace Phase3.AzureDevOps.Configuration;

public class PATRotationConfiguration
{
    public int RotationIntervalDays { get; set; } = 30;
    public int ExpiryWarningDays { get; set; } = 7;
}
