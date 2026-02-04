using AutonomousAgent.LLM.Models;
using Xunit;

namespace AutonomousAgent.LLM.Tests.Models;

public class ModelManagerTests
{
    [Fact]
    public async Task GetCatalog_ReturnsCatalog()
    {
        var manager = new ModelManager();
        var catalog = await manager.GetCatalogAsync();
        
        Assert.NotNull(catalog);
        Assert.NotEmpty(catalog.Models);
        Assert.True(catalog.Models.Count >= 3);
    }

    [Fact]
    public async Task GetCatalog_CachesResults()
    {
        var manager = new ModelManager();
        var catalog1 = await manager.GetCatalogAsync();
        var catalog2 = await manager.GetCatalogAsync();
        
        Assert.Same(catalog1, catalog2);
    }

    [Fact]
    public async Task DownloadModel_ValidId_ReturnsModelInfo()
    {
        var manager = new ModelManager();
        var progress = new Progress<DownloadProgress>();
        
        var modelInfo = await manager.DownloadModelAsync("phi-3-mini-4k-q4", progress);
        
        Assert.NotNull(modelInfo);
        Assert.Equal("phi-3-mini-4k-q4", modelInfo.ModelId);
    }

    [Fact]
    public async Task DownloadModel_InvalidId_ThrowsException()
    {
        var manager = new ModelManager();
        var progress = new Progress<DownloadProgress>();
        
        await Assert.ThrowsAsync<ArgumentException>(() => 
            manager.DownloadModelAsync("invalid-model", progress));
    }

    [Fact]
    public async Task GetInstalledModels_AfterDownload_IncludesModel()
    {
        var manager = new ModelManager();
        var progress = new Progress<DownloadProgress>();
        
        await manager.DownloadModelAsync("phi-3-mini-4k-q4", progress);
        var installed = await manager.GetInstalledModelsAsync();
        
        Assert.Contains(installed, m => m.ModelId == "phi-3-mini-4k-q4");
    }

    [Fact]
    public async Task VerifyModel_DownloadedModel_ReturnsTrue()
    {
        var manager = new ModelManager();
        var progress = new Progress<DownloadProgress>();
        
        var modelInfo = await manager.DownloadModelAsync("phi-3-mini-4k-q4", progress);
        var isValid = await manager.VerifyModelAsync(modelInfo.LocalPath);
        
        Assert.True(isValid);
    }

    [Fact]
    public async Task DeleteModel_ExistingModel_DeletesSuccessfully()
    {
        var manager = new ModelManager();
        var progress = new Progress<DownloadProgress>();
        
        var modelInfo = await manager.DownloadModelAsync("phi-3-mini-4k-q4", progress);
        await manager.DeleteModelAsync(modelInfo.ModelId);
        
        var installed = await manager.GetInstalledModelsAsync();
        Assert.DoesNotContain(installed, m => m.ModelId == "phi-3-mini-4k-q4");
    }
}
