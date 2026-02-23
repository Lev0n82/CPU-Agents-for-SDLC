using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Phase3.AgentHost.Models;
using Phase3.AzureDevOps.Interfaces;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Phase3.AgentHost.Services;

/// <summary>
/// Workflow engine implementation
/// </summary>
public class WorkflowEngine : IWorkflowEngine
{
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly IWorkflowRepository _repository;
    private readonly ITelemetryService _telemetry;
    private readonly Dictionary<string, Func<WorkflowStep, WorkflowContext, CancellationToken, Task<ActionResult>>> _actionRegistry;
    private List<Workflow> _workflows = new();
    private readonly SemaphoreSlim _workflowsLock = new(1, 1);

    // Injected services for actions
    private readonly IWorkItemService _workItemService;
    private readonly ITestPlanService _testPlanService;
    private readonly IGitService _gitService;
    private readonly IGitWorkspaceManager _workspaceManager;
    private readonly IAIDecisionService _aiService;

    public WorkflowEngine(
        ILogger<WorkflowEngine> logger,
        IWorkflowRepository repository,
        ITelemetryService telemetry,
        IWorkItemService workItemService,
        ITestPlanService testPlanService,
        IGitService gitService,
        IGitWorkspaceManager workspaceManager,
        IAIDecisionService aiService)
    {
        _logger = logger;
        _repository = repository;
        _telemetry = telemetry;
        _workItemService = workItemService;
        _testPlanService = testPlanService;
        _gitService = gitService;
        _workspaceManager = workspaceManager;
        _aiService = aiService;

        // Initialize action registry
        _actionRegistry = new Dictionary<string, Func<WorkflowStep, WorkflowContext, CancellationToken, Task<ActionResult>>>
        {
            ["log"] = ExecuteLogAction,
            ["update_work_item"] = ExecuteUpdateWorkItemAction,
            ["create_test_case"] = ExecuteCreateTestCaseAction,
            ["run_test_case"] = ExecuteRunTestCaseAction,
            ["git_clone"] = ExecuteGitCloneAction,
            ["git_commit"] = ExecuteGitCommitAction,
            ["git_push"] = ExecuteGitPushAction,
            ["ai_code_review"] = ExecuteAICodeReviewAction,
            ["ai_root_cause_analysis"] = ExecuteAIRootCauseAnalysisAction,
            ["set_variable"] = ExecuteSetVariableAction,
            ["delay"] = ExecuteDelayAction
        };

        // Load workflows asynchronously
        _ = LoadWorkflowsAsync();
    }

