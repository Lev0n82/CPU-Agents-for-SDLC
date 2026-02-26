using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Phase3.AgentHost.Models;
using System.Text.Json;
using System.ClientModel;

namespace Phase3.AgentHost.Services;

/// <summary>
/// AI Decision Service using OpenAI-compatible API (supports OpenAI, Azure OpenAI, and Ollama) for intelligent decision-making
/// </summary>
public class AIDecisionService : IAIDecisionService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<AIDecisionService> _logger;
    private readonly string _model;
    private readonly string _provider;

    private readonly bool _isEnabled;

    public AIDecisionService(
        IConfiguration configuration,
        ILogger<AIDecisionService> logger)
    {
        _logger = logger;
        
        var apiKey = configuration["OpenAI:ApiKey"];
        var provider = configuration["OpenAI:Provider"] ?? "OpenAI";
        _provider = provider;
        
        // For Ollama, API key is optional (can be empty or any value)
        _isEnabled = !string.IsNullOrEmpty(apiKey) ||
                     provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase);
        
        if (!_isEnabled)
        {
            _logger.LogWarning("OpenAI:ApiKey is not configured. AI features will be disabled.");
            _chatClient = null!;
            _model = string.Empty;
            return;
        }
        
        _model = configuration["OpenAI:Model"] ?? "gpt-4";
        var endpoint = configuration["OpenAI:Endpoint"];
        
        // Use dummy API key for Ollama if not provided
        var effectiveApiKey = string.IsNullOrEmpty(apiKey) ? "ollama" : apiKey;
        
        _logger.LogInformation("Configuring AI service with provider: {Provider}, model: {Model}, endpoint: {Endpoint}",
            provider, _model, endpoint ?? "default");
        
        if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            // For Ollama, use OpenAI client with custom base URL
            // Ollama's OpenAI-compatible API is at http://localhost:11434/v1
            var ollamaEndpoint = endpoint ?? "http://localhost:11434";
            var ollamaUri = ollamaEndpoint.EndsWith("/v1") ? ollamaEndpoint : $"{ollamaEndpoint}/v1";
            
            _logger.LogInformation("Using Ollama endpoint: {Endpoint}", ollamaUri);
            
            var openAIClient = new OpenAIClient(
                new ApiKeyCredential(effectiveApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(ollamaUri) });
            
            _chatClient = openAIClient.GetChatClient(_model);
        }
        else if (!string.IsNullOrEmpty(endpoint))
        {
            // Custom endpoint (could be Azure OpenAI or self-hosted)
            var client = new AzureOpenAIClient(
                new Uri(endpoint),
                new ApiKeyCredential(effectiveApiKey));
            _chatClient = client.GetChatClient(_model);
        }
        else
        {
            // Standard OpenAI
            var openAIClient = new OpenAIClient(new ApiKeyCredential(effectiveApiKey));
            _chatClient = openAIClient.GetChatClient(_model);
        }
        
        _logger.LogInformation("AI Decision Service initialized successfully with {Provider}", provider);
    }

    public async Task<CodeReviewResult> ReviewCodeAsync(string code, string language, WorkflowContext context)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("AI features are disabled. Returning default code review result.");
            return new CodeReviewResult
            {
                QualityScore = 0,
                Issues = new List<CodeIssue>(),
                SecurityVulnerabilities = new List<string>(),
                PerformanceConcerns = new List<string>(),
                OverallRecommendation = "AI features are disabled - manual review required"
            };
        }

        _logger.LogInformation("Starting AI code review for {Language}", language);

        var prompt = $@"You are an expert code reviewer. Review the following {language} code and provide:
1. Overall quality score (0-100)
2. List of issues found (severity: critical, major, minor)
3. Specific recommendations for improvement
4. Security vulnerabilities
5. Performance concerns

Code to review:
```{language}
{code}
```

Respond in JSON format:
{{
  ""qualityScore"": <number>,
  ""issues"": [
    {{
      ""severity"": ""critical|major|minor"",
      ""line"": <number>,
      ""description"": ""<string>"",
      ""recommendation"": ""<string>""
    }}
  ],
  ""securityVulnerabilities"": [""<string>""],
  ""performanceConcerns"": [""<string>""],
  ""overallRecommendation"": ""<string>""
}}";

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert code reviewer with deep knowledge of software engineering best practices, security, and performance optimization."),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var result = JsonSerializer.Deserialize<CodeReviewResult>(content);
            
            _logger.LogInformation("Code review completed. Quality score: {Score}", result?.QualityScore);
            
            return result ?? new CodeReviewResult
            {
                QualityScore = 0,
                Issues = new List<CodeIssue>(),
                SecurityVulnerabilities = new List<string>(),
                PerformanceConcerns = new List<string>(),
                OverallRecommendation = "Failed to parse AI response"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during AI code review");
            throw;
        }
    }

    public async Task<TestObsolescenceResult> DetectObsoleteTestsAsync(
        List<int> testCaseIds,
        WorkflowContext context)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("AI features are disabled. Returning empty obsolescence result.");
            return new TestObsolescenceResult
            {
                ObsoleteTestIds = new List<int>(),
                Reasons = new Dictionary<int, string>(),
                Confidence = 0
            };
        }

        _logger.LogInformation("Detecting obsolete tests for {Count} test cases", testCaseIds.Count);

        var prompt = $@"You are an expert QA engineer. Analyze the following test case information and determine which tests are obsolete.

