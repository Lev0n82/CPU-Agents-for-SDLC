namespace Phase3.AzureDevOps.Models;

public class WorkItem
{
    public int Id { get; set; }
    public int Rev { get; set; }
    public Dictionary<string, object?> Fields { get; set; } = new();
}
