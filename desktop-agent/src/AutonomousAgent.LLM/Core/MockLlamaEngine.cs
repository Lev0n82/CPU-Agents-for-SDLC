using AutonomousAgent.LLM.Exceptions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AutonomousAgent.LLM.Core;

/// <summary>
/// Mock implementation of ILlamaEngine for testing without native dependencies
/// This allows the system to be built and tested before llama.cpp integration
/// </summary>
public class MockLlamaEngine : ILlamaEngine
{
    private readonly ConcurrentDictionary<string, ILlamaModel> _loadedModels = new();
    private bool _disposed;

    public bool IsReady => !_disposed;

    public async Task<ILlamaModel> LoadModelAsync(
        string modelPath, 
        ModelLoadOptions options, 
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MockLlamaEngine));
        
        options.Validate();
        
        if (string.IsNullOrWhiteSpace(modelPath))
            throw new Exceptions.InvalidModelException(modelPath ?? "", "Model path cannot be empty");
        
        // For testing: only check file exists if path looks like a real file path
        if (modelPath.Contains(Path.DirectorySeparatorChar) && !File.Exists(modelPath))
            throw new FileNotFoundException($"Model file not found: {modelPath}", modelPath);
        
        // Simulate loading time
        await Task.Delay(100, cancellationToken);
        
        var model = new MockLlamaModel(modelPath, options);
        _loadedModels[model.ModelId] = model;
        
        return model;
    }

    public Task UnloadModelAsync(ILlamaModel model)
    {
        if (model is MockLlamaModel mockModel)
        {
            _loadedModels.TryRemove(mockModel.ModelId, out _);
            mockModel.Dispose();
        }
        
        return Task.CompletedTask;
    }

    public EngineInfo GetEngineInfo()
    {
        return new EngineInfo
        {
            Version = "Mock 1.0.0",
            BuildInfo = "Mock build for testing",
            CpuCapabilities = new List<string> { "AVX2", "FMA" },
            SupportsGpu = false
        };
    }

    public IReadOnlyList<ILlamaModel> GetLoadedModels()
    {
        return _loadedModels.Values.ToList();
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        foreach (var model in _loadedModels.Values)
        {
            model.Dispose();
        }
        
        _loadedModels.Clear();
        _disposed = true;
    }
}

/// <summary>
/// Mock model implementation
/// </summary>
public class MockLlamaModel : ILlamaModel
{
    private readonly ConcurrentBag<ILlamaContext> _contexts = new();
    private bool _disposed;

    public string ModelId { get; }
    public ModelMetadata Metadata { get; }
    public string FilePath { get; }
    public DateTime LoadedAt { get; }

    public MockLlamaModel(string filePath, ModelLoadOptions options)
    {
        FilePath = filePath;
        ModelId = Path.GetFileNameWithoutExtension(filePath);
        LoadedAt = DateTime.UtcNow;
        
        // For testing: use actual file size if file exists, otherwise use default
        long fileSize = File.Exists(filePath) ? new FileInfo(filePath).Length : 2_400_000_000; // 2.4 GB default
        
        Metadata = new ModelMetadata
        {
            Name = ModelId,
            Architecture = "Mock",
            ParameterCount = 3_800_000_000, // 3.8B
            Quantization = "Q4_K_M",
            ContextLength = options.ContextLength,
            FileSizeBytes = fileSize,
            Version = "1.0"
        };
    }

    public ILlamaContext CreateContext(ContextOptions options)
    {
        if (_disposed)
            throw new InvalidOperationException("Model is disposed");
        
        var context = new MockLlamaContext(this, options);
        _contexts.Add(context);
        return context;
    }

    public IReadOnlyList<ILlamaContext> GetActiveContexts()
    {
        return _contexts.Where(c => c is MockLlamaContext mc && !mc.IsDisposed).ToList();
    }

    public bool Validate()
    {
        // For testing: consider model valid if not disposed
        // In production, this would check file integrity
        return !_disposed;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        foreach (var context in _contexts)
        {
            context.Dispose();
        }
        
        _contexts.Clear();
        _disposed = true;
    }
}

