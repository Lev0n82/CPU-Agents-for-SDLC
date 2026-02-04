using AutonomousAgent.LLM.Core;
using AutonomousAgent.LLM.Exceptions;
using Xunit;

namespace AutonomousAgent.LLM.Tests.Core;

public class MockLlamaEngineTests
{
    [Fact]
    public void Constructor_CreatesEngine()
    {
        using var engine = new MockLlamaEngine();
        Assert.NotNull(engine);
    }

    [Fact]
    public void GetEngineInfo_ReturnsValidInfo()
    {
        using var engine = new MockLlamaEngine();
        var info = engine.GetEngineInfo();
        
        Assert.NotNull(info);
        Assert.Equal("Mock Llama Engine", info.Version);
        Assert.True(info.SupportsStreaming);
    }

    [Fact]
    public async Task LoadModel_ValidPath_LoadsSuccessfully()
    {
        using var engine = new MockLlamaEngine();
        var options = new ModelLoadOptions { ContextLength = 2048 };
        
        var model = await engine.LoadModelAsync("test-model.gguf", options);
        
        Assert.NotNull(model);
    }

    [Fact]
    public async Task LoadModel_InvalidPath_ThrowsException()
    {
        using var engine = new MockLlamaEngine();
        var options = new ModelLoadOptions { ContextLength = 2048 };
        
        await Assert.ThrowsAsync<InvalidModelException>(() => 
            engine.LoadModelAsync("", options));
    }

    [Fact]
    public async Task LoadModel_MultipleConcurrent_Succeeds()
    {
        using var engine = new MockLlamaEngine();
        var options = new ModelLoadOptions { ContextLength = 2048 };
        
        var model1 = await engine.LoadModelAsync("model1.gguf", options);
        var model2 = await engine.LoadModelAsync("model2.gguf", options);
        
        Assert.NotNull(model1);
        Assert.NotNull(model2);
    }

    [Fact]
    public async Task UnloadModel_LoadedModel_UnloadsSuccessfully()
    {
        using var engine = new MockLlamaEngine();
        var options = new ModelLoadOptions { ContextLength = 2048 };
        var model = await engine.LoadModelAsync("test.gguf", options);
        
        await engine.UnloadModelAsync(model);
        
        // Should not throw
    }

    [Fact]
    public async Task Dispose_UnloadsAllModels()
    {
        var engine = new MockLlamaEngine();
        var options = new ModelLoadOptions { ContextLength = 2048 };
        await engine.LoadModelAsync("test.gguf", options);
        
        engine.Dispose();
        
        // Should not throw
    }
}
