namespace Phase3.AzureDevOps.Models.Sync;

/// <summary>
/// Result of a synchronization operation.
/// </summary>
public class SyncResult
{
    /// <summary>
    /// Sync start time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Sync end time.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Number of successfully synced operations.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Detected conflicts.
    /// </summary>
    public List<Conflict> Conflicts { get; set; } = new();
}

/// <summary>
/// Represents a synchronization conflict.
/// </summary>
public class Conflict
{
    /// <summary>
    /// Conflict ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Work item ID.
    /// </summary>
    public int WorkItemId { get; set; }

    /// <summary>
    /// Local version of work item.
    /// </summary>
    public WorkItemData? LocalVersion { get; set; }

    /// <summary>
    /// Remote version of work item.
    /// </summary>
    public WorkItemData? RemoteVersion { get; set; }

    /// <summary>
    /// Base version (for 3-way merge).
    /// </summary>
    public WorkItemData? BaseVersion { get; set; }

    /// <summary>
    /// Conflict type.
    /// </summary>
    public ConflictType ConflictType { get; set; }

    /// <summary>
    /// Conflict creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Work item data snapshot.
/// </summary>
public class WorkItemData
{
    /// <summary>
    /// Work item ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Work item revision.
    /// </summary>
    public int Rev { get; set; }

    /// <summary>
    /// Work item fields.
    /// </summary>
    public Dictionary<string, object> Fields { get; set; } = new();
}

/// <summary>
/// Conflict type enumeration.
/// </summary>
public enum ConflictType
{
    /// <summary>
    /// Both local and remote modified the same work item.
    /// </summary>
    ModifyConflict,

    /// <summary>
    /// Local deleted, remote modified.
    /// </summary>
    DeleteModifyConflict,

    /// <summary>
    /// Local modified, remote deleted.
    /// </summary>
    ModifyDeleteConflict
}

/// <summary>
/// Conflict resolution policy enumeration.
/// </summary>
public enum ConflictResolutionPolicy
{
    /// <summary>
    /// Abort local changes and keep remote version.
    /// </summary>
    Abort,

    /// <summary>
    /// Perform automatic 3-way merge.
    /// </summary>
    Merge,

    /// <summary>
    /// Queue conflict for manual review.
    /// </summary>
    ManualReview,

    /// <summary>
    /// Force overwrite remote with local version.
    /// </summary>
    ForceOverwrite
}

/// <summary>
/// Offline status information.
/// </summary>
public class OfflineStatus
{
    /// <summary>
    /// Whether offline mode is enabled.
    /// </summary>
    public bool IsOfflineMode { get; set; }

    /// <summary>
    /// Number of pending operations.
    /// </summary>
    public int PendingOperations { get; set; }

    /// <summary>
    /// Number of pending conflicts.
    /// </summary>
    public int PendingConflicts { get; set; }

    /// <summary>
    /// Last sync time.
    /// </summary>
    public DateTime? LastSyncTime { get; set; }
}

/// <summary>
/// Represents a pending offline operation.
/// </summary>
internal class PendingOperation
{
    /// <summary>
    /// Operation ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Work item ID (0 for create operations).
    /// </summary>
    public int WorkItemId { get; set; }

    /// <summary>
    /// Operation type (Create, Update, Delete).
    /// </summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// Operation payload (JSON).
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Base revision (for conflict detection).
    /// </summary>
    public int BaseRevision { get; set; }

    /// <summary>
    /// Operation creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
