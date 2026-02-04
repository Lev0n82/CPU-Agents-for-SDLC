namespace AutonomousAgent.LLM.Core;

/// <summary>
/// Core engine for LLM operations using llama.cpp
/// </summary>
public interface ILlamaEngine : IDisposable
{
    /// <summary>
    /// Loads a GGUF model from the specified path
    /// </summary>
    /// <param name="modelPath">Absolute path to the GGUF model file</param>
    /// <param name="options">Model loading options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Loaded model instance</returns>
    Task<ILlamaModel> LoadModelAsync(
        string modelPath, 
        ModelLoadOptions options, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unloads a model and releases all associated resources
    /// </summary>
    Task UnloadModelAsync(ILlamaModel model);
    
    /// <summary>
    /// Gets information about the llama.cpp engine
    /// </summary>
    EngineInfo GetEngineInfo();
    
    /// <summary>
    /// Gets all currently loaded models
    /// </summary>
    IReadOnlyList<ILlamaModel> GetLoadedModels();
    
    /// <summary>
    /// Checks if the engine is initialized and ready
    /// </summary>
    bool IsReady { get; }
}

/// <summary>
/// Represents a loaded LLM model
/// </summary>
public interface ILlamaModel : IDisposable
{
    /// <summary>
    /// Unique identifier for this model instance
    /// </summary>
    string ModelId { get; }
    
    /// <summary>
    /// Model metadata
    /// </summary>
    ModelMetadata Metadata { get; }
    
    /// <summary>
    /// Path to the model file
    /// </summary>
    string FilePath { get; }
    
    /// <summary>
    /// Timestamp when the model was loaded
    /// </summary>
    DateTime LoadedAt { get; }
    
    /// <summary>
    /// Creates a new inference context for this model
    /// </summary>
    ILlamaContext CreateContext(ContextOptions options);
    
    /// <summary>
    /// Gets all active contexts for this model
    /// </summary>
    IReadOnlyList<ILlamaContext> GetActiveContexts();
    
    /// <summary>
    /// Validates model integrity
    /// </summary>
    bool Validate();
}

/// <summary>
/// Inference context for executing LLM operations
/// </summary>
public interface ILlamaContext : IDisposable
{
    /// <summary>
    /// Unique context identifier
    /// </summary>
    string ContextId { get; }
    
    /// <summary>
    /// Parent model reference
    /// </summary>
    ILlamaModel Model { get; }
    
    /// <summary>
    /// Current token count in context
    /// </summary>
    int CurrentTokenCount { get; }
    
    /// <summary>
    /// Maximum context length
    /// </summary>
    int MaxContextLength { get; }
    
    /// <summary>
    /// Executes inference
    /// </summary>
    Task<InferenceResponse> InferAsync(
        InferenceRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Streams inference tokens as they are generated
    /// </summary>
    IAsyncEnumerable<TokenResponse> StreamAsync(
        InferenceRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets context state
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Estimates token count for text
    /// </summary>
    int EstimateTokenCount(string text);
}
