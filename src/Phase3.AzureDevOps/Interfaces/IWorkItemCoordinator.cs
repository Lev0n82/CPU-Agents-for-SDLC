namespace Phase3.AzureDevOps.Interfaces;

using Phase3.AzureDevOps.Models;

/// <summary>
/// Coordinates work item processing across multiple agents to prevent race conditions.
/// </summary>
public interface IWorkItemCoordinator
{
    /// <summary>
    /// Attempts to claim a work item for processing by the specified agent.
    /// </summary>
    /// <param name="workItemId">The ID of the work item to claim.</param>
    /// <param name="revision">The current revision number of the work item (for optimistic concurrency).</param>
    /// <param name="agentId">The unique identifier of the agent claiming the work item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the claim was successful; false if the work item is already claimed.</returns>
    /// <exception cref="ArgumentException">Thrown if agentId is invalid.</exception>
    /// <exception cref="WorkItemNotFoundException">Thrown if the work item does not exist.</exception>
    Task<bool> TryClaimWorkItemAsync(int workItemId, int revision, string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a work item claim, making it available for other agents to process.
    /// </summary>
    /// <param name="workItemId">The ID of the work item to release.</param>
    /// <param name="revision">The current revision number of the work item (for optimistic concurrency).</param>
    /// <param name="agentId">The unique identifier of the agent releasing the work item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown if agentId is invalid.</exception>
    /// <exception cref="WorkItemNotFoundException">Thrown if the work item does not exist.</exception>
    Task ReleaseWorkItemAsync(int workItemId, int revision, string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews the claim on a work item, extending its expiry time.
    /// </summary>
    /// <param name="workItemId">The ID of the work item to renew.</param>
    /// <param name="revision">The current revision number of the work item (for optimistic concurrency).</param>
    /// <param name="agentId">The unique identifier of the agent renewing the claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown if agentId is invalid.</exception>
    /// <exception cref="WorkItemNotFoundException">Thrown if the work item does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the work item is not owned by the specified agent.</exception>
    Task RenewClaimAsync(int workItemId, int revision, string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all work items with expired claims.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of work items with expired claims.</returns>
    Task<IEnumerable<WorkItemClaim>> GetExpiredClaimsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases all expired claims, making them available for processing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of claims that were successfully released.</returns>
    Task<int> ReleaseExpiredClaimsAsync(CancellationToken cancellationToken = default);
}
