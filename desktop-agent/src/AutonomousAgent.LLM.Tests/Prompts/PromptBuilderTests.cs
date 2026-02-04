using AutonomousAgent.LLM.Prompts;
using Xunit;

namespace AutonomousAgent.LLM.Tests.Prompts;

public class PromptBuilderTests
{
    [Fact]
    public void WithSystemPrompt_SetsSystemPrompt()
    {
        var builder = new PromptBuilder();
        builder.WithSystemPrompt("You are a helpful assistant");
        var prompt = builder.Build();
        
        Assert.Contains("system", prompt);
        Assert.Contains("helpful assistant", prompt);
    }

    [Fact]
    public void AddUserMessage_AddsMessage()
    {
        var builder = new PromptBuilder();
        builder.AddUserMessage("Hello!");
        var prompt = builder.Build();
        
        Assert.Contains("user", prompt);
        Assert.Contains("Hello!", prompt);
    }

    [Fact]
    public void AddAssistantMessage_AddsMessage()
    {
        var builder = new PromptBuilder();
        builder.AddAssistantMessage("Hi there!");
        var prompt = builder.Build();
        
        Assert.Contains("Hi there!", prompt);
    }

    [Fact]
    public void Build_MultipleMessages_BuildsCorrectly()
    {
        var builder = new PromptBuilder();
        builder.WithSystemPrompt("System")
               .AddUserMessage("User1")
               .AddAssistantMessage("Assistant1")
               .AddUserMessage("User2");
        
        var prompt = builder.Build();
        
        Assert.Contains("System", prompt);
        Assert.Contains("User1", prompt);
        Assert.Contains("Assistant1", prompt);
        Assert.Contains("User2", prompt);
    }

    [Fact]
    public void WithMaxTokens_SetsMaxTokens()
    {
        var builder = new PromptBuilder();
        builder.WithMaxTokens(1024);
        var request = builder.BuildRequest();
        
        Assert.Equal(1024, request.MaxTokens);
    }

    [Fact]
    public void WithTemperature_SetsTemperature()
    {
        var builder = new PromptBuilder();
        builder.WithTemperature(0.5f);
        var request = builder.BuildRequest();
        
        Assert.Equal(0.5f, request.Temperature);
    }

    [Fact]
    public void WithTopP_SetsTopP()
    {
        var builder = new PromptBuilder();
        builder.WithTopP(0.95f);
        var request = builder.BuildRequest();
        
        Assert.Equal(0.95f, request.TopP);
    }

    [Fact]
    public void AddStopSequence_AddsSequence()
    {
        var builder = new PromptBuilder();
        builder.AddStopSequence("STOP");
        var request = builder.BuildRequest();
        
        Assert.Contains("STOP", request.StopSequences);
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        var builder = new PromptBuilder();
        builder.WithSystemPrompt("Test")
               .AddUserMessage("Message")
               .WithMaxTokens(1024);
        
        builder.Reset();
        var request = builder.BuildRequest();
        
        Assert.Equal(512, request.MaxTokens); // Default
    }

    [Fact]
    public void BuildRequest_CreatesValidRequest()
    {
        var builder = new PromptBuilder();
        builder.AddUserMessage("Test message")
               .WithMaxTokens(256)
               .WithTemperature(0.8f);
        
        var request = builder.BuildRequest();
        
        Assert.NotNull(request);
        Assert.NotEmpty(request.Prompt);
        Assert.Equal(256, request.MaxTokens);
        Assert.Equal(0.8f, request.Temperature);
    }
}
