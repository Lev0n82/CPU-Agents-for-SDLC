namespace Phase3.AzureDevOps.Services.Sync;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Phase3.AzureDevOps.Exceptions;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AzureDevOps.Models.Sync;
using System.Text.Json;

/// <summary>
/// Implements offline synchronization with conflict resolution.
/// </summary>
public class OfflineSyncService : IOfflineSyncService
{
    private readonly IWorkItemService _workItemService;
    private readonly ILogger<OfflineSyncService> _logger;
    private readonly string _cacheDbPath;
    private bool _isOfflineMode;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public OfflineSyncService(
        IWorkItemService workItemService,
        ILogger<OfflineSyncService> logger,
        IConfiguration configuration)
    {
        _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheDbPath = configuration["OfflineSync:CacheDbPath"] ?? "/data/offline_cache.db";
        
        InitializeCacheDatabase();
    }

    public async Task<bool> EnableOfflineModeAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Enabling offline mode");
            _isOfflineMode = true;
            
            // Cache current work items
            await CacheWorkItemsAsync(cancellationToken);
            
            _logger.LogInformation("Offline mode enabled");
            return true;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task<SyncResult> DisableOfflineModeAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Disabling offline mode and syncing changes");
            
            var syncResult = await SyncChangesAsync(cancellationToken);
            _isOfflineMode = false;
            
            _logger.LogInformation("Offline mode disabled. Synced {SuccessCount} operations, {ConflictCount} conflicts",
                syncResult.SuccessCount, syncResult.Conflicts.Count);
            
            return syncResult;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task<SyncResult> SyncChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing offline changes");

        var result = new SyncResult
        {
            StartTime = DateTime.UtcNow
        };

        await using var connection = new SqliteConnection($"Data Source={_cacheDbPath}");
        await connection.OpenAsync(cancellationToken);

        // Get pending operations
        var pendingOps = await GetPendingOperationsAsync(connection, cancellationToken);
        _logger.LogInformation("Found {Count} pending operations", pendingOps.Count);

        foreach (var op in pendingOps)
        {
            try
            {
                await SyncOperationAsync(op, result, cancellationToken);
                await MarkOperationSyncedAsync(connection, op.Id, cancellationToken);
                result.SuccessCount++;
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Conflict detected for operation {OperationId}", op.Id);
                var conflict = await CreateConflictAsync(connection, op, ex, cancellationToken);
                result.Conflicts.Add(conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync operation {OperationId}", op.Id);
                result.FailureCount++;
            }
        }

        result.EndTime = DateTime.UtcNow;
        
        // Update last sync time
        await UpdateLastSyncTimeAsync(connection, result.EndTime, cancellationToken);
        
        _logger.LogInformation("Sync completed: {SuccessCount} succeeded, {ConflictCount} conflicts, {FailureCount} failed",
            result.SuccessCount, result.Conflicts.Count, result.FailureCount);

        return result;
    }

    public async Task<IEnumerable<Conflict>> GetConflictsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving conflicts");

        await using var connection = new SqliteConnection($"Data Source={_cacheDbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkItemId, LocalVersion, RemoteVersion, BaseVersion, ConflictType, CreatedAt
            FROM Conflicts
            WHERE Status = 'Pending'
            ORDER BY CreatedAt DESC";

        var conflicts = new List<Conflict>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            conflicts.Add(new Conflict
            {
                Id = Guid.Parse(reader.GetString(0)),
                WorkItemId = reader.GetInt32(1),
                LocalVersion = JsonSerializer.Deserialize<WorkItemData>(reader.GetString(2)),
                RemoteVersion = JsonSerializer.Deserialize<WorkItemData>(reader.GetString(3)),
                BaseVersion = reader.IsDBNull(4) ? null : JsonSerializer.Deserialize<WorkItemData>(reader.GetString(4)),
                ConflictType = Enum.Parse<ConflictType>(reader.GetString(5)),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            });
        }