Test cases: {string.Join(", ", testCaseIds)}

Context:
- Work Item: {context.WorkItemId}
- Workflow: {context.WorkflowName}

Consider tests obsolete if they:
1. Test features that no longer exist
2. Are duplicates of other tests
3. Test deprecated functionality
4. Have not been updated in over 6 months and reference old code

Respond in JSON format:
{{
  ""obsoleteTestIds"": [<number>],
  ""reasons"": {{
    ""<testId>"": ""<reason>""
  }},
  ""confidence"": <number 0-1>
}}";

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert QA engineer with deep knowledge of test case management and software testing best practices."),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var result = JsonSerializer.Deserialize<TestObsolescenceResult>(content);
            
            _logger.LogInformation("Obsolescence detection completed. Found {Count} obsolete tests", 
                result?.ObsoleteTestIds.Count ?? 0);
            
            return result ?? new TestObsolescenceResult
            {
                ObsoleteTestIds = new List<int>(),
                Reasons = new Dictionary<int, string>(),
                Confidence = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during obsolete test detection");
            throw;
        }
    }

    public async Task<ConflictResolutionDecision> ResolveConflictAsync(
        string conflictDescription,
        List<string> options,
        WorkflowContext context)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("AI features are disabled. Returning default conflict resolution.");
            return new ConflictResolutionDecision
            {
                RecommendedOption = 1,
                Reasoning = "AI features are disabled - manual resolution required",
                Confidence = 0,
                AlternativeApproach = null
            };
        }

        _logger.LogInformation("Resolving conflict with {Count} options", options.Count);

        var prompt = $@"You are an expert software engineer. Analyze the following merge conflict and recommend the best resolution strategy.

Conflict Description:
{conflictDescription}

Available Options:
{string.Join("\n", options.Select((o, i) => $"{i + 1}. {o}"))}

Context:
- Work Item: {context.WorkItemId}
- Workflow: {context.WorkflowName}

Respond in JSON format:
{{
  ""recommendedOption"": <number 1-{options.Count}>,
  ""reasoning"": ""<string>"",
  ""confidence"": <number 0-1>,
  ""alternativeApproach"": ""<string or null>""
}}";

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert software engineer with deep knowledge of version control, merge conflicts, and code integration strategies."),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var result = JsonSerializer.Deserialize<ConflictResolutionDecision>(content);
            
            _logger.LogInformation("Conflict resolution completed. Recommended option: {Option}", 
                result?.RecommendedOption);
            
            return result ?? new ConflictResolutionDecision
            {
                RecommendedOption = 1,
                Reasoning = "Failed to parse AI response",
                Confidence = 0,
                AlternativeApproach = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during conflict resolution");
            throw;
        }
    }

    public async Task<RootCauseAnalysis> AnalyzeRootCauseAsync(
        string errorMessage,
        string stackTrace,
        WorkflowContext context)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("AI features are disabled. Returning default root cause analysis.");
            return new RootCauseAnalysis
            {
                RootCause = "AI features are disabled - manual analysis required",
                LikelyCodeLocation = "Unknown",
                RecommendedFix = "Manual investigation required",
                PreventionStrategies = new List<string>(),
                Confidence = 0,
                RelatedIssues = new List<string>()
            };
        }

        _logger.LogInformation("Analyzing root cause for error");

        var prompt = $@"You are an expert software engineer and debugger. Analyze the following error and provide root cause analysis.

Error Message:
{errorMessage}

Stack Trace:
{stackTrace}

Context:
- Work Item: {context.WorkItemId}
- Workflow: {context.WorkflowName}

Provide:
1. Root cause identification
2. Likely code location
3. Recommended fix
4. Prevention strategies

Respond in JSON format:
{{
  ""rootCause"": ""<string>"",
  ""likelyCodeLocation"": ""<string>"",
  ""recommendedFix"": ""<string>"",
  ""preventionStrategies"": [""<string>""],
  ""confidence"": <number 0-1>,
  ""relatedIssues"": [""<string>""]
}}";

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert software engineer and debugger with deep knowledge of common error patterns, debugging techniques, and software architecture."),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            var result = JsonSerializer.Deserialize<RootCauseAnalysis>(content);
            
            _logger.LogInformation("Root cause analysis completed. Confidence: {Confidence}", 
                result?.Confidence);
            
            return result ?? new RootCauseAnalysis
            {
                RootCause = "Failed to parse AI response",
                LikelyCodeLocation = "Unknown",
                RecommendedFix = "Manual investigation required",
                PreventionStrategies = new List<string>(),
                Confidence = 0,
                RelatedIssues = new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during root cause analysis");
            throw;
        }
    }
}


