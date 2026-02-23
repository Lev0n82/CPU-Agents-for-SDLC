namespace Phase3.AzureDevOps.Services.Git;

using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Git;

/// <summary>
/// Implements Git operations using LibGit2Sharp.
/// </summary>
public class GitService : IGitService
{
    private readonly ISecretsProvider _secretsProvider;
    private readonly ILogger<GitService> _logger;

    public GitService(
        ISecretsProvider secretsProvider,
        ILogger<GitService> logger)
    {
        _secretsProvider = secretsProvider ?? throw new ArgumentNullException(nameof(secretsProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> CloneRepositoryAsync(
        CloneRepositoryRequest request, 
        CancellationToken cancellationToken = default,
        Action<int>? progressCallback = null)
    {
        ValidateCloneRequest(request);

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", 
            request.RepositoryUrl, request.LocalPath);

        var cloneOptions = new CloneOptions
        {
            CredentialsProvider = await GetCredentialsProviderAsync(),
            IsBare = false,
            Checkout = true,
            OnProgress = output =>
            {
                _logger.LogDebug("Clone progress: {Output}", output);
                return !cancellationToken.IsCancellationRequested;
            },
            OnTransferProgress = progress =>
            {
                int percentComplete = progress.TotalObjects > 0 
                    ? (int)((progress.ReceivedObjects * 100) / progress.TotalObjects)
                    : 0;
                progressCallback?.Invoke(percentComplete);
                return !cancellationToken.IsCancellationRequested;
            }
        };

        // Note: Shallow clone depth not supported in LibGit2Sharp 0.27.2
        // Use git command line for shallow clones if needed

        try
        {
            var repositoryPath = Repository.Clone(
                sourceUrl: request.RepositoryUrl,
                workdirPath: request.LocalPath,
                options: cloneOptions);

            _logger.LogInformation("Successfully cloned repository to {RepositoryPath}", repositoryPath);
            return repositoryPath;
        }
        catch (LibGit2SharpException ex)
        {
            _logger.LogError(ex, "Failed to clone repository {RepositoryUrl}", request.RepositoryUrl);
            throw new GitOperationException($"Failed to clone repository: {ex.Message}", ex);
        }
    }

    public async Task<string> CommitChangesAsync(
        CommitChangesRequest request, 
        CancellationToken cancellationToken = default)
    {
        ValidateCommitRequest(request);

        _logger.LogInformation("Committing changes in {RepositoryPath}", request.RepositoryPath);

        try
        {
            using var repo = new Repository(request.RepositoryPath);

            // Stage all changes
            Commands.Stage(repo, "*");

            // Create signature
            var signature = new Signature(
                name: request.AuthorName,
                email: request.AuthorEmail,
                when: DateTimeOffset.Now);

            // Commit
            var commit = repo.Commit(
                message: request.CommitMessage,
                author: signature,
                committer: signature);

            _logger.LogInformation("Created commit {CommitSha} in {RepositoryPath}", 
                commit.Sha, request.RepositoryPath);

            return commit.Sha;
        }
        catch (LibGit2SharpException ex)
        {
            _logger.LogError(ex, "Failed to commit changes in {RepositoryPath}", request.RepositoryPath);
            throw new GitOperationException($"Failed to commit changes: {ex.Message}", ex);
        }
    }

    public async Task<bool> PushChangesAsync(
        PushChangesRequest request, 
        CancellationToken cancellationToken = default)
    {
        ValidatePushRequest(request);

        _logger.LogInformation("Pushing changes from {RepositoryPath} to {RemoteName}/{BranchName}", 
            request.RepositoryPath, request.RemoteName, request.BranchName);

        try
        {
            using var repo = new Repository(request.RepositoryPath);

            var remote = repo.Network.Remotes[request.RemoteName];
            if (remote == null)
            {
                throw new GitOperationException($"Remote '{request.RemoteName}' not found");
            }

            var pushOptions = new PushOptions
            {
                CredentialsProvider = await GetCredentialsProviderAsync()
            };

            var branch = repo.Branches[request.BranchName];
            if (branch == null)
            {
                throw new GitOperationException($"Branch '{request.BranchName}' not found");
            }

            repo.Network.Push(branch, pushOptions);

            _logger.LogInformation("Successfully pushed changes to {RemoteName}/{BranchName}", 
                request.RemoteName, request.BranchName);

            return true;
        }
        catch (LibGit2SharpException ex)
        {
            _logger.LogError(ex, "Failed to push changes from {RepositoryPath}", request.RepositoryPath);
            throw new GitOperationException($"Failed to push changes: {ex.Message}", ex);
        }
    }

    public async Task<PullResult> PullChangesAsync(
        PullChangesRequest request, 
        CancellationToken cancellationToken = default)
    {
        ValidatePullRequest(request);

        _logger.LogInformation("Pulling changes in {RepositoryPath}", request.RepositoryPath);

        try
        {
            using var repo = new Repository(request.RepositoryPath);

            var signature = new Signature(
                name: request.AuthorName,
                email: request.AuthorEmail,
                when: DateTimeOffset.Now);

            var pullOptions = new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = await GetCredentialsProviderAsync()
                },
                MergeOptions = new MergeOptions
                {
                    FastForwardStrategy = FastForwardStrategy.Default
                }
            };

            var mergeResult = Commands.Pull(
                repository: repo,
                merger: signature,
                options: pullOptions);

            var result = new PullResult
            {
                Status = mergeResult.Status,
                Commit = mergeResult.Commit?.Sha,
                HasConflicts = mergeResult.Status == MergeStatus.Conflicts
            };

            if (result.HasConflicts)
            {
                result.Conflicts = repo.Index.Conflicts.Select(c => c.Ancestor.Path).ToList();
                _logger.LogWarning("Pull resulted in {ConflictCount} conflicts", result.Conflicts.Count);
            }
            else
            {
                _logger.LogInformation("Successfully pulled changes in {RepositoryPath}", request.RepositoryPath);
            }

            return result;
        }
        catch (LibGit2SharpException ex)
        {
            _logger.LogError(ex, "Failed to pull changes in {RepositoryPath}", request.RepositoryPath);
            throw new GitOperationException($"Failed to pull changes: {ex.Message}", ex);
        }
    }

