using AutonomousAgent.LLM.Context;
using Xunit;

namespace AutonomousAgent.LLM.Tests.Context;

public class ContextManagerTests
{
    [Fact]
    public void AddMessage_AddsToHistory()
    {
        var manager = new ContextManager();
        manager.AddMessage("user", "Hello");
        
        var history = manager.GetHistory();
        
        Assert.Single(history);
        Assert.Equal("user", history[0].Role);
        Assert.Equal("Hello", history[0].Content);
    }

    [Fact]
    public void AddMessage_EmptyRole_ThrowsException()
    {
        var manager = new ContextManager();
        
        Assert.Throws<ArgumentException>(() => 
            manager.AddMessage("", "content"));
    }

    [Fact]
    public void AddMessage_EmptyContent_ThrowsException()
    {
        var manager = new ContextManager();
        
        Assert.Throws<ArgumentException>(() => 
            manager.AddMessage("user", ""));
    }

    [Fact]
    public void GetHistory_ReturnsAllMessages()
    {
        var manager = new ContextManager();
        manager.AddMessage("user", "Message 1");
        manager.AddMessage("assistant", "Response 1");
        manager.AddMessage("user", "Message 2");
        
        var history = manager.GetHistory();
        
        Assert.Equal(3, history.Count);
    }

    [Fact]
    public void Clear_RemovesAllMessages()
    {
        var manager = new ContextManager();
        manager.AddMessage("user", "Message");
        
        manager.Clear();
        
        Assert.Empty(manager.GetHistory());
    }

    [Fact]
    public void GetTokenCount_CalculatesCorrectly()
    {
        var manager = new ContextManager();
        manager.AddMessage("user", "Hello world");
        
        var count = manager.GetTokenCount();
        
        Assert.True(count > 0);
    }

    [Fact]
    public void OptimizeContext_SlidingWindow_RemovesOldMessages()
    {
        var manager = new ContextManager();
        for (int i = 0; i < 10; i++)
        {
            manager.AddMessage("user", new string('x', 1000)); // Large messages
        }
        
        manager.OptimizeContext(500, OptimizationStrategy.SlidingWindow);
        
        Assert.True(manager.GetTokenCount() <= 500);
    }

    [Fact]
    public void BuildContextPrompt_FormatsCorrectly()
    {
        var manager = new ContextManager();
        manager.AddMessage("user", "Hello");
        manager.AddMessage("assistant", "Hi");
        
        var prompt = manager.BuildContextPrompt();
        
        Assert.Contains("user", prompt);
        Assert.Contains("Hello", prompt);
        Assert.Contains("assistant", prompt);
        Assert.Contains("Hi", prompt);
    }

    [Fact]
    public void GetRecentMessages_ReturnsLastN()
    {
        var manager = new ContextManager();
        for (int i = 0; i < 5; i++)
        {
            manager.AddMessage("user", $"Message {i}");
        }
        
        var recent = manager.GetRecentMessages(2);
        
        Assert.Equal(2, recent.Count);
        Assert.Contains("Message 4", recent[1].Content);
    }

    [Fact]
    public void ThreadSafety_ConcurrentAdds_AllAdded()
    {
        var manager = new ContextManager();
        var tasks = new List<Task>();
        
        for (int i = 0; i < 100; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() => 
                manager.AddMessage("user", $"Message {index}")));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        Assert.Equal(100, manager.GetHistory().Count);
    }
}
