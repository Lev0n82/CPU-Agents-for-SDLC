using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phase3.AgentHost.Models;
using System.Text.Json;

namespace Phase3.AgentHost.Services;

/// <summary>
/// File system-based workflow repository
/// </summary>
public class FileSystemWorkflowRepository : IWorkflowRepository
{
    private readonly ILogger<FileSystemWorkflowRepository> _logger;
    private readonly string _workflowsDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileSystemWorkflowRepository(
        ILogger<FileSystemWorkflowRepository> logger,
        IOptions<AgentConfiguration> config)
    {
        _logger = logger;
        _workflowsDirectory = Path.GetFullPath(config.Value.WorkflowsDirectory);
        
        // Create workflows directory if it doesn't exist
        if (!Directory.Exists(_workflowsDirectory))
        {
            Directory.CreateDirectory(_workflowsDirectory);
            _logger.LogInformation("Created workflows directory: {Directory}", _workflowsDirectory);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<List<Workflow>> LoadAllWorkflowsAsync()
    {
        var workflows = new List<Workflow>();

        try
        {
            var files = Directory.GetFiles(_workflowsDirectory, "*.json");
            _logger.LogInformation("Found {Count} workflow files in {Directory}", files.Length, _workflowsDirectory);

            foreach (var file in files)
            {
                try
                {
                    var workflow = await LoadWorkflowFromFileAsync(file);
                    if (workflow != null)
                    {
                        workflows.Add(workflow);
                        _logger.LogDebug("Loaded workflow: {WorkflowId} - {WorkflowName}", workflow.Id, workflow.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load workflow from file: {File}", file);
                }
            }

            _logger.LogInformation("Loaded {Count} workflows", workflows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load workflows from directory: {Directory}", _workflowsDirectory);
        }

        return workflows;
    }

    public async Task<Workflow?> LoadWorkflowAsync(string workflowId)
    {
        try
        {
            var filePath = Path.Combine(_workflowsDirectory, $"{workflowId}.json");
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Workflow file not found: {FilePath}", filePath);
                return null;
            }

            return await LoadWorkflowFromFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load workflow: {WorkflowId}", workflowId);
            return null;
        }
    }

    public async Task SaveWorkflowAsync(Workflow workflow)
    {
        try
        {
            var filePath = Path.Combine(_workflowsDirectory, $"{workflow.Id}.json");
            var json = JsonSerializer.Serialize(workflow, _jsonOptions);
            
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation("Saved workflow: {WorkflowId} - {WorkflowName} to {FilePath}", 
                workflow.Id, workflow.Name, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save workflow: {WorkflowId}", workflow.Id);
            throw;
        }
    }

    public async Task DeleteWorkflowAsync(string workflowId)
    {
        try
        {
            var filePath = Path.Combine(_workflowsDirectory, $"{workflowId}.json");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted workflow: {WorkflowId}", workflowId);
            }
            else
            {
                _logger.LogWarning("Workflow file not found for deletion: {FilePath}", filePath);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete workflow: {WorkflowId}", workflowId);
            throw;
        }
    }

    private async Task<Workflow?> LoadWorkflowFromFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var workflow = JsonSerializer.Deserialize<Workflow>(json, _jsonOptions);
        
        if (workflow == null)
        {
            _logger.LogWarning("Failed to deserialize workflow from file: {FilePath}", filePath);
            return null;
        }

        // Validate workflow
        if (string.IsNullOrEmpty(workflow.Id))
        {
            _logger.LogWarning("Workflow missing ID in file: {FilePath}", filePath);
            return null;
        }

        if (string.IsNullOrEmpty(workflow.Name))
        {
            _logger.LogWarning("Workflow missing name in file: {FilePath}", filePath);
            return null;
        }

        if (workflow.Steps == null || workflow.Steps.Count == 0)
        {
            _logger.LogWarning("Workflow has no steps in file: {FilePath}", filePath);
            return null;
        }

        return workflow;
    }
}
