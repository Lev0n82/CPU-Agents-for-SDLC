using AutonomousAgent.LLM.Core;
using AutonomousAgent.LLM.Models;
using AutonomousAgent.LLM.Prompts;
using AutonomousAgent.LLM.Context;

namespace AutonomousAgent.Examples;

/// <summary>
/// Comprehensive demonstrations of Phase 2 LLM Integration features
/// These examples show how to use all implemented components
/// </summary>
public class Phase2Demonstrations
{
    /// <summary>
    /// Demonstration 1: Model Management
    /// Shows how to discover, download, and manage LLM models
    /// </summary>
    public static async Task DemonstrateModelManagement()
    {
        Console.WriteLine("=== Demonstration 1: Model Management ===\n");
        
        var modelManager = new ModelManager();
        
        // 1. Get available models catalog
        Console.WriteLine("1. Fetching model catalog...");
        var catalog = await modelManager.GetCatalogAsync();
        Console.WriteLine($"   Found {catalog.Models.Count} available models\n");
        
        foreach (var model in catalog.Models.Take(3))
        {
            Console.WriteLine($"   - {model.Name}");
            Console.WriteLine($"     ID: {model.ModelId}");
            Console.WriteLine($"     Parameters: {model.ParameterCount / 1_000_000_000.0:F1}B");
            Console.WriteLine($"     Size: {model.FileSizeBytes / 1_000_000_000.0:F2} GB");
            Console.WriteLine($"     Quantization: {model.Quantization}\n");
        }
        
        // 2. Download a model with progress tracking
        Console.WriteLine("2. Downloading Phi-3-mini model...");
        var progress = new Progress<DownloadProgress>(p =>
        {
            Console.Write($"\r   Progress: {p.PercentComplete:F1}% " +
                         $"({p.BytesDownloaded / 1_000_000.0:F1} MB / " +
                         $"{p.TotalBytes / 1_000_000.0:F1} MB) " +
                         $"Speed: {p.BytesPerSecond / 1_000_000.0:F2} MB/s");
        });
        
        var modelInfo = await modelManager.DownloadModelAsync("phi-3-mini-4k-q4", progress);
        Console.WriteLine($"\n   ✓ Model downloaded to: {modelInfo.LocalPath}\n");
        
        // 3. Verify model integrity
        Console.WriteLine("3. Verifying model integrity...");
        var isValid = await modelManager.VerifyModelAsync(modelInfo.LocalPath);
        Console.WriteLine($"   Model verification: {(isValid ? "✓ PASSED" : "✗ FAILED")}\n");
        
        // 4. List installed models
        Console.WriteLine("4. Listing installed models...");
        var installed = await modelManager.GetInstalledModelsAsync();
        Console.WriteLine($"   Total installed: {installed.Count}");
        foreach (var model in installed)
        {
            Console.WriteLine($"   - {model.Name} (installed: {model.InstalledDate:yyyy-MM-dd})");
        }
        
        Console.WriteLine("\n✓ Model Management demonstration complete!\n");
    }
    
