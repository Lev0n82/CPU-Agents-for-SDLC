using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Phase3.AgentHost.Models;

/// <summary>
/// Represents a workflow definition
/// </summary>
public class Workflow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public List<string> ApplicableWorkItemTypes { get; set; } = new();
    public Dictionary<string, string> Conditions { get; set; } = new();
    public List<WorkflowStep> Steps { get; set; } = new();
    public Dictionary<string, object> DefaultVariables { get; set; } = new();
}

/// <summary>
/// Represents a single step in a workflow
/// </summary>
public class WorkflowStep
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string? ContinueOnError { get; set; }
    public int RetryCount { get; set; } = 0;
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Workflow execution context
/// </summary>
public class WorkflowContext
{
    public int WorkItemId { get; set; }
    public WorkItem? WorkItem { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
    public Dictionary<string, object> StepOutputs { get; set; } = new();
    public List<string> ExecutionLog { get; set; } = new();
}

/// <summary>
/// Workflow execution result
/// </summary>
public class WorkflowResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Outputs { get; set; } = new();
    public List<string> ExecutionLog { get; set; } = new();
    public Exception? Error { get; set; }
}

/// <summary>
/// Workflow action result
/// </summary>
public class ActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Outputs { get; set; } = new();
    public Exception? Error { get; set; }
}
