using LLama;
using LLama.Common;
using AutonomousAgent.LLM.Exceptions;

namespace AutonomousAgent.LLM.Core;

public class LLamaSharpModel : ILlamaModel
{
    private readonly LLamaWeights _weights;
    private readonly ModelParams _params;
    private readonly ModelLoadOptions _options;
    private readonly List<ILlamaContext> _contexts = new();
    private bool _disposed;

    public string ModelId { get; }
    public string FilePath { get; }
    public DateTime LoadedAt { get; }
    public ModelMetadata Metadata { get; }

    public LLamaSharpModel(string modelPath, LLamaWeights weights, ModelParams parameters, ModelLoadOptions options)
    {
        FilePath = modelPath;
        _weights = weights;
        _params = parameters;
        _options = options;
        ModelId = Guid.NewGuid().ToString();
        LoadedAt = DateTime.UtcNow;
        
        // Build metadata
        var fileInfo = new FileInfo(modelPath);
        Metadata = new ModelMetadata
        {
            Name = Path.GetFileNameWithoutExtension(modelPath),
            Architecture = "llama",
            ParameterCount = EstimateParameterCount(fileInfo.Length),
            ContextLength = (int)parameters.ContextSize,
            Quantization = DetectQuantization(modelPath),
            FileSizeBytes = fileInfo.Length,
            // CreatedAt not in ModelMetadata
        };
    }

    public ILlamaContext CreateContext(ContextOptions options)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LLamaSharpModel));

        try
        {
            var context = _weights.CreateContext(_params);
            var llamaContext = new LLamaSharpContext(this, context, _params, options);
            _contexts.Add(llamaContext);
            return llamaContext;
        }
        catch (Exception ex)
        {
            throw new ContextCreationException($"Failed to create context: {ex.Message}", ex);
        }
    }

    public IReadOnlyList<ILlamaContext> GetActiveContexts()
    {
        return _contexts.ToList();
    }

    public bool Validate()
    {
        if (_disposed) return false;
        if (!File.Exists(FilePath)) return false;
        return _weights != null;
    }

    private long EstimateParameterCount(long sizeBytes)
    {
        var sizeGB = sizeBytes / (1024.0 * 1024.0 * 1024.0);
        if (sizeGB < 1) return 1_000_000_000;
        if (sizeGB < 2) return 3_000_000_000;
        if (sizeGB < 5) return 7_000_000_000;
        if (sizeGB < 10) return 13_000_000_000;
        return 70_000_000_000;
    }

    private string DetectQuantization(string path)
    {
        var filename = Path.GetFileName(path).ToLower();
        if (filename.Contains("q4_0")) return "Q4_0";
        if (filename.Contains("q4_1")) return "Q4_1";
        if (filename.Contains("q5_0")) return "Q5_0";
        if (filename.Contains("q5_1")) return "Q5_1";
        if (filename.Contains("q8_0")) return "Q8_0";
        if (filename.Contains("f16")) return "F16";
        if (filename.Contains("f32")) return "F32";
        return "Unknown";
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var context in _contexts.ToList())
        {
            context.Dispose();
        }
        _contexts.Clear();

        _weights?.Dispose();
        _disposed = true;
    }
}
