using Phase3.AzureDevOps;
using Phase3.AzureDevOps.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure services
var services = new ServiceCollection();
services.AddPhase3AzureDevOps(config =>
{
    config.OrganizationUrl = "https://dev.azure.com/your-org";
    config.ProjectName = "YourProject";
    config.Authentication = new AuthenticationConfiguration
    {
        Method = AuthenticationMethod.PAT,
        PAT = "your-pat-token"
    };
    config.Concurrency = new ConcurrencyConfiguration
    {
        ClaimDurationMinutes = 15,
        StaleClaimCheckIntervalMinutes = 5
    };
});

var serviceProvider = services.BuildServiceProvider();

// Use work item service
var workItemService = serviceProvider.GetRequiredService<IWorkItemService>();

// Create work item
var workItem = await workItemService.CreateWorkItemAsync(
    "Task",
    new Dictionary<string, object>
    {
        ["System.Title"] = "Implement feature X",
        ["System.Description"] = "Feature description"
    });

Console.WriteLine($"Created work item {workItem.Id}");

// Claim work item
var coordinator = serviceProvider.GetRequiredService<IWorkItemCoordinator>();
var claimed = await coordinator.TryClaimWorkItemAsync(
    workItem.Id,
    workItem.Rev,
    "agent-001");

if (claimed)
{
    Console.WriteLine("Work item claimed successfully");

    // Update work item
    var updated = await workItemService.UpdateWorkItemAsync(
        workItem.Id,
        workItem.Rev,
        new Dictionary<string, object>
        {
            ["System.State"] = "Active"
        });

    // Release claim
    await coordinator.ReleaseWorkItemAsync(
        updated.Id,
        updated.Rev,
        "agent-001");
}
