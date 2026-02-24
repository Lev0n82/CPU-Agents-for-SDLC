namespace Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Provides migration operations from Phase 2 to Phase 3.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Analyzes Phase 2 data for migration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration analysis result.</returns>
    Task<MigrationAnalysis> AnalyzeMigrationAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Migrates data from Phase 2 to Phase 3.
    /// </summary>
    /// <param name="options">Migration options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result.</returns>
    Task<MigrationResult> MigrateDataAsync(
        MigrationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates migration results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<ValidationResult> ValidateMigrationAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back migration if needed.
    /// </summary>
    /// <param name="migrationId">Migration ID to rollback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rollback result.</returns>
    Task<RollbackResult> RollbackMigrationAsync(
        string migrationId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Migration analysis result.
/// </summary>
public class MigrationAnalysis
{
    public int TotalWorkItems { get; set; }
    public int TotalTestCases { get; set; }
    public int TotalGitRepos { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool CanMigrate => !Errors.Any();
}

/// <summary>
/// Migration options.
/// </summary>
public class MigrationOptions
{
    public bool DryRun { get; set; } = false;
    public bool MigrateWorkItems { get; set; } = true;
    public bool MigrateTestCases { get; set; } = true;
    public bool MigrateGitRepos { get; set; } = false;
    public int BatchSize { get; set; } = 100;
}

/// <summary>
/// Migration result.
/// </summary>
public class MigrationResult
{
    public string MigrationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int WorkItemsMigrated { get; set; }
    public int TestCasesMigrated { get; set; }
    public int GitReposMigrated { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success => !Errors.Any();
}

/// <summary>
/// Validation result.
/// </summary>
public class ValidationResult
{
    public int TotalChecks { get; set; }
    public int PassedChecks { get; set; }
    public int FailedChecks { get; set; }
    public List<string> Failures { get; set; } = new();
    public bool IsValid => FailedChecks == 0;
}

/// <summary>
/// Rollback result.
/// </summary>
public class RollbackResult
{
    public string MigrationId { get; set; } = string.Empty;
    public DateTime RollbackTime { get; set; }
    public int ItemsRolledBack { get; set; }
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}
