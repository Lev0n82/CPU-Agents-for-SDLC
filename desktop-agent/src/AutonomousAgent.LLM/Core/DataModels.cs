namespace AutonomousAgent.LLM.Core;

/// <summary>
/// Options for loading a model
/// </summary>
public class ModelLoadOptions
{
    /// <summary>
    /// Context length (default: 2048, range: 128-32768)
    /// </summary>
    public int ContextLength { get; set; } = 2048;
    
    /// <summary>
    /// Number of threads (default: CPU count / 2, range: 1-CPU count)
    /// </summary>
    public int ThreadCount { get; set; } = Math.Max(1, Environment.ProcessorCount / 2);
    
    /// <summary>
    /// Lock model in memory to prevent swapping
    /// </summary>
    public bool UseMemoryLock { get; set; } = false;
    
    /// <summary>
    /// Use memory mapping for faster loading
    /// </summary>
    public bool UseMemoryMap { get; set; } = true;
    
    /// <summary>
    /// GPU layers to offload (0 = CPU only)
    /// </summary>
    public int GpuLayers { get; set; } = 0;
    
    /// <summary>
    /// Validates the options
    /// </summary>
    public void Validate()
    {
        if (ContextLength < 128 || ContextLength > 32768)
            throw new ArgumentOutOfRangeException(nameof(ContextLength), "Must be between 128 and 32768");
        
        if (ThreadCount < 1 || ThreadCount > Environment.ProcessorCount)
            throw new ArgumentOutOfRangeException(nameof(ThreadCount), $"Must be between 1 and {Environment.ProcessorCount}");
        
        if (GpuLayers < 0)
            throw new ArgumentOutOfRangeException(nameof(GpuLayers), "Must be >= 0");
    }
}

/// <summary>
/// Options for creating an inference context
/// </summary>
public class ContextOptions
{
    /// <summary>
    /// Random seed (-1 for random)
    /// </summary>
    public int Seed { get; set; } = -1;
    
    /// <summary>
    /// Batch size for processing
    /// </summary>
    public int BatchSize { get; set; } = 512;
}

/// <summary>
/// Request parameters for inference
/// </summary>
public class InferenceRequest
{
    /// <summary>
    /// Input prompt (required)
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum tokens to generate (default: 512, range: 1-4096)
    /// </summary>
    public int MaxTokens { get; set; } = 512;
    
    /// <summary>
    /// Temperature (default: 0.7, range: 0.0-2.0)
    /// </summary>
    public float Temperature { get; set; } = 0.7f;
    
    /// <summary>
    /// Top-p sampling (default: 0.9, range: 0.0-1.0)
    /// </summary>
    public float TopP { get; set; } = 0.9f;
    
    /// <summary>
    /// Top-k sampling (default: 40)
    /// </summary>
    public int TopK { get; set; } = 40;
    
    /// <summary>
    /// Repetition penalty (default: 1.1)
    /// </summary>
    public float RepetitionPenalty { get; set; } = 1.1f;
    
    /// <summary>
    /// Stop sequences
    /// </summary>
    public List<string> StopSequences { get; set; } = new();
    
    /// <summary>
    /// Random seed (-1 for random)
    /// </summary>
    public int Seed { get; set; } = -1;
    
    /// <summary>
    /// Validates the request
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(Prompt));
        
        if (MaxTokens < 1 || MaxTokens > 4096)
            throw new ArgumentOutOfRangeException(nameof(MaxTokens), "Must be between 1 and 4096");
        
        if (Temperature < 0.0f || Temperature > 2.0f)
            throw new ArgumentOutOfRangeException(nameof(Temperature), "Must be between 0.0 and 2.0");
        
        if (TopP < 0.0f || TopP > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(TopP), "Must be between 0.0 and 1.0");
        
        if (TopK < 1)
            throw new ArgumentOutOfRangeException(nameof(TopK), "Must be > 0");
        
        if (RepetitionPenalty <= 0.0f)
            throw new ArgumentOutOfRangeException(nameof(RepetitionPenalty), "Must be > 0");
    }
}

/// <summary>
/// Response from inference
/// </summary>
public class InferenceResponse
{
    /// <summary>
    /// Generated text
    /// </summary>
    public string GeneratedText { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of tokens generated
    /// </summary>
    public int TokensGenerated { get; set; }
    
    /// <summary>
    /// Number of tokens in prompt
    /// </summary>
    public int TokensPrompt { get; set; }
    
    /// <summary>
    /// Total inference duration
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Tokens per second
    /// </summary>
    public double TokensPerSecond { get; set; }
    
    /// <summary>
    /// Stop reason (max_tokens, stop_sequence, eos)
    /// </summary>
    public string StopReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of inference
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Token response for streaming
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Token text
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Token index
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// Is this the final token?
    /// </summary>
    public bool IsFinal { get; set; }
}

/// <summary>
/// Model metadata
/// </summary>
public class ModelMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public long ParameterCount { get; set; }
    public string Quantization { get; set; } = string.Empty;
    public int ContextLength { get; set; }
    public long FileSizeBytes { get; set; }
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
}

/// <summary>
/// Engine information
/// </summary>
public class EngineInfo
{
    public string Version { get; set; } = string.Empty;
    public string BuildInfo { get; set; } = string.Empty;
    public List<string> CpuCapabilities { get; set; } = new();
    public bool SupportsGpu { get; set; }
    public bool SupportsStreaming { get; set; } = true;
}
