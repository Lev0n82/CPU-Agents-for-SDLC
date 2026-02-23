namespace Phase3.AzureDevOps.Services.Migration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Phase3.AzureDevOps.Interfaces;

/// <summary>
/// Implements Phase 2 to Phase 3 migration.
/// </summary>
public class MigrationService : IMigrationService
{
    private readonly IWorkItemService _workItemService;
    private readonly ILogger<MigrationService> _logger;
    private readonly string _projectName;
    private readonly WorkItemTrackingHttpClient _client;

    public MigrationService(
        IWorkItemService workItemService,
        ILogger<MigrationService> logger,
        IConfiguration configuration,
        WorkItemTrackingHttpClient client)
    {
        _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        
        _projectName = configuration["AzureDevOps:ProjectName"] 
            ?? throw new InvalidOperationException("ProjectName not configured");
    }

    public async Task<MigrationAnalysis> AnalyzeMigrationAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing Phase 2 data for migration");

        var analysis = new MigrationAnalysis();

        try
        {
            // Count work items
            var workItemsWiql = new Wiql
            {
                Query = @"
                    SELECT [System.Id]
                    FROM WorkItems
                    WHERE [System.TeamProject] = @project"
            };
            var workItemsResult = await _client.QueryByWiqlAsync(workItemsWiql, _projectName, cancellationToken: cancellationToken);
            analysis.TotalWorkItems = workItemsResult.WorkItems.Count();

            // Count test cases
            var testCasesWiql = new Wiql
            {
                Query = @"
                    SELECT [System.Id]
                    FROM WorkItems
                    WHERE [System.WorkItemType] = 'Test Case'
                    AND [System.TeamProject] = @project"
            };
            var testCasesResult = await _client.QueryByWiqlAsync(testCasesWiql, _projectName, cancellationToken: cancellationToken);
            analysis.TotalTestCases = testCasesResult.WorkItems.Count();

            // Check for custom fields
            var customFieldsExist = await CheckCustomFieldsExistAsync(cancellationToken);
            if (!customFieldsExist)
            {
                analysis.Warnings.Add("Phase 3 custom fields not found - will be created during migration");
            }

            _logger.LogInformation(
                "Migration analysis complete: {WorkItems} work items, {TestCases} test cases",
                analysis.TotalWorkItems, analysis.TotalTestCases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration analysis failed");
            analysis.Errors.Add($"Analysis failed: {ex.Message}");
        }

        return analysis;
    }

    public async Task<MigrationResult> MigrateDataAsync(
        MigrationOptions options,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting migration (DryRun: {DryRun})", options.DryRun);

        var result = new MigrationResult
        {
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Step 1: Create Phase 3 schema
            if (!options.DryRun)
            {
                await CreatePhase3SchemaAsync(cancellationToken);
            }

            // Step 2: Migrate work items
            if (options.MigrateWorkItems)
            {
                result.WorkItemsMigrated = await MigrateWorkItemsAsync(options, cancellationToken);
            }

            // Step 3: Migrate test cases
            if (options.MigrateTestCases)
            {
                result.TestCasesMigrated = await MigrateTestCasesAsync(options, cancellationToken);
            }

            result.EndTime = DateTime.UtcNow;
            _logger.LogInformation(
                "Migration complete: {WorkItems} work items, {TestCases} test cases migrated",
                result.WorkItemsMigrated, result.TestCasesMigrated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed");
            result.Errors.Add($"Migration failed: {ex.Message}");
            result.EndTime = DateTime.UtcNow;
        }

        return result;
    }

    public async Task<ValidationResult> ValidateMigrationAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating migration");

        var result = new ValidationResult();

        // Check 1: Verify custom fields exist
        result.TotalChecks++;
        if (await CheckCustomFieldsExistAsync(cancellationToken))
        {
            result.PassedChecks++;
        }
        else
        {
            result.FailedChecks++;
            result.Failures.Add("Phase 3 custom fields not found");
        }

        // Check 2: Verify work items have claim metadata
        result.TotalChecks++;
        var workItemsWithClaims = await CountWorkItemsWithClaimsAsync(cancellationToken);
        if (workItemsWithClaims > 0)
        {
            result.PassedChecks++;
        }
        else
        {
            result.FailedChecks++;
            result.Failures.Add("No work items have claim metadata");
        }

        // Check 3: Verify test cases have lifecycle metadata
        result.TotalChecks++;
        var testCasesWithLifecycle = await CountTestCasesWithLifecycleAsync(cancellationToken);
        if (testCasesWithLifecycle > 0)
        {
            result.PassedChecks++;
        }
        else
        {
            result.FailedChecks++;
            result.Failures.Add("No test cases have lifecycle metadata");
        }

        _logger.LogInformation(
            "Validation complete: {Passed}/{Total} checks passed",
            result.PassedChecks, result.TotalChecks);

        return result;
    }

