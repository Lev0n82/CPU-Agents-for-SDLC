using AutonomousAgent.LLM.Exceptions;
using System.Text.Json;

namespace AutonomousAgent.LLM.Models;

public interface IModelManager
{
    Task<ModelCatalog> GetCatalogAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<ModelInfo> DownloadModelAsync(string modelId, IProgress<DownloadProgress>? progress = null, CancellationToken cancellationToken = default);
    Task<List<ModelInfo>> GetInstalledModelsAsync();
    Task<bool> DeleteModelAsync(string modelId);
    Task<bool> VerifyModelAsync(string modelId);
    string ModelsDirectory { get; }
}

public class ModelManager : IModelManager
{
    private readonly string _modelsDirectory;
    private ModelCatalog? _cachedCatalog;
    private DateTime _catalogCacheTime;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public string ModelsDirectory => _modelsDirectory;

    public ModelManager(string? modelsDirectory = null)
    {
        _modelsDirectory = modelsDirectory ?? 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                        "AutonomousAgent", "Models");
        
        Directory.CreateDirectory(_modelsDirectory);
    }

    public async Task<ModelCatalog> GetCatalogAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && _cachedCatalog != null && 
            DateTime.UtcNow - _catalogCacheTime < CacheDuration)
        {
            return _cachedCatalog;
        }

        // Return embedded catalog (in production, would fetch from URL)
        _cachedCatalog = GetEmbeddedCatalog();
        _catalogCacheTime = DateTime.UtcNow;
        
        await Task.CompletedTask;
        return _cachedCatalog;
    }

    public async Task<ModelInfo> DownloadModelAsync(
        string modelId, 
        IProgress<DownloadProgress>? progress = null, 
        CancellationToken cancellationToken = default)
    {
        var catalog = await GetCatalogAsync(cancellationToken: cancellationToken);
        var modelInfo = catalog.Models.FirstOrDefault(m => m.ModelId == modelId);
        
        if (modelInfo == null)
            throw new ModelNotFoundException(modelId);

        var localPath = Path.Combine(_modelsDirectory, $"{modelId}.gguf");
        
        // Mock download for testing
        await Task.Delay(1000, cancellationToken);
        
        // Create a dummy file for testing
        await File.WriteAllTextAsync(localPath, "Mock model file", cancellationToken);
        
        modelInfo.LocalPath = localPath;
        modelInfo.IsInstalled = true;
        modelInfo.InstalledDate = DateTime.UtcNow;
        
        return modelInfo;
    }

    public async Task<List<ModelInfo>> GetInstalledModelsAsync()
    {
        var catalog = await GetCatalogAsync();
        var installedFiles = Directory.GetFiles(_modelsDirectory, "*.gguf");
        
        return catalog.Models
            .Where(m => installedFiles.Any(f => Path.GetFileNameWithoutExtension(f) == m.ModelId))
            .Select(m => {
                m.LocalPath = Path.Combine(_modelsDirectory, $"{m.ModelId}.gguf");
                m.IsInstalled = true;
                return m;
            })
            .ToList();
    }

    public async Task<bool> DeleteModelAsync(string modelId)
    {
        var localPath = Path.Combine(_modelsDirectory, $"{modelId}.gguf");
        
        if (!File.Exists(localPath))
            return false;
        
        File.Delete(localPath);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> VerifyModelAsync(string modelId)
    {
        var localPath = Path.Combine(_modelsDirectory, $"{modelId}.gguf");
        await Task.CompletedTask;
        return File.Exists(localPath);
    }

    private ModelCatalog GetEmbeddedCatalog()
    {
        return new ModelCatalog
        {
            Models = new List<ModelInfo>
            {
                new() {
                    ModelId = "phi-3-mini-4k-q4",
                    Name = "Phi-3 Mini 4K",
                    Description = "Microsoft Phi-3 Mini optimized for CPU inference",
                    ParameterCount = 3_800_000_000,
                    Quantization = "Q4_K_M",
                    FileSizeBytes = 2_300_000_000,
                    DownloadUrl = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf",
                    Sha256Hash = "mock-hash-1"
                },
                new() {
                    ModelId = "qwen2.5-3b-q4",
                    Name = "Qwen 2.5 3B",
                    Description = "Qwen 2.5 3B optimized for code generation",
                    ParameterCount = 3_000_000_000,
                    Quantization = "Q4_K_M",
                    FileSizeBytes = 1_900_000_000,
                    DownloadUrl = "https://huggingface.co/Qwen/Qwen2.5-3B-Instruct-GGUF",
                    Sha256Hash = "mock-hash-2"
                },
                new() {
                    ModelId = "mistral-7b-q4",
                    Name = "Mistral 7B",
                    Description = "Mistral 7B for advanced reasoning",
                    ParameterCount = 7_000_000_000,
                    Quantization = "Q4_K_M",
                    FileSizeBytes = 4_100_000_000,
                    DownloadUrl = "https://huggingface.co/mistralai/Mistral-7B-Instruct-v0.3-GGUF",
                    Sha256Hash = "mock-hash-3"
                }
            },
            LastUpdated = DateTime.UtcNow,
            Version = "1.0"
        };
    }
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
