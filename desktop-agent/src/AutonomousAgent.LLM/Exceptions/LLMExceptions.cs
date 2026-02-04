namespace AutonomousAgent.LLM.Exceptions;

/// <summary>
/// Base exception for all LLM-related errors
/// </summary>
public class LLMException : Exception
{
    public LLMException(string message) : base(message) { }
    public LLMException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a model file is invalid or corrupted
/// </summary>
public class InvalidModelException : LLMException
{
    public string ModelPath { get; }
    
    public InvalidModelException(string modelPath, string message) : base(message)
    {
        ModelPath = modelPath;
    }
    
    public InvalidModelException(string modelPath, string message, Exception inner) 
        : base(message, inner)
    {
        ModelPath = modelPath;
    }
}

/// <summary>
/// Thrown when context exceeds maximum token limit
/// </summary>
public class ContextOverflowException : LLMException
{
    public int RequiredTokens { get; }
    public int MaxTokens { get; }
    
    public ContextOverflowException(int required, int max) 
        : base($"Context overflow: {required} tokens required, {max} available")
    {
        RequiredTokens = required;
        MaxTokens = max;
    }
}

/// <summary>
/// Thrown when inference execution fails
/// </summary>
public class InferenceException : LLMException
{
    public InferenceException(string message) : base(message) { }
    public InferenceException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a requested model is not found
/// </summary>
public class ModelNotFoundException : LLMException
{
    public string ModelId { get; }
    
    public ModelNotFoundException(string modelId) 
        : base($"Model not found: {modelId}")
    {
        ModelId = modelId;
    }
}

/// <summary>
/// Thrown when model download fails
/// </summary>
public class DownloadException : LLMException
{
    public string Url { get; }
    
    public DownloadException(string url, string message) : base(message)
    {
        Url = url;
    }
    
    public DownloadException(string url, string message, Exception inner) 
        : base(message, inner)
    {
        Url = url;
    }
}

/// <summary>
/// Thrown when response parsing fails
/// </summary>
public class ParseException : LLMException
{
    public string Response { get; }
    
    public ParseException(string response, string message) : base(message)
    {
        Response = response;
    }
    
    public ParseException(string response, string message, Exception inner) 
        : base(message, inner)
    {
        Response = response;
    }
}

/// <summary>
/// Thrown when a required template variable is missing
/// </summary>
public class MissingVariableException : LLMException
{
    public string VariableName { get; }
    
    public MissingVariableException(string variableName) 
        : base($"Required variable not provided: {variableName}")
    {
        VariableName = variableName;
    }
}
