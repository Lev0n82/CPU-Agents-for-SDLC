namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Manages test case lifecycles.
/// </summary>
public interface ITestCaseLifecycleManager
{
    /// <summary>
    /// Identifies obsolete test cases linked to removed requirements.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of obsolete test case IDs.</returns>
    Task<IEnumerable<int>> IdentifyObsoleteTestCasesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes obsolete test cases.
    /// </summary>
    /// <param name="testCaseIds">Test case IDs to close.</param>
    /// <param name="reason">Closure reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of test cases closed.</returns>
    Task<int> CloseObsoleteTestCasesAsync(
        IEnumerable<int> testCaseIds,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets requirement links for a test case.
    /// </summary>
    /// <param name="testCaseId">Test case ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of linked requirement IDs.</returns>
    Task<IEnumerable<int>> GetTestCaseLinksAsync(
        int testCaseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets lifecycle audit trail for a test case.
    /// </summary>
    /// <param name="testCaseId">Test case ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Audit trail entries.</returns>
    Task<IEnumerable<LifecycleAuditEntry>> GetAuditTrailAsync(
        int testCaseId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Lifecycle audit trail entry.
/// </summary>
public class LifecycleAuditEntry
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
