using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phase3.AzureDevOps.Interfaces;
using Phase3.AgentHost.Models;
using System.Diagnostics;

namespace Phase3.AgentHost.Services;

/// <summary>
/// Main agent host service that implements the polling loop and work item processing
/// </summary>
public class AgentHostService : BackgroundService
{
    private readonly ILogger<AgentHostService> _logger;
    private readonly AgentConfiguration _config;
    private readonly IWorkItemService _workItemService;
    private readonly IWorkItemCoordinator _coordinator;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ITelemetryService _telemetry;
    private readonly IResiliencePolicy _resilience;
    private readonly SemaphoreSlim _concurrencySemaphore;

    public AgentHostService(
        ILogger<AgentHostService> logger,
        IOptions<AgentConfiguration> config,
        IWorkItemService workItemService,
        IWorkItemCoordinator coordinator,
        IWorkflowEngine workflowEngine,
        ITelemetryService telemetry,
        IResiliencePolicy resilience)
    {
        _logger = logger;
        _config = config.Value;
        _workItemService = workItemService;
        _coordinator = coordinator;
        _workflowEngine = workflowEngine;
        _telemetry = telemetry;
        _resilience = resilience;
        _concurrencySemaphore = new SemaphoreSlim(_config.MaxConcurrentWorkItems, _config.MaxConcurrentWorkItems);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CPU Agent {AgentName} starting...", _config.Name);
        _logger.LogInformation("Polling interval: {Interval} seconds", _config.PollingIntervalSeconds);
        _logger.LogInformation("Max concurrent work items: {MaxConcurrent}", _config.MaxConcurrentWorkItems);

        // Wait a bit for services to initialize
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessWorkItemsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Agent shutdown requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in agent polling loop");
                _telemetry.RecordException(ex, new Dictionary<string, object>
                {
                    ["agent"] = _config.Name,
                    ["error_type"] = "polling_loop_error"
                });
            }

            // Wait for next polling interval
            await Task.Delay(TimeSpan.FromSeconds(_config.PollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("CPU Agent {AgentName} stopped", _config.Name);
    }

    private async Task ProcessWorkItemsAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var activity = _telemetry.StartActivity("ProcessWorkItems", ActivityKind.Internal);
        
        try
        {
            // Query for available work items
            _logger.LogDebug("Querying for available work items...");
            var workItems = await _resilience.ExecuteAsync(
                async (ct) => await _workItemService.QueryWorkItemsAsync(_config.WorkItemQueryWiql, ct),
                cancellationToken);

            if (workItems == null || workItems.Count() == 0)
            {
                _logger.LogDebug("No work items found");
                _telemetry.RecordMetric("work_items_found", 0, null);
                return;
            }

            _logger.LogInformation("Found {Count} work items to process", workItems.Count());
            _telemetry.RecordMetric("work_items_found", workItems.Count(), null);

            // Process work items concurrently (up to MaxConcurrentWorkItems)
            var tasks = new List<Task>();
            
            foreach (var workItem in workItems)
            {
                // Check if we can process more work items
                await _concurrencySemaphore.WaitAsync(cancellationToken);
                
                // Process work item in background task
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessSingleWorkItemAsync(workItem.Id ?? 0, workItem.Rev ?? 0, cancellationToken);
                    }
                    finally
                    {
                        _concurrencySemaphore.Release();
                    }
                }, cancellationToken);
                
