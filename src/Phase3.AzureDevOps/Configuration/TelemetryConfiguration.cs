namespace Phase3.AzureDevOps.Configuration;

/// <summary>
/// Telemetry configuration settings.
/// </summary>
public class TelemetryConfiguration
{
    /// <summary>
    /// Service name for telemetry.
    /// </summary>
    public string ServiceName { get; set; } = "cpu-agents";

    /// <summary>
    /// Service version.
    /// </summary>
    public string ServiceVersion { get; set; } = "3.0.0";

    /// <summary>
    /// OTLP exporter endpoint.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Whether to export to console (for debugging).
    /// </summary>
    public bool ConsoleExporter { get; set; } = false;

    /// <summary>
    /// Sampling rate (0.0 to 1.0).
    /// </summary>
    public double SamplingRate { get; set; } = 1.0;
}
