namespace Phase3.AgentHost.Models;

/// <summary>
/// Code review result from AI analysis
/// </summary>
public class CodeReviewResult
{
    public int QualityScore { get; set; }
    public List<CodeIssue> Issues { get; set; } = new();
    public List<string> SecurityVulnerabilities { get; set; } = new();
    public List<string> PerformanceConcerns { get; set; } = new();
    public string OverallRecommendation { get; set; } = string.Empty;
}

public class CodeIssue
{
    public string Severity { get; set; } = string.Empty;
    public int Line { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Test obsolescence detection result
/// </summary>
public class TestObsolescenceResult
{
    public List<int> ObsoleteTestIds { get; set; } = new();
    public Dictionary<int, string> Reasons { get; set; } = new();
    public double Confidence { get; set; }
}

/// <summary>
/// Conflict resolution decision from AI
/// </summary>
public class ConflictResolutionDecision
{
    public int RecommendedOption { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? AlternativeApproach { get; set; }
}

/// <summary>
/// Root cause analysis result
/// </summary>
public class RootCauseAnalysis
{
    public string RootCause { get; set; } = string.Empty;
    public string LikelyCodeLocation { get; set; } = string.Empty;
    public string RecommendedFix { get; set; } = string.Empty;
    public List<string> PreventionStrategies { get; set; } = new();
    public double Confidence { get; set; }
    public List<string> RelatedIssues { get; set; } = new();
}