    private async Task LoadWorkflowsAsync()
    {
        try
        {
            await _workflowsLock.WaitAsync();
            _workflows = await _repository.LoadAllWorkflowsAsync();
            _logger.LogInformation("Loaded {Count} workflows", _workflows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load workflows");
        }
        finally
        {
            _workflowsLock.Release();
        }
    }

    public async Task<Workflow?> SelectWorkflowAsync(WorkItem workItem)
    {
        await _workflowsLock.WaitAsync();
        try
        {
            var workItemType = workItem.Fields.ContainsKey("System.WorkItemType")
                ? workItem.Fields["System.WorkItemType"]?.ToString()
                : null;

            if (string.IsNullOrEmpty(workItemType))
            {
                _logger.LogWarning("Work item {WorkItemId} has no type", workItem.Id);
                return null;
            }

            // Find workflows applicable to this work item type
            var applicableWorkflows = _workflows
                .Where(w => w.ApplicableWorkItemTypes.Contains(workItemType, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (applicableWorkflows.Count == 0)
            {
                _logger.LogDebug("No workflows found for work item type: {WorkItemType}", workItemType);
                return null;
            }

            // Check conditions for each workflow
            foreach (var workflow in applicableWorkflows)
            {
                if (await EvaluateConditionsAsync(workflow, workItem))
                {
                    _logger.LogInformation("Selected workflow '{WorkflowName}' for work item {WorkItemId}",
                        workflow.Name, workItem.Id);
                    return workflow;
                }
            }

            // Return first applicable workflow if no conditions matched
            var defaultWorkflow = applicableWorkflows.First();
            _logger.LogInformation("Selected default workflow '{WorkflowName}' for work item {WorkItemId}",
                defaultWorkflow.Name, workItem.Id);
            return defaultWorkflow;
        }
        finally
        {
            _workflowsLock.Release();
        }
    }

    private async Task<bool> EvaluateConditionsAsync(Workflow workflow, WorkItem workItem)
    {
        if (workflow.Conditions == null || workflow.Conditions.Count == 0)
        {
            return true; // No conditions means always applicable
        }

        foreach (var condition in workflow.Conditions)
        {
            var fieldName = condition.Key;
            var expectedValue = condition.Value;

            if (!workItem.Fields.ContainsKey(fieldName))
            {
                return false;
            }

            var actualValue = workItem.Fields[fieldName]?.ToString();
            if (!string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return await Task.FromResult(true);
    }

    public async Task<WorkflowResult> ExecuteWorkflowAsync(Workflow workflow, WorkflowContext context, CancellationToken cancellationToken = default)
    {
        using var activity = _telemetry.StartActivity("ExecuteWorkflow", ActivityKind.Internal);
        activity?.SetTag("workflow_id", workflow.Id);
        activity?.SetTag("workflow_name", workflow.Name);
        activity?.SetTag("work_item_id", context.WorkItemId);

        var stopwatch = Stopwatch.StartNew();
        var result = new WorkflowResult
        {
            Success = true,
            ExecutionLog = new List<string>()
        };

        try
        {
            _logger.LogInformation("Executing workflow '{WorkflowName}' for work item {WorkItemId}",
                workflow.Name, context.WorkItemId);

            // Initialize variables with defaults
            foreach (var variable in workflow.DefaultVariables)
            {
                if (!context.Variables.ContainsKey(variable.Key))
                {
                    context.Variables[variable.Key] = variable.Value;
                }
            }

            // Execute each step
            for (int i = 0; i < workflow.Steps.Count; i++)
            {
                var step = workflow.Steps[i];
                
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Success = false;
                    result.Message = "Workflow execution cancelled";
                    break;
                }

                _logger.LogInformation("Executing step {StepNumber}/{TotalSteps}: {StepName}",
                    i + 1, workflow.Steps.Count, step.Name);
                context.ExecutionLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Executing step: {step.Name}");

                var stepResult = await ExecuteStepAsync(step, context, cancellationToken);

                if (stepResult.Success)
                {
                    _logger.LogInformation("Step '{StepName}' completed successfully", step.Name);
                    context.ExecutionLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Step '{step.Name}' completed: {stepResult.Message}");
                    
                    // Store step outputs
                    context.StepOutputs[step.Id] = stepResult.Outputs;
                }
                else
                {
                    _logger.LogWarning("Step '{StepName}' failed: {Error}", step.Name, stepResult.Message);
                    context.ExecutionLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Step '{step.Name}' failed: {stepResult.Message}");

                    // Check if we should continue on error
                    if (step.ContinueOnError?.ToLower() != "true")
                    {
                        result.Success = false;
                        result.Message = $"Step '{step.Name}' failed: {stepResult.Message}";
                        result.Error = stepResult.Error;
                        break;
                    }
                }
            }

            stopwatch.Stop();

            if (result.Success)
            {
                result.Message = $"Workflow completed successfully in {stopwatch.ElapsedMilliseconds}ms";
                _logger.LogInformation("Workflow '{WorkflowName}' completed successfully in {Duration}ms",
                    workflow.Name, stopwatch.ElapsedMilliseconds);
                _telemetry.RecordMetric("workflow_duration_ms", stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogError("Workflow '{WorkflowName}' failed: {Error}",
                    workflow.Name, result.Message);
                _telemetry.RecordMetric("workflow_failed", 1);
            }

            result.ExecutionLog = context.ExecutionLog;
            result.Outputs = context.StepOutputs.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow '{WorkflowName}'", workflow.Name);
            result.Success = false;
            result.Message = $"Workflow execution error: {ex.Message}";
            result.Error = ex;
            _telemetry.RecordException(ex, new Dictionary<string, object>
            {
                ["workflow_id"] = workflow.Id,
                ["work_item_id"] = context.WorkItemId
            });
        }

        return result;
    }

    private async Task<ActionResult> ExecuteStepAsync(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Check if action exists in registry
            if (!_actionRegistry.ContainsKey(step.Action))
            {
                return new ActionResult
                {
                    Success = false,
                    Message = $"Unknown action: {step.Action}"
                };
            }

            // Execute action with retry if configured
            var retryCount = step.RetryCount > 0 ? step.RetryCount : 1;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    if (step.TimeoutSeconds > 0)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(step.TimeoutSeconds));
                    }

                    var actionFunc = _actionRegistry[step.Action];
                    var result = await actionFunc(step, context, cts.Token);

                    if (result.Success || attempt == retryCount)
                    {
                        return result;
                    }

                    _logger.LogWarning("Action '{Action}' failed (attempt {Attempt}/{MaxAttempts}): {Error}",
                        step.Action, attempt, retryCount, result.Message);
                    lastException = result.Error;

                    // Wait before retry
                    if (attempt < retryCount)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw; // Propagate cancellation
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Action '{Action}' threw exception (attempt {Attempt}/{MaxAttempts})",
                        step.Action, attempt, retryCount);

                    if (attempt == retryCount)
                    {
                        return new ActionResult
                        {
                            Success = false,
                            Message = $"Action failed after {retryCount} attempts: {ex.Message}",
                            Error = ex
                        };
                    }

                    // Wait before retry
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }

            return new ActionResult
            {
                Success = false,
                Message = $"Action failed after {retryCount} attempts",
                Error = lastException
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step '{StepName}'", step.Name);
            return new ActionResult
            {
                Success = false,
                Message = $"Step execution error: {ex.Message}",
                Error = ex
            };
        }
    }

    // Action implementations

    private Task<ActionResult> ExecuteLogAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        var message = GetParameterValue<string>(step, "message", context) ?? "No message";
        _logger.LogInformation("[Workflow Log] {Message}", message);
        
        return Task.FromResult(new ActionResult
        {
            Success = true,
            Message = message
        });
    }