/// <summary>
/// Mock context implementation
/// </summary>
public class MockLlamaContext : ILlamaContext
{
    private readonly MockLlamaModel _model;
    private readonly ContextOptions _options;
    private int _currentTokenCount;
    private bool _disposed;

    public string ContextId { get; }
    public ILlamaModel Model => _model;
    public int CurrentTokenCount => _currentTokenCount;
    public int MaxContextLength => _model.Metadata.ContextLength;
    public bool IsDisposed => _disposed;

    public MockLlamaContext(MockLlamaModel model, ContextOptions options)
    {
        _model = model;
        _options = options;
        ContextId = Guid.NewGuid().ToString();
    }

    public async Task<InferenceResponse> InferAsync(
        InferenceRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MockLlamaContext));
        
        request.Validate();
        
        var promptTokens = EstimateTokenCount(request.Prompt);
        
        if (promptTokens + request.MaxTokens > MaxContextLength)
            throw new ContextOverflowException(promptTokens + request.MaxTokens, MaxContextLength);
        
        var sw = Stopwatch.StartNew();
        
        // Simulate inference with realistic timing
        var tokensToGenerate = Math.Min(request.MaxTokens, 50); // Generate up to 50 tokens
        var generatedText = GenerateMockResponse(request.Prompt, tokensToGenerate);
        
        // Simulate token generation time (25 tokens/sec)
        var delayMs = (int)(tokensToGenerate / 25.0 * 1000);
        await Task.Delay(delayMs, cancellationToken);
        
        sw.Stop();
        
        _currentTokenCount += promptTokens + tokensToGenerate;
        
        return new InferenceResponse
        {
            GeneratedText = generatedText,
            TokensGenerated = tokensToGenerate,
            TokensPrompt = promptTokens,
            Duration = sw.Elapsed,
            TokensPerSecond = tokensToGenerate / sw.Elapsed.TotalSeconds,
            StopReason = tokensToGenerate >= request.MaxTokens ? "max_tokens" : "eos",
            Timestamp = DateTime.UtcNow
        };
    }

    public async IAsyncEnumerable<TokenResponse> StreamAsync(
        InferenceRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MockLlamaContext));
        
        request.Validate();
        
        var tokensToGenerate = Math.Min(request.MaxTokens, 50);
        var words = GenerateMockResponse(request.Prompt, tokensToGenerate).Split(' ');
        
        for (int i = 0; i < words.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Simulate token generation delay (25 tokens/sec = 40ms per token)
            await Task.Delay(40, cancellationToken);
            
            yield return new TokenResponse
            {
                Token = words[i] + (i < words.Length - 1 ? " " : ""),
                Index = i,
                IsFinal = i == words.Length - 1
            };
        }
    }

    public void Reset()
    {
        _currentTokenCount = 0;
    }

    public int EstimateTokenCount(string text)
    {
        // Rough estimation: ~4 characters per token
        return Math.Max(1, text.Length / 4);
    }

    private string GenerateMockResponse(string prompt, int maxTokens)
    {
        // Generate contextually relevant mock responses
        if (prompt.Contains("requirement", StringComparison.OrdinalIgnoreCase))
        {
            return "The requirement specifies that the system shall provide user authentication functionality. " +
                   "This includes login, logout, and session management capabilities. " +
                   "The system must support secure password storage and multi-factor authentication.";
        }
        else if (prompt.Contains("test case", StringComparison.OrdinalIgnoreCase))
        {
            return "Test Case ID: TC-001\nDescription: Verify user login functionality\n" +
                   "Preconditions: User account exists\nSteps: 1. Navigate to login page 2. Enter credentials 3. Click login\n" +
                   "Expected Result: User is authenticated and redirected to dashboard";
        }
        else if (prompt.Contains("code", StringComparison.OrdinalIgnoreCase))
        {
            return "```csharp\npublic class Example {\n    public void Method() {\n        Console.WriteLine(\"Hello\");\n    }\n}\n```";
        }
        else
        {
            return "This is a mock response generated for testing purposes. " +
                   "The actual LLM implementation will provide more sophisticated and contextually relevant responses. " +
                   "This mock allows the system to be built and tested before llama.cpp integration is complete.";
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