    /// <summary>
    /// Demonstration 2: LLM Engine and Inference
    /// Shows how to load models and perform inference
    /// </summary>
    public static async Task DemonstrateLLMEngine()
    {
        Console.WriteLine("=== Demonstration 2: LLM Engine and Inference ===\n");
        
        using var engine = new MockLlamaEngine();
        
        // 1. Check engine status
        Console.WriteLine("1. Checking engine status...");
        var engineInfo = engine.GetEngineInfo();
        Console.WriteLine($"   Engine: {engineInfo.Version}");
        Console.WriteLine($"   Build: {engineInfo.BuildInfo}");
        Console.WriteLine($"   GPU Support: {engineInfo.SupportsGpu}");
        Console.WriteLine($"   Streaming: {engineInfo.SupportsStreaming}");
        Console.WriteLine($"   Ready: {engine.IsReady}\n");
        
        // 2. Load a model
        Console.WriteLine("2. Loading model...");
        var loadOptions = new ModelLoadOptions
        {
            ContextLength = 4096,
            ThreadCount = 8,
            BatchSize = 512,
            EnableGpu = false
        };
        
        var model = await engine.LoadModelAsync("phi-3-mini.gguf", loadOptions);
        Console.WriteLine($"   ✓ Model loaded: {model.ModelId}");
        Console.WriteLine($"   File: {model.FilePath}");
        Console.WriteLine($"   Loaded at: {model.LoadedAt:yyyy-MM-dd HH:mm:ss}\n");
        
        // 3. Create inference context
        Console.WriteLine("3. Creating inference context...");
        var contextOptions = new ContextOptions
        {
            MaxContextLength = 4096,
            Temperature = 0.7f,
            TopP = 0.9f,
            RepetitionPenalty = 1.1f
        };
        
        var context = model.CreateContext(contextOptions);
        Console.WriteLine($"   ✓ Context created: {context.ContextId}");
        Console.WriteLine($"   Max tokens: {context.MaxContextLength}");
        Console.WriteLine($"   Current tokens: {context.CurrentTokenCount}\n");
        
        // 4. Perform inference
        Console.WriteLine("4. Performing inference...");
        var request = new InferenceRequest
        {
            Prompt = "Explain what autonomous AI agents are in 2 sentences.",
            MaxTokens = 100,
            Temperature = 0.7f,
            TopP = 0.9f,
            StopSequences = new List<string> { "\n\n" }
        };
        
        var response = await context.InferAsync(request);
        Console.WriteLine($"   Prompt: {request.Prompt}");
        Console.WriteLine($"   Response: {response.Text}");
        Console.WriteLine($"   Tokens generated: {response.TokensGenerated}");
        Console.WriteLine($"   Time: {response.InferenceTime.TotalMilliseconds:F0} ms");
        Console.WriteLine($"   Speed: {response.TokensGenerated / response.InferenceTime.TotalSeconds:F1} tokens/sec\n");
        
        // 5. Stream inference
        Console.WriteLine("5. Streaming inference...");
        var streamRequest = new InferenceRequest
        {
            Prompt = "List 3 benefits of CPU-based AI agents:",
            MaxTokens = 150,
            Temperature = 0.8f
        };
        
        Console.Write("   Response: ");
        await foreach (var token in context.StreamAsync(streamRequest))
        {
            Console.Write(token.Token);
            if (token.IsFinal)
            {
                Console.WriteLine($"\n   ✓ Streaming complete ({token.Index + 1} tokens)\n");
            }
        }
        
        Console.WriteLine("✓ LLM Engine demonstration complete!\n");
    }
    
    /// <summary>
    /// Demonstration 3: Prompt Engineering
    /// Shows how to use templates, builders, and parsers
    /// </summary>
    public static void DemonstratePromptEngineering()
    {
        Console.WriteLine("=== Demonstration 3: Prompt Engineering ===\n");
        
        // 1. Using prompt templates
        Console.WriteLine("1. Using prompt templates...");
        var template = new PromptTemplate(
            "test-case-gen",
            "Test Case Generator",
            "Generates test cases from requirements",
            @"You are a QA engineer. Generate test cases for the following requirement:

Requirement: {{requirement}}
Test Type: {{test_type}}
Coverage Level: {{coverage}}

Generate {{count}} test cases in the following format:
- Test Case ID
- Description
- Preconditions
- Steps
- Expected Result",
            new[] { "requirement", "test_type", "count" },
            new Dictionary<string, string> { ["coverage"] = "comprehensive" }
        );
        
        var variables = new Dictionary<string, string>
        {
            ["requirement"] = "User login with email and password",
            ["test_type"] = "Functional",
            ["count"] = "5"
        };
        
        var prompt = template.Render(variables);
        Console.WriteLine($"   Template: {template.Name}");
        Console.WriteLine($"   Variables: {variables.Count}");
        Console.WriteLine($"   Generated prompt length: {prompt.Length} characters\n");
        
        // 2. Using prompt builder
        Console.WriteLine("2. Using prompt builder...");
        var builder = new PromptBuilder();
        builder
            .WithSystemPrompt("You are an expert software testing assistant specialized in requirements analysis.")
            .AddUserMessage("Analyze this requirement for testability: User shall be able to reset password via email")
            .WithMaxTokens(500)
            .WithTemperature(0.3f)
            .WithTopP(0.95f)
            .AddStopSequence("---");
        
        var builtRequest = builder.BuildRequest();
        Console.WriteLine($"   System prompt: Set");
        Console.WriteLine($"   User messages: 1");
        Console.WriteLine($"   Max tokens: {builtRequest.MaxTokens}");
        Console.WriteLine($"   Temperature: {builtRequest.Temperature}");
        Console.WriteLine($"   Stop sequences: {builtRequest.StopSequences.Count}\n");
        
        // 3. Using response parser
        Console.WriteLine("3. Using response parser...");
        var parser = new ResponseParser();
        
        // Parse JSON response
        var jsonResponse = @"{
            ""requirement_id"": ""REQ-001"",
            ""testable"": true,
            ""ambiguities"": [""What is the timeout for email delivery?""],
            ""test_cases_count"": 8
        }";
        
