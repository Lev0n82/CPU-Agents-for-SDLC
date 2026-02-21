namespace Phase3.AzureDevOps.Configuration;

public class PATRotationConfiguration
{
    public bool Enabled { get; set; } = false;
    public int RotationIntervalDays { get; set; } = 30;
    public int ExpiryWarningDays { get; set; } = 7;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public int NewPATValidityDays { get; set; } = 90;
    public int GracePeriodHours { get; set; } = 24;
}
