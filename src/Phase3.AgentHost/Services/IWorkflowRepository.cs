using Phase3.AgentHost.Models;

namespace Phase3.AgentHost.Services;

/// <summary>
/// Repository for loading and managing workflows
/// </summary>
public interface IWorkflowRepository
{
    /// <summary>
    /// Load all workflows from storage
    /// </summary>
    Task<List<Workflow>> LoadAllWorkflowsAsync();

    /// <summary>
    /// Load a specific workflow by ID
    /// </summary>
    Task<Workflow?> LoadWorkflowAsync(string workflowId);

    /// <summary>
    /// Save a workflow to storage
    /// </summary>
    Task SaveWorkflowAsync(Workflow workflow);

    /// <summary>
    /// Delete a workflow from storage
    /// </summary>
    Task DeleteWorkflowAsync(string workflowId);
}
