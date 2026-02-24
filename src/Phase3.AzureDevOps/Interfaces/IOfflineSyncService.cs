namespace Phase3.AzureDevOps.Interfaces;

using Phase3.AzureDevOps.Models.Sync;

/// <summary>
/// Provides offline synchronization operations.
/// </summary>
public interface IOfflineSyncService
{
    /// <summary>
    /// Enables offline mode.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if offline mode was enabled.</returns>
    Task<bool> EnableOfflineModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables offline mode and syncs pending changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with conflict information.</returns>
    Task<SyncResult> DisableOfflineModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs offline changes to the server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with conflict information.</returns>
    Task<SyncResult> SyncChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detected conflicts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of conflicts.</returns>
    Task<IEnumerable<Conflict>> GetConflictsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a conflict using the specified policy.
    /// </summary>
    /// <param name="conflictId">Conflict ID.</param>
    /// <param name="policy">Resolution policy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if conflict was resolved.</returns>
    Task<bool> ResolveConflictAsync(
        Guid conflictId, 
        ConflictResolutionPolicy policy, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current offline status.
    /// </summary>
    /// <returns>Offline status information.</returns>
    Task<OfflineStatus> GetOfflineStatusAsync();
}
