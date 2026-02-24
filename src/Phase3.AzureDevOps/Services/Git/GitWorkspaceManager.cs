namespace Phase3.AzureDevOps.Services.Git;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Git;
using System.Diagnostics;
using System.Text.Json;

/// <summary>
/// Manages persistent Git workspaces with dependency caching.
/// </summary>
public class GitWorkspaceManager : IGitWorkspaceManager
{
    private readonly IGitService _gitService;
    private readonly ILogger<GitWorkspaceManager> _logger;
    private readonly string _workspaceRoot;
    private readonly long _maxDiskUsageBytes;
    private readonly double _cleanupThresholdPercent;
    private readonly string _metadataPath;
    private readonly SemaphoreSlim _workspaceLock = new(1, 1);

    public GitWorkspaceManager(
        IGitService gitService,
        ILogger<GitWorkspaceManager> logger,
        IConfiguration configuration)
    {
        _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _workspaceRoot = configuration["GitWorkspace:RootPath"] ?? "/workspaces";
        _maxDiskUsageBytes = (configuration.GetValue<long>("GitWorkspace:MaxDiskUsageGB", 100)) * 1024 * 1024 * 1024;
        _cleanupThresholdPercent = configuration.GetValue<double>("GitWorkspace:CleanupThresholdPercent", 90);
        _metadataPath = Path.Combine(_workspaceRoot, ".metadata");

        InitializeWorkspaceRoot();
    }

