# Phase 2: LLM Integration - Implementation Specification

**Document Version:** 1.0.0  
**Date:** February 4, 2026  
**Status:** Final Specification

---

## Executive Summary

This document provides comprehensive implementation specifications for Phase 2 of the CPU Agents for SDLC project, focusing on **LLM Integration**. The phase introduces local large language model capabilities using llama.cpp, enabling the autonomous agent to perform natural language understanding, code generation, requirements analysis, and test case generation entirely on CPU without GPU requirements.

The implementation follows a modular architecture with clearly defined interfaces, comprehensive acceptance criteria at function, class, module, and system levels, and built-in self-testing infrastructure to ensure continuous validation.

---

## 1. Architecture Overview

### 1.1 System Components

The LLM Integration phase introduces four primary modules:

| Module | Responsibility | Dependencies |
|--------|---------------|--------------|
| **LLM Engine** | Core llama.cpp integration, model loading, inference execution | llama.cpp native library |
| **Model Manager** | Model discovery, download, versioning, lifecycle management | LLM Engine, File System |
| **Prompt Engineering Framework** | Template management, context injection, response parsing | LLM Engine |
| **Context Manager** | Conversation history, memory management, context window optimization | Prompt Framework |

### 1.2 Technology Stack

The implementation leverages the following technologies:

- **.NET 8.0**: Primary application framework
- **llama.cpp**: CPU-optimized LLM inference engine (via P/Invoke)
- **C# Native Interop**: Bridge between managed .NET and native llama.cpp
- **System.Text.Json**: JSON serialization for prompts and responses
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for unit tests
- **BenchmarkDotNet**: Performance benchmarking

### 1.3 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Autonomous Agent Core                     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ├─────────────────────────────────┐
                            │                                 │
                            ▼                                 ▼
┌────────────────────────────────────┐    ┌──────────────────────────────┐
│      Context Manager               │    │   Prompt Engineering         │
│  - Conversation History            │◄───│   - Template Management      │
│  - Memory Management               │    │   - Context Injection        │
│  - Context Window Optimization     │    │   - Response Parsing         │
└────────────────────────────────────┘    └──────────────────────────────┘
                            │                                 │
                            └─────────────┬───────────────────┘
                                          │
                                          ▼
                            ┌──────────────────────────────┐
                            │      LLM Engine              │
                            │  - Model Loading             │
                            │  - Inference Execution       │
                            │  - Token Management          │
                            └──────────────────────────────┘
                                          │
                                          ▼
                            ┌──────────────────────────────┐
                            │      Model Manager           │
                            │  - Model Discovery           │
                            │  - Download Management       │
                            │  - Version Control           │
                            └──────────────────────────────┘
                                          │
                                          ▼
                            ┌──────────────────────────────┐
                            │   llama.cpp (Native)         │
                            │  - GGUF Model Format         │
                            │  - CPU Inference             │
                            └──────────────────────────────┘
```

---

## 2. Module Specifications

### 2.1 LLM Engine Module

#### 2.1.1 Purpose

Provides the core interface to llama.cpp for model loading, inference execution, and token management. Abstracts native library complexity behind a clean C# API.

#### 2.1.2 Class Design

**Primary Classes:**

1. **`LlamaEngine`** - Main engine class
2. **`LlamaModel`** - Represents a loaded model
3. **`LlamaContext`** - Inference context
4. **`InferenceRequest`** - Request parameters
5. **`InferenceResponse`** - Response with tokens and metadata

#### 2.1.3 API Interface

```csharp
public interface ILlamaEngine
{
    /// <summary>
    /// Loads a model from the specified path
    /// </summary>
    /// <param name="modelPath">Absolute path to GGUF model file</param>
    /// <param name="options">Model loading options</param>
    /// <returns>Loaded model instance</returns>
    Task<ILlamaModel> LoadModelAsync(string modelPath, ModelLoadOptions options);
    
    /// <summary>
    /// Unloads a model and frees resources
    /// </summary>
    Task UnloadModelAsync(ILlamaModel model);
    
    /// <summary>
    /// Gets engine version and capabilities
    /// </summary>
    EngineInfo GetEngineInfo();
}

public interface ILlamaModel
{
    /// <summary>
    /// Model unique identifier
    /// </summary>
    string ModelId { get; }
    
    /// <summary>
    /// Model metadata (parameters, quantization, context length)
    /// </summary>
    ModelMetadata Metadata { get; }
    
