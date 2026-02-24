namespace Phase3.AzureDevOps.Models.Git;

/// <summary>
/// Represents a Git workspace.
/// </summary>
public class Workspace
{
    /// <summary>
    /// Workspace ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Workspace directory path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Repository URL (if cloned).
    /// </summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Repository path within workspace.
    /// </summary>
    public string? RepositoryPath { get; set; }

    /// <summary>
    /// Cached dependency types.
    /// </summary>
    public List<string> CachedDependencies { get; set; } = new();

    /// <summary>
    /// Workspace creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last accessed time.
    /// </summary>
    public DateTime LastAccessedAt { get; set; }

    /// <summary>
    /// Disk space used (bytes).
    /// </summary>
    public long DiskSpaceBytes { get; set; }
}

/// <summary>
/// Request model for creating a workspace.
/// </summary>
public class CreateWorkspaceRequest
{
    /// <summary>
    /// Workspace name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Repository URL to clone (optional).
    /// </summary>
    public string? RepositoryUrl { get; set; }

    /// <summary>
    /// Whether to perform shallow clone.
    /// </summary>
    public bool ShallowClone { get; set; } = true;
}

/// <summary>
/// Workspace statistics.
/// </summary>
public class WorkspaceStatistics
{
    /// <summary>
    /// Total number of workspaces.
    /// </summary>
    public int TotalWorkspaces { get; set; }

    /// <summary>
    /// Total disk space used (bytes).
    /// </summary>
    public long TotalDiskSpaceBytes { get; set; }

    /// <summary>
    /// Available disk space (bytes).
    /// </summary>
    public long AvailableDiskSpaceBytes { get; set; }

    /// <summary>
    /// Disk usage percentage.
    /// </summary>
    public double DiskUsagePercent { get; set; }

    /// <summary>
    /// Number of workspaces with cached dependencies.
    /// </summary>
    public int WorkspacesWithCachedDependencies { get; set; }
}