    private async Task<ActionResult> ExecuteUpdateWorkItemAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var fields = GetParameterValue<Dictionary<string, object>>(step, "fields", context);
            if (fields == null || fields.Count == 0)
            {
                return new ActionResult
                {
                    Success = false,
                    Message = "No fields specified for update"
                };
            }

            // Replace variable placeholders in field values
            var processedFields = new Dictionary<string, object>();
            foreach (var field in fields)
            {
                processedFields[field.Key] = ReplaceVariables(field.Value?.ToString() ?? "", context);
            }

            // Get current work item revision
            var workItem = await _workItemService.GetWorkItemAsync(context.WorkItemId, CancellationToken.None);
            await _workItemService.UpdateWorkItemAsync(context.WorkItemId, workItem.Rev ?? 0, processedFields, CancellationToken.None);

            return new ActionResult
            {
                Success = true,
                Message = $"Updated {fields.Count} fields"
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"Failed to update work item: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteCreateTestCaseAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var title = GetParameterValue<string>(step, "title", context) ?? "Test Case";
            var testPlanId = GetParameterValue<int>(step, "testPlanId", context);
            var requirementId = GetParameterValue<int>(step, "requirementId", context);

            var testCase = await _testPlanService.CreateTestCaseAsync(new Phase3.AzureDevOps.Models.TestPlans.CreateTestCaseRequest
            {
                Title = ReplaceVariables(title, context),
                Description = "",
                RequirementId = requirementId,
                Steps = new List<Phase3.AzureDevOps.Models.TestPlans.TestStep>()
            }, CancellationToken.None);

            return new ActionResult
            {
                Success = true,
                Message = $"Created test case {testCase}",
                Outputs = new Dictionary<string, object>
                {
                    ["testCaseId"] = testCase
                }
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"Failed to create test case: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteRunTestCaseAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var testCaseId = GetParameterValue<int>(step, "testCaseId", context);
            var testPlanId = GetParameterValue<int>(step, "testPlanId", context);
            var outcome = GetParameterValue<string>(step, "outcome", context) ?? "Passed";

            // Parse outcome string to enum
            var testOutcome = Enum.Parse<Phase3.AzureDevOps.Models.TestPlans.TestOutcome>(outcome, ignoreCase: true);
            
            await _testPlanService.UpdateTestResultAsync(new Phase3.AzureDevOps.Models.TestPlans.UpdateTestResultRequest
            {
                TestCaseId = testCaseId,
                TestPlanId = testPlanId,
                Outcome = testOutcome,
                Comment = $"Executed by {context.AgentName}"
            }, CancellationToken.None);

            return new ActionResult
            {
                Success = true,
                Message = $"Test case {testCaseId} result: {outcome}"
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"Failed to run test case: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteGitCloneAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var repositoryUrl = GetParameterValue<string>(step, "repositoryUrl", context);
            if (string.IsNullOrEmpty(repositoryUrl))
            {
                return new ActionResult
                {
                    Success = false,
                    Message = "Repository URL not specified"
                };
            }

            var workspaceName = GetParameterValue<string>(step, "workspaceName", context) ?? $"workspace-{context.WorkItemId}";
            
            var workspace = await _workspaceManager.CreateWorkspaceAsync(new Phase3.AzureDevOps.Models.Git.CreateWorkspaceRequest
            {
                Name = workspaceName
            });

            var cloneRequest = new Phase3.AzureDevOps.Models.Git.CloneRepositoryRequest
            {
                RepositoryUrl = ReplaceVariables(repositoryUrl, context),
                LocalPath = workspace.Path
            };

            await _gitService.CloneRepositoryAsync(cloneRequest);

            return new ActionResult
            {
                Success = true,
                Message = $"Cloned repository to {workspace.Path}",
                Outputs = new Dictionary<string, object>
                {
                    ["workspacePath"] = workspace.Path,
                    ["workspaceId"] = workspace.Id
                }
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"Failed to clone repository: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteGitCommitAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var workspacePath = GetParameterValue<string>(step, "workspacePath", context);
            var commitMessage = GetParameterValue<string>(step, "message", context) ?? "Automated commit";

            if (string.IsNullOrEmpty(workspacePath))
            {
                return new ActionResult
                {
                    Success = false,
                    Message = "Workspace path not specified"
                };
            }

            var commitRequest = new Phase3.AzureDevOps.Models.Git.CommitChangesRequest
            {
                RepositoryPath = workspacePath,
                CommitMessage = ReplaceVariables(commitMessage, context),
                AuthorName = context.AgentName,
                AuthorEmail = $"{context.AgentName}@cpu-agents.local"
            };

            await _gitService.CommitChangesAsync(commitRequest);

            return new ActionResult
            {
                Success = true,
                Message = "Changes committed"
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"Failed to commit changes: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteGitPushAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var workspacePath = GetParameterValue<string>(step, "workspacePath", context);

            if (string.IsNullOrEmpty(workspacePath))
            {
                return new ActionResult
                {
                    Success = false,
                    Message = "Workspace path not specified"
                };
            }

            var pushRequest = new Phase3.AzureDevOps.Models.Git.PushChangesRequest
            {
                RepositoryPath = workspacePath
            };

            await _gitService.PushChangesAsync(pushRequest);

            return new ActionResult
            {
                Success = true,
                Message = "Changes pushed to remote"
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"Failed to push changes: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteAICodeReviewAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var code = GetParameterValue<string>(step, "code", context) ?? "";
            var reviewContext = GetParameterValue<string>(step, "context", context) ?? "";

            var review = await _aiService.ReviewCodeAsync(code, "csharp", context);

            return new ActionResult
            {
                Success = true,
                Message = "Code review completed",
                Outputs = new Dictionary<string, object>
                {
                    ["review"] = review
                }
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"AI code review failed: {ex.Message}",
                Error = ex
            };
        }
    }

    private async Task<ActionResult> ExecuteAIRootCauseAnalysisAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var bugDescription = GetParameterValue<string>(step, "description", context) ?? "";
            var stackTrace = GetParameterValue<string>(step, "stackTrace", context) ?? "";

            var analysis = await _aiService.AnalyzeRootCauseAsync(bugDescription, stackTrace, context);

            return new ActionResult
            {
                Success = true,
                Message = "Root cause analysis completed",
                Outputs = new Dictionary<string, object>
                {
                    ["analysis"] = analysis
                }
            };
        }
        catch (Exception ex)
        {
            return new ActionResult
            {
                Success = false,
                Message = $"AI root cause analysis failed: {ex.Message}",
                Error = ex
            };
        }
    }

    private Task<ActionResult> ExecuteSetVariableAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        var name = GetParameterValue<string>(step, "name", context);
        var value = GetParameterValue<object>(step, "value", context);

        if (string.IsNullOrEmpty(name))
        {
            return Task.FromResult(new ActionResult
            {
                Success = false,
                Message = "Variable name not specified"
            });
        }

        context.Variables[name] = value ?? "";

        return Task.FromResult(new ActionResult
        {
            Success = true,
            Message = $"Set variable '{name}' = '{value}'"
        });
    }

    private async Task<ActionResult> ExecuteDelayAction(WorkflowStep step, WorkflowContext context, CancellationToken cancellationToken)
    {
        var seconds = GetParameterValue<int>(step, "seconds", context);
        if (seconds > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
        }

        return new ActionResult
        {
            Success = true,
            Message = $"Delayed for {seconds} seconds"
        };
    }

    // Helper methods

    private T? GetParameterValue<T>(WorkflowStep step, string parameterName, WorkflowContext context)
    {
        if (!step.Parameters.ContainsKey(parameterName))
        {
            return default;
        }

        var value = step.Parameters[parameterName];

        // Replace variable placeholders if value is a string
        if (value is string strValue)
        {
            value = ReplaceVariables(strValue, context);
        }

        try
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    private string ReplaceVariables(string input, WorkflowContext context)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Replace {{variableName}} with actual values
        var pattern = @"\{\{([^}]+)\}\}";
        return Regex.Replace(input, pattern, match =>
        {
            var variableName = match.Groups[1].Value.Trim();
            
            if (context.Variables.ContainsKey(variableName))
            {
                return context.Variables[variableName]?.ToString() ?? "";
            }

            if (context.StepOutputs.ContainsKey(variableName))
            {
                return context.StepOutputs[variableName]?.ToString() ?? "";
            }

            // Check for work item fields
            if (variableName.StartsWith("workItem."))
            {
                var fieldName = variableName.Substring("workItem.".Length);
                if (context.WorkItem?.Fields.ContainsKey(fieldName) == true)
                {
                    return context.WorkItem.Fields[fieldName]?.ToString() ?? "";
                }
            }

            return match.Value; // Return original if variable not found
        });
    }
}
