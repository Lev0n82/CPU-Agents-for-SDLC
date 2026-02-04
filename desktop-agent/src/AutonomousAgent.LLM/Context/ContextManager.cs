using AutonomousAgent.LLM.Core;

namespace AutonomousAgent.LLM.Context;

public enum OptimizationStrategy
{
    SlidingWindow,
    Summarization,
    ImportanceBased
}

public class Message
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int TokenCount { get; set; }
}

public interface IContextManager
{
    void AddMessage(string role, string content);
    IReadOnlyList<Message> GetHistory();
    void Clear();
    int GetTokenCount();
    void OptimizeContext(int targetTokens, OptimizationStrategy strategy = OptimizationStrategy.SlidingWindow);
    string BuildContextPrompt();
    IReadOnlyList<Message> GetRecentMessages(int count);
}

public class ContextManager : IContextManager
{
    private readonly List<Message> _messages = new();
    private readonly object _lock = new();

    public void AddMessage(string role, string content)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty", nameof(role));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        lock (_lock)
        {
            var message = new Message
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow,
                TokenCount = EstimateTokenCount(content)
            };

            _messages.Add(message);
        }
    }

    public IReadOnlyList<Message> GetHistory()
    {
        lock (_lock)
        {
            return _messages.ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _messages.Clear();
        }
    }

    public int GetTokenCount()
    {
        lock (_lock)
        {
            return _messages.Sum(m => m.TokenCount);
        }
    }

    public void OptimizeContext(int targetTokens, OptimizationStrategy strategy = OptimizationStrategy.SlidingWindow)
    {
        lock (_lock)
        {
            var currentTokens = GetTokenCount();
            if (currentTokens <= targetTokens)
                return;

            switch (strategy)
            {
                case OptimizationStrategy.SlidingWindow:
                    OptimizeSlidingWindow(targetTokens);
                    break;
                case OptimizationStrategy.Summarization:
                    OptimizeSummarization(targetTokens);
                    break;
                case OptimizationStrategy.ImportanceBased:
                    OptimizeImportanceBased(targetTokens);
                    break;
            }
        }
    }

    public string BuildContextPrompt()
    {
        lock (_lock)
        {
            var parts = new List<string>();

            foreach (var message in _messages)
            {
                parts.Add($"<|{message.Role}|>\n{message.Content}\n");
            }

            return string.Join("", parts);
        }
    }

    public IReadOnlyList<Message> GetRecentMessages(int count)
    {
        lock (_lock)
        {
            return _messages.TakeLast(count).ToList();
        }
    }

    private void OptimizeSlidingWindow(int targetTokens)
    {
        // Keep most recent messages
        var tokensRemoved = 0;
        var tokensNeeded = GetTokenCount() - targetTokens;

        while (tokensRemoved < tokensNeeded && _messages.Count > 1)
        {
            tokensRemoved += _messages[0].TokenCount;
            _messages.RemoveAt(0);
        }
    }

    private void OptimizeSummarization(int targetTokens)
    {
        // For now, use sliding window (summarization requires LLM)
        OptimizeSlidingWindow(targetTokens);
    }

    private void OptimizeImportanceBased(int targetTokens)
    {
        // For now, use sliding window (importance scoring requires LLM)
        OptimizeSlidingWindow(targetTokens);
    }

    private int EstimateTokenCount(string text)
    {
        // Rough estimation: ~4 characters per token
        return Math.Max(1, text.Length / 4);
    }
}