    /// <summary>
    /// Creates an inference context
    /// </summary>
    ILlamaContext CreateContext(ContextOptions options);
    
    /// <summary>
    /// Disposes model and releases native resources
    /// </summary>
    void Dispose();
}

public interface ILlamaContext
{
    /// <summary>
    /// Executes inference with the given prompt
    /// </summary>
    /// <param name="request">Inference request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inference response with generated text</returns>
    Task<InferenceResponse> InferAsync(InferenceRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Streams inference tokens as they are generated
    /// </summary>
    IAsyncEnumerable<string> StreamAsync(InferenceRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets context state
    /// </summary>
    void Reset();
}
```

#### 2.1.4 Data Models

```csharp
public class ModelLoadOptions
{
    public int ContextLength { get; set; } = 2048;
    public int ThreadCount { get; set; } = Environment.ProcessorCount / 2;
    public bool UseMemoryLock { get; set; } = false;
    public bool UseMemoryMap { get; set; } = true;
}

public class ContextOptions
{
    public int Seed { get; set; } = -1; // Random seed if -1
    public int BatchSize { get; set; } = 512;
}

public class InferenceRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 512;
    public float Temperature { get; set; } = 0.7f;
    public float TopP { get; set; } = 0.9f;
    public float TopK { get; set; } = 40;
    public float RepetitionPenalty { get; set; } = 1.1f;
    public List<string> StopSequences { get; set; } = new();
}

public class InferenceResponse
{
    public string GeneratedText { get; set; } = string.Empty;
    public int TokensGenerated { get; set; }
    public int TokensPrompt { get; set; }
    public TimeSpan Duration { get; set; }
    public double TokensPerSecond { get; set; }
    public string StopReason { get; set; } = string.Empty; // "max_tokens", "stop_sequence", "eos"
}

public class ModelMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public long ParameterCount { get; set; }
    public string Quantization { get; set; } = string.Empty;
    public int ContextLength { get; set; }
    public long FileSizeBytes { get; set; }
}
```

#### 2.1.5 Acceptance Criteria

**Function-Level:**

| Function | Acceptance Criteria |
|----------|---------------------|
| `LoadModelAsync` | ✓ Successfully loads valid GGUF model<br>✓ Throws `FileNotFoundException` for missing files<br>✓ Throws `InvalidModelException` for corrupted models<br>✓ Completes within 30 seconds for 7B parameter models<br>✓ Returns model with correct metadata |
| `InferAsync` | ✓ Generates coherent text for valid prompts<br>✓ Respects `MaxTokens` limit<br>✓ Achieves 15-30 tokens/second on Intel i7/AMD Ryzen 7<br>✓ Stops at specified stop sequences<br>✓ Returns accurate token counts and timing |
| `StreamAsync` | ✓ Yields tokens incrementally<br>✓ Allows cancellation mid-stream<br>✓ Maintains same quality as `InferAsync` |

**Class-Level:**

| Class | Acceptance Criteria |
|-------|---------------------|
| `LlamaEngine` | ✓ Manages multiple loaded models concurrently<br>✓ Properly disposes native resources<br>✓ Thread-safe for concurrent operations<br>✓ Reports accurate engine version |
| `LlamaModel` | ✓ Supports multiple concurrent contexts<br>✓ Releases memory on disposal<br>✓ Provides accurate metadata |
| `LlamaContext` | ✓ Maintains conversation state across calls<br>✓ Resets cleanly without memory leaks<br>✓ Handles cancellation gracefully |

**Module-Level:**

✓ All classes implement proper `IDisposable` pattern  
✓ No memory leaks after 1000 inference cycles  
✓ Native interop errors are properly wrapped in managed exceptions  
✓ Performance benchmarks meet targets (15-30 tokens/sec)  
✓ 100% unit test coverage for public APIs  

**System-Level:**

✓ Integrates seamlessly with existing self-test framework  
✓ Supports Phi-3-mini, Qwen2.5, Mistral models  
✓ Works on Intel and AMD CPUs without GPU  
✓ Handles out-of-memory conditions gracefully  
✓ Logs all operations for debugging  

---

### 2.2 Model Manager Module

#### 2.2.1 Purpose

Manages the lifecycle of LLM models including discovery, download, versioning, and storage. Provides a catalog of available models and handles automatic updates.

#### 2.2.2 Class Design

**Primary Classes:**

1. **`ModelManager`** - Main manager class
2. **`ModelCatalog`** - Registry of available models
3. **`ModelDownloader`** - Handles model downloads
4. **`ModelInfo`** - Model metadata and location

#### 2.2.3 API Interface

```csharp
public interface IModelManager
{
    /// <summary>
    /// Gets the catalog of available models
    /// </summary>
    Task<ModelCatalog> GetCatalogAsync();
    
