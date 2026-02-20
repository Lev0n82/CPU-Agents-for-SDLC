namespace Phase3.AzureDevOps.Models;

/// <summary>
/// Represents a claim on a work item by an agent.
/// </summary>
public class WorkItemClaim
{
    /// <summary>
    /// Gets or sets the work item ID.
    /// </summary>
    public int WorkItemId { get; set; }

    /// <summary>
    /// Gets or sets the current revision number.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets the agent ID that owns the claim.
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the claim was created.
    /// </summary>
    public DateTime ClaimedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the claim expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether the claim has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Gets the remaining time until the claim expires.
    /// </summary>
    public TimeSpan TimeUntilExpiry => ExpiresAt - DateTime.UtcNow;
}