                tasks.Add(task);
            }

            // Wait for all work items to complete
            await Task.WhenAll(tasks);
            
            stopwatch.Stop();
            _logger.LogInformation("Processed {Count} work items in {Duration}ms", 
                workItems.Count(), stopwatch.ElapsedMilliseconds);
            _telemetry.RecordMetric("work_items_processing_duration_ms", stopwatch.ElapsedMilliseconds, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing work items");
            _telemetry.RecordException(ex, new Dictionary<string, object>
            {
                ["agent"] = _config.Name,
                ["error_type"] = "work_items_processing_error"
            });
            throw;
        }
    }

    private async Task ProcessSingleWorkItemAsync(int workItemId, int revision, CancellationToken cancellationToken)
    {
        using var activity = _telemetry.StartActivity("ProcessSingleWorkItem", ActivityKind.Internal);
        activity?.SetTag("work_item_id", workItemId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing work item {WorkItemId}", workItemId);

            // Step 1: Claim the work item
            _logger.LogDebug("Claiming work item {WorkItemId}", workItemId);
            var claimed = await _coordinator.TryClaimWorkItemAsync(workItemId, revision, _config.Name, cancellationToken);
            
            if (!claimed)
            {
                _logger.LogWarning("Failed to claim work item {WorkItemId} - already claimed by another agent", workItemId);
                _telemetry.RecordMetric("work_item_claim_failed", 1, null);
                return;
            }

            _logger.LogInformation("Successfully claimed work item {WorkItemId}", workItemId);
            _telemetry.RecordMetric("work_item_claimed", 1, null);

            try
            {
                // Step 2: Get full work item details
                var workItem = await _workItemService.GetWorkItemAsync(workItemId, cancellationToken);
                if (workItem == null)
                {
                    _logger.LogWarning("Work item {WorkItemId} not found", workItemId);
                    return;
                }

                var workItemType = workItem.Fields.ContainsKey("System.WorkItemType") 
                    ? workItem.Fields["System.WorkItemType"]?.ToString() 
                    : "Unknown";
                
                _logger.LogInformation("Work item {WorkItemId} type: {WorkItemType}", workItemId, workItemType);

                // Step 3: Select and execute workflow
                var workflow = await _workflowEngine.SelectWorkflowAsync(workItem);
                if (workflow == null)
                {
                    _logger.LogWarning("No workflow found for work item {WorkItemId} type {WorkItemType}", 
                        workItemId, workItemType);
                    _telemetry.RecordMetric("workflow_not_found", 1, null);
                    
                    // Update work item with comment
                    var historyFields = new Dictionary<string, object>
                    {
                        ["System.History"] = $"[CPU Agent {_config.Name}] No workflow configured for work item type '{workItemType}'"
                    };
                    await _workItemService.UpdateWorkItemAsync(workItemId, workItem.Rev ?? 0, historyFields, cancellationToken);
                    
                    return;
                }

                _logger.LogInformation("Executing workflow '{WorkflowName}' for work item {WorkItemId}", 
                    workflow.Name, workItemId);

                // Step 4: Execute workflow
                var context = new WorkflowContext
                {
                    WorkItemId = workItemId,
                    WorkItem = workItem,
                    AgentName = _config.Name,
                    Variables = new Dictionary<string, object>()
                };

                var result = await _workflowEngine.ExecuteWorkflowAsync(workflow, context, cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation("Workflow '{WorkflowName}' completed successfully for work item {WorkItemId}", 
                        workflow.Name, workItemId);
                    _telemetry.RecordMetric("workflow_success", 1, null);
                    
                    // Update work item with success
                    var successFields = new Dictionary<string, object>
                    {
                        ["System.History"] = $"[CPU Agent {_config.Name}] Workflow '{workflow.Name}' completed successfully. {result.Message}"
                    };
                    await _workItemService.UpdateWorkItemAsync(workItemId, workItem.Rev ?? 0, successFields, cancellationToken);
                }
                else
                {
                    _logger.LogError("Workflow '{WorkflowName}' failed for work item {WorkItemId}: {Error}", 
                        workflow.Name, workItemId, result.Message);
                    _telemetry.RecordMetric("workflow_failed", 1, null);
                    
                    // Update work item with failure
                    var failureFields = new Dictionary<string, object>
                    {
                        ["System.History"] = $"[CPU Agent {_config.Name}] Workflow '{workflow.Name}' failed: {result.Message}"
                    };
                    await _workItemService.UpdateWorkItemAsync(workItemId, workItem.Rev ?? 0, failureFields, cancellationToken);
                }
            }
            finally
            {
                // Step 5: Release the work item claim
                _logger.LogDebug("Releasing work item {WorkItemId}", workItemId);
                await _coordinator.ReleaseWorkItemAsync(workItemId, revision, _config.Name, cancellationToken);
                _logger.LogInformation("Released work item {WorkItemId}", workItemId);
            }

            stopwatch.Stop();
            _logger.LogInformation("Completed processing work item {WorkItemId} in {Duration}ms", 
                workItemId, stopwatch.ElapsedMilliseconds);
            _telemetry.RecordMetric("work_item_processing_duration_ms", stopwatch.ElapsedMilliseconds, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing work item {WorkItemId}", workItemId);
            _telemetry.RecordException(ex, new Dictionary<string, object>
            {
                ["work_item_id"] = workItemId,
                ["error_type"] = "work_item_processing_error"
            });
            
            // Try to release claim on error
            try
            {
                await _coordinator.ReleaseWorkItemAsync(workItemId, revision, _config.Name, cancellationToken);
            }
            catch (Exception releaseEx)
            {
                _logger.LogError(releaseEx, "Failed to release work item {WorkItemId} after error", workItemId);
            }
            
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CPU Agent {AgentName} stopping gracefully...", _config.Name);
        
        // Wait for all work items to complete (with timeout)
        var timeout = TimeSpan.FromSeconds(30);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        
        try
        {
            // Wait for semaphore to be fully released (all work items completed)
            for (int i = 0; i < _config.MaxConcurrentWorkItems; i++)
            {
                await _concurrencySemaphore.WaitAsync(cts.Token);
            }
            
            _logger.LogInformation("All work items completed gracefully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Graceful shutdown timeout - some work items may not have completed");
        }
        
        await base.StopAsync(cancellationToken);
    }
}