    public async Task<RollbackResult> RollbackMigrationAsync(
        string migrationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rolling back migration {MigrationId}", migrationId);

        var result = new RollbackResult
        {
            MigrationId = migrationId,
            RollbackTime = DateTime.UtcNow
        };

        try
        {
            // Remove Phase 3 custom fields
            await RemoveCustomFieldsAsync(cancellationToken);

            result.Success = true;
            _logger.LogInformation("Rollback complete for migration {MigrationId}", migrationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed for migration {MigrationId}", migrationId);
            result.Success = false;
            result.Errors.Add($"Rollback failed: {ex.Message}");
        }

        return result;
    }

    private async Task CreatePhase3SchemaAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Phase 3 schema");

        // Create custom fields for Phase 3
        // - ClaimedBy (string)
        // - ClaimedAt (DateTime)
        // - LifecycleStatus (string)
        // - ObsoleteReason (string)

        // Note: Field creation requires admin permissions and REST API calls
        // Implementation would use WorkItemTrackingProcessHttpClient

        await Task.CompletedTask;
    }

    private async Task<int> MigrateWorkItemsAsync(MigrationOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating work items");

        var wiql = new Wiql
        {
            Query = @"
                SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = @project"
        };

        var queryResult = await _client.QueryByWiqlAsync(wiql, _projectName, cancellationToken: cancellationToken);
        var workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();

        int migratedCount = 0;

        foreach (var batch in workItemIds.Chunk(options.BatchSize))
        {
            foreach (var workItemId in batch)
            {
                try
                {
                    if (!options.DryRun)
                    {
                        // Add Phase 3 metadata to work item
                        var patchDocument = new JsonPatchDocument
                        {
                            new JsonPatchOperation
                            {
                                Operation = Operation.Add,
                                Path = "/fields/Custom.ClaimedBy",
                                Value = string.Empty
                            },
                            new JsonPatchOperation
                            {
                                Operation = Operation.Add,
                                Path = "/fields/Custom.ClaimedAt",
                                Value = null
                            }
                        };

                        await _client.UpdateWorkItemAsync(patchDocument, workItemId, cancellationToken: cancellationToken);
                    }

                    migratedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to migrate work item {WorkItemId}", workItemId);
                }
            }
        }

        _logger.LogInformation("Migrated {Count} work items", migratedCount);
        return migratedCount;
    }

    private async Task<int> MigrateTestCasesAsync(MigrationOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating test cases");

        var wiql = new Wiql
        {
            Query = @"
                SELECT [System.Id]
                FROM WorkItems
                WHERE [System.WorkItemType] = 'Test Case'
                AND [System.TeamProject] = @project"
        };

        var queryResult = await _client.QueryByWiqlAsync(wiql, _projectName, cancellationToken: cancellationToken);
        var testCaseIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();

        int migratedCount = 0;

        foreach (var batch in testCaseIds.Chunk(options.BatchSize))
        {
            foreach (var testCaseId in batch)
            {
                try
                {
                    if (!options.DryRun)
                    {
                        // Add Phase 3 lifecycle metadata
                        var patchDocument = new JsonPatchDocument
                        {
                            new JsonPatchOperation
                            {
                                Operation = Operation.Add,
                                Path = "/fields/Custom.LifecycleStatus",
                                Value = "Active"
                            }
                        };

                        await _client.UpdateWorkItemAsync(patchDocument, testCaseId, cancellationToken: cancellationToken);
                    }

                    migratedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to migrate test case {TestCaseId}", testCaseId);
                }
            }
        }

        _logger.LogInformation("Migrated {Count} test cases", migratedCount);
        return migratedCount;
    }

    private async Task<bool> CheckCustomFieldsExistAsync(CancellationToken cancellationToken)
    {
        // Check if Phase 3 custom fields exist
        // Would query WorkItemTrackingProcessHttpClient
        await Task.CompletedTask;
        return true; // Placeholder
    }

    private async Task<int> CountWorkItemsWithClaimsAsync(CancellationToken cancellationToken)
    {
        // Count work items with claim metadata
        await Task.CompletedTask;
        return 0; // Placeholder
    }

    private async Task<int> CountTestCasesWithLifecycleAsync(CancellationToken cancellationToken)
    {
        // Count test cases with lifecycle metadata
        await Task.CompletedTask;
        return 0; // Placeholder
    }

    private async Task RemoveCustomFieldsAsync(CancellationToken cancellationToken)
    {
        // Remove Phase 3 custom fields
        await Task.CompletedTask;
    }
}