    public string GetCurrentBranch(string repositoryPath)
    {
        try
        {
            using var repo = new Repository(repositoryPath);
            return repo.Head.FriendlyName;
        }
        catch (LibGit2SharpException ex)
        {
            _logger.LogError(ex, "Failed to get current branch in {RepositoryPath}", repositoryPath);
            throw new GitOperationException($"Failed to get current branch: {ex.Message}", ex);
        }
    }

    public bool HasUncommittedChanges(string repositoryPath)
    {
        try
        {
            using var repo = new Repository(repositoryPath);
            var status = repo.RetrieveStatus();
            return status.IsDirty;
        }
        catch (LibGit2SharpException ex)
        {
            _logger.LogError(ex, "Failed to check repository status in {RepositoryPath}", repositoryPath);
            throw new GitOperationException($"Failed to check repository status: {ex.Message}", ex);
        }
    }

    private async Task<LibGit2Sharp.Handlers.CredentialsHandler> GetCredentialsProviderAsync()
    {
        var pat = await _secretsProvider.GetSecretAsync("AzureDevOpsPAT");
        
        return (url, usernameFromUrl, types) =>
            new UsernamePasswordCredentials
            {
                Username = "pat",
                Password = pat
            };
    }

    private void ValidateCloneRequest(CloneRepositoryRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl)) 
            throw new ValidationException("Repository URL is required");
        if (string.IsNullOrWhiteSpace(request.LocalPath)) 
            throw new ValidationException("Local path is required");
    }

    private void ValidateCommitRequest(CommitChangesRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.RepositoryPath)) 
            throw new ValidationException("Repository path is required");
        if (string.IsNullOrWhiteSpace(request.CommitMessage)) 
            throw new ValidationException("Commit message is required");
        if (string.IsNullOrWhiteSpace(request.AuthorName)) 
            throw new ValidationException("Author name is required");
        if (string.IsNullOrWhiteSpace(request.AuthorEmail)) 
            throw new ValidationException("Author email is required");
    }

    private void ValidatePushRequest(PushChangesRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.RepositoryPath)) 
            throw new ValidationException("Repository path is required");
        if (string.IsNullOrWhiteSpace(request.RemoteName)) 
            throw new ValidationException("Remote name is required");
        if (string.IsNullOrWhiteSpace(request.BranchName)) 
            throw new ValidationException("Branch name is required");
    }

    private void ValidatePullRequest(PullChangesRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.RepositoryPath)) 
            throw new ValidationException("Repository path is required");
        if (string.IsNullOrWhiteSpace(request.AuthorName)) 
            throw new ValidationException("Author name is required");
        if (string.IsNullOrWhiteSpace(request.AuthorEmail)) 
            throw new ValidationException("Author email is required");
    }
}
