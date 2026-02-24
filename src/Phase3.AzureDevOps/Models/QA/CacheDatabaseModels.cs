using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Phase3.AzureDevOps.Models.QA;

/// <summary>
/// Database model for cached Azure DevOps QA data
/// </summary>
[Table("Projects")]
public class Project
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AzureDevOpsProjectId { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    public DateTime? LastSyncDate { get; set; }
    
    public ICollection<Release> Releases { get; set; } = new List<Release>();
    public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
}

[Table("Releases")]
public class Release
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string AzureDevOpsReleaseId { get; set; } = string.Empty;
    
    public DateTime? RequirementsPublishedDate { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    
    public ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
}

[Table("Requirements")]
public class Requirement
{
    [Key]
    public int Id { get; set; }
    
    public int? ReleaseId { get; set; }
    public Release? Release { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AzureDevOpsWorkItemId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string WorkItemType { get; set; } = "User Story";
    
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? RequirementsPublishedDate { get; set; }
    public DateTime? TestCaseDevelopmentStartDate { get; set; }
    public DateTime? FirstTestCaseCreatedDate { get; set; }
    public DateTime? FirstTestCaseExecutedDate { get; set; }
    public DateTime? LastTestCaseExecutedDate { get; set; }
    
    public int PlannedTestCaseCount { get; set; }
    public int ActualTestCaseCount { get; set; }
    public int ExecutedTestCaseCount { get; set; }
    public int PassedTestCaseCount { get; set; }
    public int FailedTestCaseCount { get; set; }
    
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
}

[Table("TestCases")]
public class TestCase
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int RequirementId { get; set; }
    public Requirement Requirement { get; set; } = null!;
    
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AzureDevOpsWorkItemId { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? FirstExecutedDate { get; set; }
    public DateTime? LastExecutedDate { get; set; }
    
    [MaxLength(50)]
    public string CurrentState { get; set; } = "New";
    
    [MaxLength(50)]
    public string LastOutcome { get; set; } = "Not Executed";
    
    public int ExecutionCount { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal AverageExecutionTimeMs { get; set; }
    
    public ICollection<TestExecution> Executions { get; set; } = new List<TestExecution>();
}

[Table("TestExecutions")]
public class TestExecution
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int TestCaseId { get; set; }
    public TestCase TestCase { get; set; } = null!;
    
    [MaxLength(100)]
    public string AzureDevOpsTestRunId { get; set; } = string.Empty;
    
    public DateTime ExecutedDate { get; set; }
    public long DurationMs { get; set; }
    
    [MaxLength(50)]
    public string Outcome { get; set; } = "Not Executed";
    
    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// DbContext for QA cache database
/// </summary>
public class QaCacheDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Release> Releases { get; set; }
    public DbSet<Requirement> Requirements { get; set; }
    public DbSet<TestCase> TestCases { get; set; }
    public DbSet<TestExecution> TestExecutions { get; set; }
    
    public QaCacheDbContext(DbContextOptions<QaCacheDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Release>()
            .HasMany(r => r.Requirements)
            .WithOne(r => r.Release)
            .HasForeignKey(r => r.ReleaseId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Requirements)
            .WithOne(r => r.Project)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Requirement>()
            .HasMany(r => r.TestCases)
            .WithOne(tc => tc.Requirement)
            .HasForeignKey(tc => tc.RequirementId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<TestCase>()
            .HasMany(tc => tc.Executions)
            .WithOne(te => te.TestCase)
            .HasForeignKey(te => te.TestCaseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure indexes for performance
        modelBuilder.Entity<Project>()
            .HasIndex(p => p.AzureDevOpsProjectId)
            .IsUnique();
            
        modelBuilder.Entity<Requirement>()
            .HasIndex(r => r.AzureDevOpsWorkItemId)
            .IsUnique();
            
        modelBuilder.Entity<TestCase>()
            .HasIndex(tc => tc.AzureDevOpsWorkItemId)
            .IsUnique();
            
        modelBuilder.Entity<Release>()
            .HasIndex(r => new { r.ProjectId, r.Name })
            .IsUnique();
    }
}