namespace Phase3.AzureDevOps.Interfaces;

using Phase3.AzureDevOps.Models.Git;

/// <summary>
/// Manages persistent Git workspaces with dependency caching.
/// </summary>
public interface IGitWorkspaceManager
{
    /// <summary>
    /// Creates a new workspace.
    /// </summary>
    /// <param name="request">Workspace creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workspace.</returns>
    Task<Workspace> CreateWorkspaceAsync(
        CreateWorkspaceRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workspace by ID.
    /// </summary>
    /// <param name="workspaceId">Workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workspace.</returns>
    Task<Workspace> GetWorkspaceAsync(
        Guid workspaceId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all workspaces.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of workspaces.</returns>
    Task<IEnumerable<Workspace>> ListWorkspacesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workspace.
    /// </summary>
    /// <param name="workspaceId">Workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if workspace was deleted.</returns>
    Task<bool> CleanupWorkspaceAsync(
        Guid workspaceId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches dependencies for a workspace.
    /// </summary>
    /// <param name="workspaceId">Workspace ID.</param>
    /// <param name="dependencyType">Dependency type (NuGet, npm, pip).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if dependencies were cached.</returns>
    Task<bool> CacheDependenciesAsync(
        Guid workspaceId, 
        DependencyType dependencyType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workspace statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workspace statistics.</returns>
    Task<WorkspaceStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Dependency type enumeration.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// NuGet packages (.NET).
    /// </summary>
    NuGet,

    /// <summary>
    /// npm packages (Node.js).
    /// </summary>
    Npm,

    /// <summary>
    /// pip packages (Python).
    /// </summary>
    Pip
}
