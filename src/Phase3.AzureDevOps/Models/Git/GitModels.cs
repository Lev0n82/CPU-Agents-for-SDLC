namespace Phase3.AzureDevOps.Models.Git;

using LibGit2Sharp;

/// <summary>
/// Request model for committing changes.
/// </summary>
public class CommitChangesRequest
{
    /// <summary>
    /// Repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Commit message.
    /// </summary>
    public string CommitMessage { get; set; } = string.Empty;

    /// <summary>
    /// Author name.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Author email.
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;
}

/// <summary>
/// Request model for pushing changes.
/// </summary>
public class PushChangesRequest
{
    /// <summary>
    /// Repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Remote name (e.g., "origin").
    /// </summary>
    public string RemoteName { get; set; } = "origin";

    /// <summary>
    /// Branch name (e.g., "main").
    /// </summary>
    public string BranchName { get; set; } = "main";
}

/// <summary>
/// Request model for pulling changes.
/// </summary>
public class PullChangesRequest
{
    /// <summary>
    /// Repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Author name for merge commit.
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Author email for merge commit.
    /// </summary>
    public string AuthorEmail { get; set; } = string.Empty;
}

/// <summary>
/// Result of a pull operation.
/// </summary>
public class PullResult
{
    /// <summary>
    /// Merge status.
    /// </summary>
    public MergeStatus Status { get; set; }

    /// <summary>
    /// Commit SHA (if merge succeeded).
    /// </summary>
    public string? Commit { get; set; }

    /// <summary>
    /// Whether there are conflicts.
    /// </summary>
    public bool HasConflicts { get; set; }

    /// <summary>
    /// List of conflicted file paths.
    /// </summary>
    public List<string> Conflicts { get; set; } = new();
}
