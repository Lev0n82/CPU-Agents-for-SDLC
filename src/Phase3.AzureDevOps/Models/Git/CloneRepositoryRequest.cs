namespace Phase3.AzureDevOps.Models.Git;

/// <summary>
/// Request model for cloning a repository.
/// </summary>
public class CloneRepositoryRequest
{
    /// <summary>
    /// Repository URL.
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Local path to clone to.
    /// </summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to perform a shallow clone (--depth 1).
    /// </summary>
    public bool ShallowClone { get; set; } = true;
}