        var parsed = parser.Parse<TestabilityAnalysis>(jsonResponse);
        Console.WriteLine($"   Parsed requirement: {parsed.RequirementId}");
        Console.WriteLine($"   Testable: {parsed.Testable}");
        Console.WriteLine($"   Ambiguities found: {parsed.Ambiguities.Count}");
        Console.WriteLine($"   Suggested test cases: {parsed.TestCasesCount}\n");
        
        // Extract code blocks
        var codeResponse = @"Here's the test code:
```csharp
[Test]
public void TestPasswordReset()
{
    // Arrange
    var user = CreateTestUser();
    
    // Act
    var result = user.RequestPasswordReset();
    
    // Assert
    Assert.IsTrue(result.Success);
}
```";
        
        var code = parser.ExtractCodeBlock(codeResponse, "csharp");
        Console.WriteLine($"   Extracted code block ({code.Split('\n').Length} lines)");
        Console.WriteLine($"   Language: csharp\n");
        
        // Extract lists
        var listResponse = @"Test types to generate:
- Unit tests for password validation
- Integration tests for email service
- End-to-end tests for complete flow
- Security tests for token expiration";
        
        var items = parser.ExtractList(listResponse);
        Console.WriteLine($"   Extracted list items: {items.Count}");
        foreach (var item in items)
        {
            Console.WriteLine($"     • {item}");
        }
        
