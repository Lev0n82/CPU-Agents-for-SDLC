using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Phase3.AgentHost.Models;

namespace Phase3.AgentHost.Services;

/// <summary>
/// Workflow engine interface for selecting and executing workflows
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Select the appropriate workflow for a work item
    /// </summary>
    Task<Workflow?> SelectWorkflowAsync(WorkItem workItem);

    /// <summary>
    /// Execute a workflow with the given context
    /// </summary>
    Task<WorkflowResult> ExecuteWorkflowAsync(Workflow workflow, WorkflowContext context, CancellationToken cancellationToken = default);
}
