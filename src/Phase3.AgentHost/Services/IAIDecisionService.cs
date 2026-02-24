using Phase3.AgentHost.Models;

namespace Phase3.AgentHost.Services;

/// <summary>
/// AI decision service interface for intelligent decision-making using OpenAI GPT-4
/// </summary>
public interface IAIDecisionService
{
    /// <summary>
    /// Reviews code and provides quality assessment and recommendations
    /// </summary>
    Task<CodeReviewResult> ReviewCodeAsync(string code, string language, WorkflowContext context);

    /// <summary>
    /// Detects obsolete test cases based on context and patterns
    /// </summary>
    Task<TestObsolescenceResult> DetectObsoleteTestsAsync(List<int> testCaseIds, WorkflowContext context);

    /// <summary>
    /// Resolves merge conflicts by recommending the best resolution strategy
    /// </summary>
    Task<ConflictResolutionDecision> ResolveConflictAsync(string conflictDescription, List<string> options, WorkflowContext context);

    /// <summary>
    /// Analyzes errors and provides root cause analysis
    /// </summary>
    Task<RootCauseAnalysis> AnalyzeRootCauseAsync(string errorMessage, string stackTrace, WorkflowContext context);
}