    /// <summary>
    /// Downloads a model from the catalog
    /// </summary>
    /// <param name="modelId">Model identifier</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ModelInfo> DownloadModelAsync(string modelId, IProgress<DownloadProgress> progress, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets locally installed models
    /// </summary>
    Task<List<ModelInfo>> GetInstalledModelsAsync();
    
    /// <summary>
    /// Deletes a locally installed model
    /// </summary>
    Task DeleteModelAsync(string modelId);
    
    /// <summary>
    /// Verifies model integrity
    /// </summary>
    Task<bool> VerifyModelAsync(string modelId);
}

public class ModelCatalog
{
    public List<ModelInfo> Models { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Version { get; set; } = "1.0";
}

public class ModelInfo
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long ParameterCount { get; set; }
    public string Quantization { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string Sha256Hash { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public bool IsInstalled { get; set; }
    public DateTime? InstalledDate { get; set; }
}

public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;
    public TimeSpan Elapsed { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
}
```

#### 2.2.4 Default Model Catalog

The system ships with a default catalog of recommended models:

| Model ID | Name | Parameters | Quantization | Size | Use Case |
|----------|------|------------|--------------|------|----------|
| `phi-3-mini-4k-q4` | Phi-3 Mini 4K | 3.8B | Q4_K_M | 2.3 GB | General purpose, fast |
| `qwen2.5-3b-q4` | Qwen 2.5 3B | 3B | Q4_K_M | 1.9 GB | Code generation |
| `mistral-7b-q4` | Mistral 7B | 7B | Q4_K_M | 4.1 GB | Advanced reasoning |

#### 2.2.5 Acceptance Criteria

**Function-Level:**

| Function | Acceptance Criteria |
|----------|---------------------|
| `GetCatalogAsync` | ✓ Returns valid catalog with ≥3 models<br>✓ Caches catalog for 24 hours<br>✓ Falls back to embedded catalog if network fails |
| `DownloadModelAsync` | ✓ Downloads complete model file<br>✓ Verifies SHA256 hash<br>✓ Reports progress every 1%<br>✓ Supports cancellation<br>✓ Resumes interrupted downloads |
| `VerifyModelAsync` | ✓ Validates file exists<br>✓ Checks SHA256 hash<br>✓ Verifies GGUF format |

**Module-Level:**

✓ Stores models in `%APPDATA%\AutonomousAgent\Models`  
✓ Handles concurrent downloads safely  
✓ Cleans up partial downloads on failure  
✓ Maintains download history  

---

### 2.3 Prompt Engineering Framework

#### 2.3.1 Purpose

Provides a structured approach to prompt construction, template management, and response parsing. Enables consistent, reusable prompts for different agent tasks.

#### 2.3.2 Class Design

**Primary Classes:**

1. **`PromptTemplate`** - Template with variables
2. **`PromptBuilder`** - Fluent API for prompt construction
3. **`ResponseParser`** - Extracts structured data from responses

#### 2.3.3 API Interface

```csharp
public interface IPromptTemplate
{
    string TemplateId { get; }
    string Content { get; }
    List<string> Variables { get; }
    
    string Render(Dictionary<string, string> variables);
}

public interface IPromptBuilder
{
    IPromptBuilder WithSystemPrompt(string systemPrompt);
    IPromptBuilder AddUserMessage(string message);
    IPromptBuilder AddAssistantMessage(string message);
    IPromptBuilder WithTemplate(IPromptTemplate template, Dictionary<string, string> variables);
    IPromptBuilder WithMaxTokens(int maxTokens);
    IPromptBuilder WithTemperature(float temperature);
    string Build();
    InferenceRequest BuildRequest();
}

public interface IResponseParser
{
    T Parse<T>(string response) where T : class;
    bool TryParse<T>(string response, out T? result) where T : class;
    string ExtractCodeBlock(string response, string language = "");
    List<string> ExtractList(string response);
}
```

#### 2.3.4 Built-in Templates

The framework includes pre-built templates for common tasks:

**Requirements Analysis Template:**

```
You are an expert requirements analyst. Analyze the following requirement and identify:
1. Functional requirements
2. Non-functional requirements
3. Acceptance criteria
4. Potential ambiguities

Requirement:
{{requirement_text}}

Provide your analysis in JSON format.
```

**Test Case Generation Template:**

```
You are a QA engineer. Generate comprehensive test cases for the following requirement:

{{requirement_text}}

For each test case, include:
- Test ID
- Description
- Preconditions
- Steps
- Expected Result
- Test Type (functional/non-functional)

Output as JSON array.
```

#### 2.3.5 Acceptance Criteria

**Function-Level:**

| Function | Acceptance Criteria |
|----------|---------------------|
| `Render` | ✓ Replaces all variables correctly<br>✓ Throws exception for missing variables<br>✓ Escapes special characters |
| `Parse<T>` | ✓ Deserializes valid JSON responses<br>✓ Handles markdown code blocks<br>✓ Provides clear error messages |

**Module-Level:**

✓ Ships with ≥5 built-in templates  
✓ Supports custom template registration  
✓ Templates are versioned  
✓ Validates template syntax on load  

---

### 2.4 Context Manager

#### 2.4.1 Purpose

Manages conversation history, memory optimization, and context window management to maintain coherent multi-turn interactions within model context limits.

#### 2.4.2 Class Design

**Primary Classes:**

1. **`ContextManager`** - Main manager
2. **`ConversationHistory`** - Message history
3. **`ContextOptimizer`** - Compression and summarization

#### 2.4.3 API Interface

```csharp
public interface IContextManager
{
    void AddMessage(string role, string content);
    List<Message> GetHistory();
    void Clear();
    int GetTokenCount();
    void OptimizeContext(int targetTokens);
    string BuildContextPrompt();
}

public class Message
{
    public string Role { get; set; } = string.Empty; // "system", "user", "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int TokenCount { get; set; }
}
```

#### 2.4.4 Acceptance Criteria

**Function-Level:**

| Function | Acceptance Criteria |
|----------|---------------------|
| `AddMessage` | ✓ Appends message to history<br>✓ Calculates token count<br>✓ Timestamps message |
| `OptimizeContext` | ✓ Reduces context to target tokens<br>✓ Preserves most recent messages<br>✓ Summarizes older messages |

**Module-Level:**

✓ Maintains context within model limits  
✓ Supports sliding window strategy  
✓ Supports summarization strategy  
✓ Thread-safe for concurrent access  

---

## 3. Implementation Plan

### 3.1 Development Phases

| Phase | Tasks | Duration | Dependencies |
|-------|-------|----------|--------------|
| **3.1** | Native llama.cpp integration, P/Invoke bindings | 2 days | llama.cpp library |
| **3.2** | LLM Engine implementation | 3 days | Phase 3.1 |
| **3.3** | Model Manager implementation | 2 days | - |
| **3.4** | Prompt Framework implementation | 2 days | - |
| **3.5** | Context Manager implementation | 1 day | Phase 3.4 |
| **3.6** | Unit tests for all modules | 2 days | Phases 3.2-3.5 |
| **3.7** | Integration tests | 2 days | Phase 3.6 |
| **3.8** | Performance optimization | 1 day | Phase 3.7 |
| **Total** | | **15 days** | |

### 3.2 Step-by-Step Development Guide

#### Step 1: Setup llama.cpp Native Library

1. Download llama.cpp pre-built binaries for Windows x64
2. Place `llama.dll` in `desktop-agent/native/x64/`
3. Create P/Invoke wrapper class `LlamaCppInterop.cs`
4. Define native function signatures

#### Step 2: Implement LLM Engine Core

1. Create `LlamaEngine` class implementing `ILlamaEngine`
2. Implement model loading with native interop
3. Implement inference execution
4. Add error handling and resource management
5. Create unit tests

#### Step 3: Implement Model Manager

1. Create `ModelManager` class
2. Implement catalog loading (embedded JSON)
3. Implement download functionality with progress
4. Add SHA256 verification
5. Create unit tests

#### Step 4: Implement Prompt Framework

1. Create `PromptTemplate` class
2. Implement variable substitution
3. Create `PromptBuilder` with fluent API
4. Implement `ResponseParser` with JSON support
5. Add built-in templates
6. Create unit tests

#### Step 5: Implement Context Manager

1. Create `ContextManager` class
2. Implement message history tracking
3. Add token counting
4. Implement context optimization strategies
5. Create unit tests

#### Step 6: Integration Testing

1. Create end-to-end test scenarios
2. Test full inference pipeline
3. Test model download and loading
4. Test prompt templates with real models
5. Performance benchmarking

---

## 4. Testing Strategy

### 4.1 Unit Testing

**Coverage Target:** 100% of public APIs

**Test Categories:**

1. **Happy Path Tests**: Verify correct behavior with valid inputs
2. **Edge Case Tests**: Test boundary conditions
3. **Error Handling Tests**: Verify exceptions are thrown correctly
4. **Performance Tests**: Ensure operations meet timing requirements

**Example Unit Test:**

```csharp
[Fact]
public async Task LoadModelAsync_ValidModel_ReturnsLoadedModel()
{
    // Arrange
    var engine = new LlamaEngine();
    var modelPath = "models/phi-3-mini-4k-q4.gguf";
    var options = new ModelLoadOptions { ContextLength = 2048 };
    
    // Act
    var model = await engine.LoadModelAsync(modelPath, options);
    
    // Assert
    Assert.NotNull(model);
    Assert.Equal("phi-3-mini-4k-q4", model.ModelId);
    Assert.True(model.Metadata.ParameterCount > 0);
}
```

### 4.2 Integration Testing

**Test Scenarios:**

1. **Model Download and Load**: Download model, verify, load, execute inference
2. **Multi-Turn Conversation**: Maintain context across multiple inferences
3. **Template-Based Generation**: Use templates to generate structured output
4. **Concurrent Inference**: Multiple contexts running simultaneously
5. **Resource Cleanup**: Verify no memory leaks after extended use

**Example Integration Test:**

```csharp
[Fact]
public async Task EndToEnd_RequirementsAnalysis_GeneratesValidOutput()
{
    // Arrange
    var modelManager = new ModelManager();
    var engine = new LlamaEngine();
    var promptBuilder = new PromptBuilder();
    
    // Download and load model
    var modelInfo = await modelManager.DownloadModelAsync("phi-3-mini-4k-q4", null);
    var model = await engine.LoadModelAsync(modelInfo.LocalPath, new ModelLoadOptions());
    var context = model.CreateContext(new ContextOptions());
    
    // Build prompt
    var template = PromptTemplates.RequirementsAnalysis;
    var prompt = promptBuilder
        .WithTemplate(template, new() { ["requirement_text"] = "User shall be able to login" })
        .Build();
    
    // Act
    var response = await context.InferAsync(new InferenceRequest { Prompt = prompt });
    
    // Assert
    Assert.NotEmpty(response.GeneratedText);
    Assert.True(response.TokensPerSecond >= 15);
    var parsed = ResponseParser.Parse<RequirementsAnalysis>(response.GeneratedText);
    Assert.NotNull(parsed);
    Assert.NotEmpty(parsed.FunctionalRequirements);
}
```

### 4.3 Self-Testing Integration

All Phase 2 components integrate with the existing self-test framework:

```csharp
[SelfTest(Level = TestLevel.Module, Category = "LLM")]
public async Task<TestResult> SelfTest_LLMEngine()
{
    var result = new TestResult { TestName = "LLM Engine Module" };
    
    try
    {
        // Test model loading
        var engine = new LlamaEngine();
        var model = await engine.LoadModelAsync("models/test-model.gguf", new ModelLoadOptions());
        
        // Test inference
        var context = model.CreateContext(new ContextOptions());
        var response = await context.InferAsync(new InferenceRequest 
        { 
            Prompt = "Hello, world!", 
            MaxTokens = 10 
        });
        
        // Validate
        if (response.TokensGenerated > 0 && response.TokensPerSecond >= 10)
        {
            result.Success = true;
            result.Message = $"LLM Engine operational. {response.TokensPerSecond:F1} tokens/sec";
        }
        else
        {
            result.Success = false;
            result.Message = "Performance below threshold";
        }
    }
    catch (Exception ex)
    {
        result.Success = false;
        result.Message = $"Test failed: {ex.Message}";
    }
    
    return result;
}
```

---

## 5. Performance Requirements

### 5.1 Inference Performance

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Tokens per second (3B model) | 25-40 | BenchmarkDotNet |
| Tokens per second (7B model) | 15-30 | BenchmarkDotNet |
| Model load time (3B) | <10 seconds | Stopwatch |
| Model load time (7B) | <30 seconds | Stopwatch |
| Memory usage (3B) | <3 GB | Performance counters |
| Memory usage (7B) | <6 GB | Performance counters |

### 5.2 Scalability Requirements

- Support ≥3 concurrent inference contexts
- Handle prompts up to model context length (2048-4096 tokens)
- Maintain performance under continuous operation (24 hours)
- No memory leaks after 10,000 inference cycles

---

## 6. Security Considerations

### 6.1 Model Integrity

- All downloaded models verified via SHA256 hash
- Models stored in user-specific AppData directory
- File permissions restrict access to current user

### 6.2 Prompt Injection Protection

- Input sanitization for user-provided prompts
- Template validation to prevent code injection
- Response parsing with strict schema validation

### 6.3 Resource Limits

- Maximum inference time: 120 seconds (configurable)
- Maximum memory per context: 8 GB
- Automatic context cleanup on timeout

---

## 7. Configuration

### 7.1 Configuration File

`appsettings.json` additions:

```json
{
  "LLM": {
    "ModelsDirectory": "%APPDATA%\\AutonomousAgent\\Models",
    "DefaultModel": "phi-3-mini-4k-q4",
    "DefaultContextLength": 2048,
    "DefaultThreadCount": 0,
    "InferenceTimeout": 120,
    "EnableLogging": true,
    "CatalogUrl": "https://cdn.example.com/model-catalog.json"
  }
}
```

### 7.2 Environment Variables

- `LLAMA_CPP_PATH`: Override path to llama.cpp library
- `LLAMA_LOG_LEVEL`: Set logging verbosity (0-5)

---

## 8. Monitoring and Logging

### 8.1 Metrics

The system exposes the following metrics:

- Total inferences executed
- Average tokens per second
- Model load count
- Active contexts
- Memory usage
- Error rate

### 8.2 Logging

All operations logged with structured logging:

```csharp
_logger.LogInformation(
    "Inference completed. Model={ModelId}, Tokens={Tokens}, Duration={Duration}ms, TPS={TPS}",
    model.ModelId,
    response.TokensGenerated,
    response.Duration.TotalMilliseconds,
    response.TokensPerSecond
);
```

---

## 9. Deliverables

### 9.1 Code Deliverables

1. **Source Code**
   - `AutonomousAgent.LLM` project
   - All classes and interfaces
   - Native interop layer
   - Configuration files

2. **Tests**
   - Unit tests (xUnit)
   - Integration tests
   - Performance benchmarks

3. **Documentation**
   - API documentation (XML comments)
   - Usage examples
   - Troubleshooting guide

### 9.2 Artifacts

1. **Model Catalog** (`model-catalog.json`)
2. **Built-in Templates** (embedded resources)
3. **Sample Models** (for testing)
4. **Performance Benchmarks** (results report)

---

## 10. Success Criteria Summary

### 10.1 Function-Level Success Criteria

✓ All public methods have unit tests with ≥95% code coverage  
✓ All functions meet documented acceptance criteria  
✓ No critical or high-severity bugs  

### 10.2 Class-Level Success Criteria

✓ All classes implement proper resource management  
✓ All classes are thread-safe where required  
✓ All classes have comprehensive XML documentation  

### 10.3 Module-Level Success Criteria

✓ LLM Engine achieves 15-30 tokens/sec on target hardware  
✓ Model Manager successfully downloads and verifies models  
✓ Prompt Framework supports all built-in templates  
✓ Context Manager maintains conversation state correctly  

### 10.4 System-Level Success Criteria

✓ Phase 2 integrates seamlessly with existing agent  
✓ Self-tests pass at all levels (function, class, module, system)  
✓ Performance benchmarks meet targets  
✓ No memory leaks or resource exhaustion  
✓ System operates continuously for 24 hours without degradation  

---

## 11. References

1. [llama.cpp GitHub Repository](https://github.com/ggerganov/llama.cpp) - CPU-optimized LLM inference engine
2. [GGUF Format Specification](https://github.com/ggerganov/ggml/blob/master/docs/gguf.md) - Model file format
3. [Phi-3 Technical Report](https://arxiv.org/abs/2404.14219) - Microsoft Phi-3 model details
4. [Qwen2.5 Model Card](https://huggingface.co/Qwen/Qwen2.5-3B) - Qwen model specifications
5. [.NET P/Invoke Documentation](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke) - Native interop guidance

---

**End of Specification Document**