        _logger.LogInformation("Retrieved {Count} conflicts", conflicts.Count);
        return conflicts;
    }

    public async Task<bool> ResolveConflictAsync(
        Guid conflictId, 
        ConflictResolutionPolicy policy, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resolving conflict {ConflictId} with policy {Policy}", conflictId, policy);

        await using var connection = new SqliteConnection($"Data Source={_cacheDbPath}");
        await connection.OpenAsync(cancellationToken);

        var conflict = await GetConflictByIdAsync(connection, conflictId, cancellationToken);
        if (conflict == null)
        {
            _logger.LogWarning("Conflict {ConflictId} not found", conflictId);
            return false;
        }

        bool resolved = policy switch
        {
            ConflictResolutionPolicy.Abort => await ResolveWithAbortAsync(conflict, cancellationToken),
            ConflictResolutionPolicy.Merge => await ResolveWithMergeAsync(conflict, cancellationToken),
            ConflictResolutionPolicy.ManualReview => await ResolveWithManualReviewAsync(conflict, cancellationToken),
            ConflictResolutionPolicy.ForceOverwrite => await ResolveWithForceOverwriteAsync(conflict, cancellationToken),
            _ => throw new ArgumentException($"Unknown policy: {policy}")
        };

        if (resolved)
        {
            await MarkConflictResolvedAsync(connection, conflictId, policy, cancellationToken);
            _logger.LogInformation("Conflict {ConflictId} resolved with policy {Policy}", conflictId, policy);
        }

        return resolved;
    }

    public async Task<OfflineStatus> GetOfflineStatusAsync()
    {
        await using var connection = new SqliteConnection($"Data Source={_cacheDbPath}");
        await connection.OpenAsync();

        var status = new OfflineStatus
        {
            IsOfflineMode = _isOfflineMode,
            PendingOperations = await GetPendingOperationCountAsync(connection),
            PendingConflicts = await GetPendingConflictCountAsync(connection),
            LastSyncTime = await GetLastSyncTimeAsync(connection)
        };

        return status;
    }

    private async Task SyncOperationAsync(PendingOperation op, SyncResult result, CancellationToken cancellationToken)
    {
        switch (op.OperationType)
        {
            case "Create":
                var createFields = JsonSerializer.Deserialize<Dictionary<string, object>>(op.Payload);
                if (createFields != null)
                {
                    await _workItemService.CreateWorkItemAsync("Task", createFields, cancellationToken);
                }
                break;

            case "Update":
                var updateFields = JsonSerializer.Deserialize<Dictionary<string, object>>(op.Payload);
                if (updateFields != null)
                {
                    // Check for conflicts using ETag
                    var remoteWorkItem = await _workItemService.GetWorkItemAsync(op.WorkItemId, cancellationToken);
                    if (remoteWorkItem.Rev != op.BaseRevision)
                    {
                        throw new ConflictException($"Work item {op.WorkItemId} was modified remotely (base: {op.BaseRevision}, remote: {remoteWorkItem.Rev})");
                    }
                    
                    await _workItemService.UpdateWorkItemAsync(op.WorkItemId, op.BaseRevision, updateFields, cancellationToken);
                }
                break;

            case "Delete":
                // Delete operation - not implemented in IWorkItemService yet
                _logger.LogWarning("Delete operation not yet supported for work item {WorkItemId}", op.WorkItemId);
                break;

            default:
                throw new InvalidOperationException($"Unknown operation type: {op.OperationType}");
        }
    }

    private async Task<bool> ResolveWithAbortAsync(Conflict conflict, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resolving conflict {ConflictId} with Abort policy (keeping remote version)", conflict.Id);
        // Discard local changes, keep remote version
        return true;
    }

    private async Task<bool> ResolveWithMergeAsync(Conflict conflict, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resolving conflict {ConflictId} with Merge policy (3-way merge)", conflict.Id);

        if (conflict.BaseVersion == null || conflict.LocalVersion == null || conflict.RemoteVersion == null)
        {
            _logger.LogError("Cannot perform 3-way merge without base, local, and remote versions");
            return false;
        }

        // Perform 3-way merge (base, local, remote)
        var mergedVersion = PerformThreeWayMerge(conflict.BaseVersion, conflict.LocalVersion, conflict.RemoteVersion);

        // Apply merged version
        await _workItemService.UpdateWorkItemAsync(
            conflict.WorkItemId, 
            conflict.RemoteVersion.Rev, 
            mergedVersion.Fields, 
            cancellationToken);
        
        return true;
    }

    private async Task<bool> ResolveWithManualReviewAsync(Conflict conflict, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resolving conflict {ConflictId} with ManualReview policy (queuing for review)", conflict.Id);
        // Queue conflict for manual review (no automatic resolution)
        return false; // Not resolved yet
    }

    private async Task<bool> ResolveWithForceOverwriteAsync(Conflict conflict, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resolving conflict {ConflictId} with ForceOverwrite policy (using local version)", conflict.Id);

        if (conflict.LocalVersion == null || conflict.RemoteVersion == null)
        {
            _logger.LogError("Cannot force overwrite without local and remote versions");
            return false;
        }

        // Overwrite remote with local version
        await _workItemService.UpdateWorkItemAsync(
            conflict.WorkItemId,
            conflict.RemoteVersion.Rev,
            conflict.LocalVersion.Fields,
            cancellationToken);
        
        return true;
    }

    private WorkItemData PerformThreeWayMerge(WorkItemData baseVersion, WorkItemData localVersion, WorkItemData remoteVersion)
    {
        var merged = new WorkItemData
        {
            Id = localVersion.Id,
            Rev = remoteVersion.Rev,
            Fields = new Dictionary<string, object>()
        };

        // Get all field names
        var allFields = baseVersion.Fields.Keys
            .Union(localVersion.Fields.Keys)
            .Union(remoteVersion.Fields.Keys)
            .ToHashSet();

        foreach (var field in allFields)
        {
            var baseValue = baseVersion.Fields.GetValueOrDefault(field);
            var localValue = localVersion.Fields.GetValueOrDefault(field);
            var remoteValue = remoteVersion.Fields.GetValueOrDefault(field);

            // 3-way merge logic
            if (Equals(localValue, remoteValue))
            {
                // Both changed to same value or both unchanged
                merged.Fields[field] = localValue ?? remoteValue ?? baseValue;
            }
            else if (Equals(localValue, baseValue))
            {
                // Local unchanged, remote changed -> use remote
                merged.Fields[field] = remoteValue ?? baseValue;
            }
            else if (Equals(remoteValue, baseValue))
            {
                // Remote unchanged, local changed -> use local
                merged.Fields[field] = localValue ?? baseValue;
            }
            else
            {
                // Both changed to different values -> conflict (use local by default)
                merged.Fields[field] = localValue ?? baseValue;
                _logger.LogWarning("Merge conflict on field {Field}: base={Base}, local={Local}, remote={Remote}. Using local.",
                    field, baseValue, localValue, remoteValue);
            }
        }

        return merged;
    }

    private void InitializeCacheDatabase()
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_cacheDbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = new SqliteConnection($"Data Source={_cacheDbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS PendingOperations (
                Id TEXT PRIMARY KEY,
                WorkItemId INTEGER,
                OperationType TEXT NOT NULL,
                Payload TEXT NOT NULL,
                BaseRevision INTEGER,
                CreatedAt TEXT NOT NULL,
                Synced INTEGER DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Conflicts (
                Id TEXT PRIMARY KEY,
                WorkItemId INTEGER NOT NULL,
                LocalVersion TEXT NOT NULL,
                RemoteVersion TEXT NOT NULL,
                BaseVersion TEXT,
                ConflictType TEXT NOT NULL,
                Status TEXT DEFAULT 'Pending',
                ResolutionPolicy TEXT,
                CreatedAt TEXT NOT NULL,
                ResolvedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS CachedWorkItems (
                Id INTEGER PRIMARY KEY,
                Data TEXT NOT NULL,
                CachedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS SyncMetadata (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );";

        command.ExecuteNonQuery();
    }

    private async Task CacheWorkItemsAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement work item caching
        _logger.LogInformation("Caching work items for offline access");
        await Task.CompletedTask;
    }

    private async Task<List<PendingOperation>> GetPendingOperationsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkItemId, OperationType, Payload, BaseRevision, CreatedAt
            FROM PendingOperations
            WHERE Synced = 0
            ORDER BY CreatedAt ASC";

        var operations = new List<PendingOperation>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            operations.Add(new PendingOperation
            {
                Id = Guid.Parse(reader.GetString(0)),
                WorkItemId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                OperationType = reader.GetString(2),
                Payload = reader.GetString(3),
                BaseRevision = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                CreatedAt = DateTime.Parse(reader.GetString(5))
            });
        }

        return operations;
    }

    private async Task MarkOperationSyncedAsync(SqliteConnection connection, Guid operationId, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = "UPDATE PendingOperations SET Synced = 1 WHERE Id = @id";
        command.Parameters.AddWithValue("@id", operationId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<Conflict> CreateConflictAsync(SqliteConnection connection, PendingOperation op, ConflictException ex, CancellationToken cancellationToken)
    {
        var conflict = new Conflict
        {
            Id = Guid.NewGuid(),
            WorkItemId = op.WorkItemId,
            LocalVersion = JsonSerializer.Deserialize<WorkItemData>(op.Payload),
            RemoteVersion = null, // TODO: Fetch remote version
            ConflictType = ConflictType.ModifyConflict,
            CreatedAt = DateTime.UtcNow
        };

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Conflicts (Id, WorkItemId, LocalVersion, RemoteVersion, ConflictType, CreatedAt)
            VALUES (@id, @workItemId, @local, @remote, @type, @createdAt)";
        
        command.Parameters.AddWithValue("@id", conflict.Id.ToString());
        command.Parameters.AddWithValue("@workItemId", conflict.WorkItemId);
        command.Parameters.AddWithValue("@local", JsonSerializer.Serialize(conflict.LocalVersion));
        command.Parameters.AddWithValue("@remote", "{}"); // Placeholder
        command.Parameters.AddWithValue("@type", conflict.ConflictType.ToString());
        command.Parameters.AddWithValue("@createdAt", conflict.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);

        return conflict;
    }

    private async Task<Conflict?> GetConflictByIdAsync(SqliteConnection connection, Guid conflictId, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, WorkItemId, LocalVersion, RemoteVersion, BaseVersion, ConflictType, CreatedAt
            FROM Conflicts
            WHERE Id = @id AND Status = 'Pending'";
        
        command.Parameters.AddWithValue("@id", conflictId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new Conflict
            {
                Id = Guid.Parse(reader.GetString(0)),
                WorkItemId = reader.GetInt32(1),
                LocalVersion = JsonSerializer.Deserialize<WorkItemData>(reader.GetString(2)),
                RemoteVersion = JsonSerializer.Deserialize<WorkItemData>(reader.GetString(3)),
                BaseVersion = reader.IsDBNull(4) ? null : JsonSerializer.Deserialize<WorkItemData>(reader.GetString(4)),
                ConflictType = Enum.Parse<ConflictType>(reader.GetString(5)),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            };
        }

        return null;
    }

    private async Task MarkConflictResolvedAsync(SqliteConnection connection, Guid conflictId, ConflictResolutionPolicy policy, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Conflicts 
            SET Status = 'Resolved', ResolutionPolicy = @policy, ResolvedAt = @resolvedAt
            WHERE Id = @id";
        
        command.Parameters.AddWithValue("@id", conflictId.ToString());
        command.Parameters.AddWithValue("@policy", policy.ToString());
        command.Parameters.AddWithValue("@resolvedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<int> GetPendingOperationCountAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM PendingOperations WHERE Synced = 0";
        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    private async Task<int> GetPendingConflictCountAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Conflicts WHERE Status = 'Pending'";
        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    private async Task<DateTime?> GetLastSyncTimeAsync(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM SyncMetadata WHERE Key = 'LastSyncTime'";
        var result = await command.ExecuteScalarAsync();
        return result != null ? DateTime.Parse(result.ToString()!) : null;
    }

    private async Task UpdateLastSyncTimeAsync(SqliteConnection connection, DateTime syncTime, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO SyncMetadata (Key, Value)
            VALUES ('LastSyncTime', @syncTime)";
        command.Parameters.AddWithValue("@syncTime", syncTime.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
