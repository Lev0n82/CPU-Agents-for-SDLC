namespace Phase3.AzureDevOps.Interfaces;

using Phase3.AzureDevOps.Models.Git;

/// <summary>
/// Provides Git repository operations.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Clones a repository to a local path.
    /// </summary>
    /// <param name="request">Clone request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="progressCallback">Progress callback.</param>
    /// <returns>The local repository path.</returns>
    Task<string> CloneRepositoryAsync(
        CloneRepositoryRequest request, 
        CancellationToken cancellationToken = default,
        Action<int>? progressCallback = null);

    /// <summary>
    /// Commits changes to the local repository.
    /// </summary>
    /// <param name="request">Commit request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The commit SHA.</returns>
    Task<string> CommitChangesAsync(
        CommitChangesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes commits to the remote repository.
    /// </summary>
    /// <param name="request">Push request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if push succeeded.</returns>
    Task<bool> PushChangesAsync(
        PushChangesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulls latest changes from the remote repository.
    /// </summary>
    /// <param name="request">Pull request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pull result with merge status.</returns>
    Task<PullResult> PullChangesAsync(
        PullChangesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current branch name.
    /// </summary>
    /// <param name="repositoryPath">Repository path.</param>
    /// <returns>The current branch name.</returns>
    string GetCurrentBranch(string repositoryPath);

    /// <summary>
    /// Checks if the repository has uncommitted changes.
    /// </summary>
    /// <param name="repositoryPath">Repository path.</param>
    /// <returns>True if there are uncommitted changes.</returns>
    bool HasUncommittedChanges(string repositoryPath);
}