    public async Task<Workspace> CreateWorkspaceAsync(
        CreateWorkspaceRequest request, 
        CancellationToken cancellationToken = default)
    {
        ValidateCreateWorkspaceRequest(request);

        await _workspaceLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Creating workspace {WorkspaceName}", request.Name);

            // Ensure disk space
            await EnsureDiskSpaceAsync(cancellationToken);

            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Path = Path.Combine(_workspaceRoot, request.Name),
                RepositoryUrl = request.RepositoryUrl,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            // Create workspace directory
            Directory.CreateDirectory(workspace.Path);

            // Clone repository if URL provided
            if (!string.IsNullOrEmpty(request.RepositoryUrl))
            {
                var repoPath = Path.Combine(workspace.Path, "repo");
                var cloneRequest = new CloneRepositoryRequest
                {
                    RepositoryUrl = request.RepositoryUrl,
                    LocalPath = repoPath,
                    ShallowClone = request.ShallowClone
                };

                workspace.RepositoryPath = await _gitService.CloneRepositoryAsync(cloneRequest, cancellationToken);
            }

            // Calculate disk space
            workspace.DiskSpaceBytes = GetDirectorySize(workspace.Path);

            // Save metadata
            await SaveWorkspaceMetadataAsync(workspace, cancellationToken);

            _logger.LogInformation("Created workspace {WorkspaceId} at {WorkspacePath}", 
                workspace.Id, workspace.Path);

            return workspace;
        }
        finally
        {
            _workspaceLock.Release();
        }
    }

    public async Task<Workspace> GetWorkspaceAsync(
        Guid workspaceId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving workspace {WorkspaceId}", workspaceId);

        var workspace = await LoadWorkspaceMetadataAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            throw new NotFoundException($"Workspace {workspaceId} not found");
        }

        // Update last accessed time
        workspace.LastAccessedAt = DateTime.UtcNow;
        await SaveWorkspaceMetadataAsync(workspace, cancellationToken);

        return workspace;
    }

    public async Task<IEnumerable<Workspace>> ListWorkspacesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing all workspaces");

        if (!Directory.Exists(_metadataPath))
        {
            return Enumerable.Empty<Workspace>();
        }

        var workspaces = new List<Workspace>();
        var metadataFiles = Directory.GetFiles(_metadataPath, "*.json");

        foreach (var file in metadataFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var workspace = JsonSerializer.Deserialize<Workspace>(json);
                if (workspace != null)
                {
                    workspaces.Add(workspace);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load workspace metadata from {File}", file);
            }
        }

        return workspaces.OrderByDescending(w => w.LastAccessedAt);
    }

    public async Task<bool> CleanupWorkspaceAsync(
        Guid workspaceId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cleaning up workspace {WorkspaceId}", workspaceId);

        await _workspaceLock.WaitAsync(cancellationToken);
        try
        {
            var workspace = await LoadWorkspaceMetadataAsync(workspaceId, cancellationToken);
            if (workspace == null)
            {
                _logger.LogWarning("Workspace {WorkspaceId} not found", workspaceId);
                return false;
            }

            // Delete workspace directory
            if (Directory.Exists(workspace.Path))
            {
                Directory.Delete(workspace.Path, recursive: true);
            }

            // Delete metadata
            var metadataFile = Path.Combine(_metadataPath, $"{workspaceId}.json");
            if (File.Exists(metadataFile))
            {
                File.Delete(metadataFile);
            }

            _logger.LogInformation("Cleaned up workspace {WorkspaceId}", workspaceId);
            return true;
        }
        finally
        {
            _workspaceLock.Release();
        }
    }

    public async Task<bool> CacheDependenciesAsync(
        Guid workspaceId, 
        DependencyType dependencyType, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Caching {DependencyType} dependencies for workspace {WorkspaceId}", 
            dependencyType, workspaceId);

        var workspace = await GetWorkspaceAsync(workspaceId, cancellationToken);

        if (string.IsNullOrEmpty(workspace.RepositoryPath))
        {
            _logger.LogWarning("Workspace {WorkspaceId} has no repository", workspaceId);
            return false;
        }

        bool success = dependencyType switch
        {
            DependencyType.NuGet => await CacheNuGetDependenciesAsync(workspace, cancellationToken),
            DependencyType.Npm => await CacheNpmDependenciesAsync(workspace, cancellationToken),
            DependencyType.Pip => await CachePipDependenciesAsync(workspace, cancellationToken),
            _ => throw new ArgumentException($"Unknown dependency type: {dependencyType}")
        };

        if (success)
        {
            workspace.CachedDependencies.Add(dependencyType.ToString());
            workspace.DiskSpaceBytes = GetDirectorySize(workspace.Path);
            await SaveWorkspaceMetadataAsync(workspace, cancellationToken);
        }

        return success;
    }

    public async Task<WorkspaceStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating workspace statistics");

        var workspaces = await ListWorkspacesAsync(cancellationToken);
        var totalDiskSpace = workspaces.Sum(w => w.DiskSpaceBytes);

        // Get available disk space
        var driveInfo = new DriveInfo(Path.GetPathRoot(_workspaceRoot)!);
        var availableSpace = driveInfo.AvailableFreeSpace;

        var stats = new WorkspaceStatistics
        {
            TotalWorkspaces = workspaces.Count(),
            TotalDiskSpaceBytes = totalDiskSpace,
            AvailableDiskSpaceBytes = availableSpace,
            DiskUsagePercent = (totalDiskSpace / (double)_maxDiskUsageBytes) * 100,
            WorkspacesWithCachedDependencies = workspaces.Count(w => w.CachedDependencies.Any())
        };

        return stats;
    }

    private async Task<bool> CacheNuGetDependenciesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Caching NuGet dependencies for workspace {WorkspaceId}", workspace.Id);

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "restore",
                    WorkingDirectory = workspace.RepositoryPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Successfully cached NuGet dependencies for workspace {WorkspaceId}", workspace.Id);
                return true;
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                _logger.LogWarning("Failed to cache NuGet dependencies: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching NuGet dependencies for workspace {WorkspaceId}", workspace.Id);
            return false;
        }
    }

    private async Task<bool> CacheNpmDependenciesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Caching npm dependencies for workspace {WorkspaceId}", workspace.Id);

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "install",
                    WorkingDirectory = workspace.RepositoryPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Successfully cached npm dependencies for workspace {WorkspaceId}", workspace.Id);
                return true;
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                _logger.LogWarning("Failed to cache npm dependencies: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching npm dependencies for workspace {WorkspaceId}", workspace.Id);
            return false;
        }
    }

    private async Task<bool> CachePipDependenciesAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Caching pip dependencies for workspace {WorkspaceId}", workspace.Id);

        try
        {
            var requirementsPath = Path.Combine(workspace.RepositoryPath!, "requirements.txt");
            if (!File.Exists(requirementsPath))
            {
                _logger.LogWarning("No requirements.txt found in workspace {WorkspaceId}", workspace.Id);
                return false;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pip",
                    Arguments = $"install -r {requirementsPath}",
                    WorkingDirectory = workspace.RepositoryPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Successfully cached pip dependencies for workspace {WorkspaceId}", workspace.Id);
                return true;
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);
                _logger.LogWarning("Failed to cache pip dependencies: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching pip dependencies for workspace {WorkspaceId}", workspace.Id);
            return false;
        }
    }

    private async Task EnsureDiskSpaceAsync(CancellationToken cancellationToken)
    {
        var stats = await GetStatisticsAsync(cancellationToken);

        if (stats.DiskUsagePercent >= _cleanupThresholdPercent)
        {
            _logger.LogWarning("Disk usage at {Percent}%, triggering cleanup", stats.DiskUsagePercent);
            await CleanupOldWorkspacesAsync(cancellationToken);
        }
    }

    private async Task CleanupOldWorkspacesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up old workspaces using LRU policy");

        var workspaces = (await ListWorkspacesAsync(cancellationToken))
            .OrderBy(w => w.LastAccessedAt)
            .ToList();

        // Remove oldest 20% of workspaces
        int toRemove = Math.Max(1, workspaces.Count / 5);

        for (int i = 0; i < toRemove && i < workspaces.Count; i++)
        {
            await CleanupWorkspaceAsync(workspaces[i].Id, cancellationToken);
        }

        _logger.LogInformation("Cleaned up {Count} old workspaces", toRemove);
    }

    private long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        var directoryInfo = new DirectoryInfo(path);
        return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }

    private void InitializeWorkspaceRoot()
    {
        if (!Directory.Exists(_workspaceRoot))
        {
            Directory.CreateDirectory(_workspaceRoot);
        }

        if (!Directory.Exists(_metadataPath))
        {
            Directory.CreateDirectory(_metadataPath);
        }
    }

    private async Task SaveWorkspaceMetadataAsync(Workspace workspace, CancellationToken cancellationToken)
    {
        var metadataFile = Path.Combine(_metadataPath, $"{workspace.Id}.json");
        var json = JsonSerializer.Serialize(workspace, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataFile, json, cancellationToken);
    }

    private async Task<Workspace?> LoadWorkspaceMetadataAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var metadataFile = Path.Combine(_metadataPath, $"{workspaceId}.json");
        if (!File.Exists(metadataFile))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(metadataFile, cancellationToken);
        return JsonSerializer.Deserialize<Workspace>(json);
    }

    private void ValidateCreateWorkspaceRequest(CreateWorkspaceRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Name)) 
            throw new ValidationException("Workspace name is required");
    }
}