        Console.WriteLine("\n✓ Prompt Engineering demonstration complete!\n");
    }
    
    /// <summary>
    /// Demonstration 4: Context Management
    /// Shows how to manage conversation history and optimize context
    /// </summary>
    public static void DemonstrateContextManagement()
    {
        Console.WriteLine("=== Demonstration 4: Context Management ===\n");
        
        var contextManager = new ContextManager();
        
        // 1. Building conversation history
        Console.WriteLine("1. Building conversation history...");
        contextManager.AddMessage("system", "You are a requirements analysis assistant.");
        contextManager.AddMessage("user", "Analyze this requirement: User login");
        contextManager.AddMessage("assistant", "The requirement is too vague. Need more details about authentication method.");
        contextManager.AddMessage("user", "User login with email and password, max 3 attempts");
        contextManager.AddMessage("assistant", "Good! This requirement is testable. I can generate functional and security test cases.");
        
        var history = contextManager.GetHistory();
        Console.WriteLine($"   Messages in history: {history.Count}");
        Console.WriteLine($"   Total tokens: {contextManager.GetTokenCount()}\n");
        
        // 2. Displaying conversation
        Console.WriteLine("2. Conversation history:");
        foreach (var msg in history)
        {
            var preview = msg.Content.Length > 60 
                ? msg.Content.Substring(0, 60) + "..." 
                : msg.Content;
            Console.WriteLine($"   [{msg.Role}] {preview}");
        }
        Console.WriteLine();
        
        // 3. Building context prompt
        Console.WriteLine("3. Building context prompt...");
        var contextPrompt = contextManager.BuildContextPrompt();
        Console.WriteLine($"   Prompt length: {contextPrompt.Length} characters");
        Console.WriteLine($"   Estimated tokens: {contextManager.GetTokenCount()}\n");
        
        // 4. Getting recent messages
        Console.WriteLine("4. Getting recent messages...");
        var recent = contextManager.GetRecentMessages(3);
        Console.WriteLine($"   Last {recent.Count} messages:");
        foreach (var msg in recent)
        {
            Console.WriteLine($"     [{msg.Role}] {msg.Content.Substring(0, Math.Min(50, msg.Content.Length))}...");
        }
        Console.WriteLine();
        
        // 5. Context optimization
        Console.WriteLine("5. Optimizing context (sliding window strategy)...");
        var tokensBefore = contextManager.GetTokenCount();
        contextManager.OptimizeContext(maxTokens: 100, OptimizationStrategy.SlidingWindow);
        var tokensAfter = contextManager.GetTokenCount();
        
        Console.WriteLine($"   Tokens before: {tokensBefore}");
        Console.WriteLine($"   Tokens after: {tokensAfter}");
        Console.WriteLine($"   Reduction: {tokensBefore - tokensAfter} tokens ({(1 - (double)tokensAfter/tokensBefore)*100:F1}%)\n");
        
        Console.WriteLine("✓ Context Management demonstration complete!\n");
    }
    
    /// <summary>
    /// Demonstration 5: End-to-End Workflow
    /// Shows a complete workflow combining all components
    /// </summary>
    public static async Task DemonstrateEndToEndWorkflow()
    {
        Console.WriteLine("=== Demonstration 5: End-to-End Workflow ===\n");
        Console.WriteLine("Scenario: Automated Requirements Analysis and Test Generation\n");
        
        // 1. Initialize components
        Console.WriteLine("1. Initializing components...");
        var modelManager = new ModelManager();
        using var engine = new MockLlamaEngine();
        var contextManager = new ContextManager();
        var parser = new ResponseParser();
        Console.WriteLine("   ✓ All components initialized\n");
        
        // 2. Load model
        Console.WriteLine("2. Loading model...");
        var loadOptions = new ModelLoadOptions { ContextLength = 4096, ThreadCount = 8 };
        var model = await engine.LoadModelAsync("phi-3-mini.gguf", loadOptions);
        Console.WriteLine($"   ✓ Model loaded: {model.ModelId}\n");
        
        // 3. Create context
        Console.WriteLine("3. Creating inference context...");
        var contextOptions = new ContextOptions { MaxContextLength = 4096, Temperature = 0.7f };
        var context = model.CreateContext(contextOptions);
        Console.WriteLine($"   ✓ Context ready: {context.ContextId}\n");
        
        // 4. Analyze requirement
        Console.WriteLine("4. Analyzing requirement...");
        var requirement = "The system shall allow users to upload files up to 10MB in size";
        
        var analysisPrompt = new PromptBuilder()
            .WithSystemPrompt("You are a requirements analysis expert.")
            .AddUserMessage($"Analyze this requirement for testability and ambiguities: {requirement}")
            .WithMaxTokens(300)
            .WithTemperature(0.3f)
            .BuildRequest();
        
        var analysisResponse = await context.InferAsync(analysisPrompt);
        Console.WriteLine($"   Requirement: {requirement}");
        Console.WriteLine($"   Analysis: {analysisResponse.Text.Substring(0, Math.Min(150, analysisResponse.Text.Length))}...\n");
        
        // 5. Generate test cases
        Console.WriteLine("5. Generating test cases...");
        contextManager.AddMessage("user", $"Generate 3 test cases for: {requirement}");
        
        var testGenPrompt = new PromptBuilder()
            .WithSystemPrompt("You are a test case generation expert.")
            .AddUserMessage($"Generate 3 functional test cases for: {requirement}")
            .WithMaxTokens(500)
            .WithTemperature(0.6f)
            .BuildRequest();
        
        var testCases = await context.InferAsync(testGenPrompt);
        var extractedTests = parser.ExtractList(testCases.Text);
        Console.WriteLine($"   Generated {extractedTests.Count} test cases:");
        foreach (var test in extractedTests.Take(3))
        {
            Console.WriteLine($"     • {test}");
        }
        Console.WriteLine();
        
        // 6. Performance metrics
        Console.WriteLine("6. Workflow performance metrics:");
        Console.WriteLine($"   Total inference time: {analysisResponse.InferenceTime.TotalMilliseconds + testCases.InferenceTime.TotalMilliseconds:F0} ms");
        Console.WriteLine($"   Total tokens generated: {analysisResponse.TokensGenerated + testCases.TokensGenerated}");
        Console.WriteLine($"   Average speed: {(analysisResponse.TokensGenerated + testCases.TokensGenerated) / (analysisResponse.InferenceTime.TotalSeconds + testCases.InferenceTime.TotalSeconds):F1} tokens/sec");
        Console.WriteLine($"   Context tokens used: {contextManager.GetTokenCount()}\n");
        
        Console.WriteLine("✓ End-to-End Workflow demonstration complete!\n");
    }
}

// Helper class for demonstration 3
public class TestabilityAnalysis
{
    public string RequirementId { get; set; } = string.Empty;
    public bool Testable { get; set; }
    public List<string> Ambiguities { get; set; } = new();
    public int TestCasesCount { get; set; }
}
