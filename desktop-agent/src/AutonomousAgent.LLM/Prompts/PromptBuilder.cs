using AutonomousAgent.LLM.Core;

namespace AutonomousAgent.LLM.Prompts;

public interface IPromptBuilder
{
    IPromptBuilder WithSystemPrompt(string systemPrompt);
    IPromptBuilder AddUserMessage(string message);
    IPromptBuilder AddAssistantMessage(string message);
    IPromptBuilder WithTemplate(IPromptTemplate template, IDictionary<string, string> variables);
    IPromptBuilder WithMaxTokens(int maxTokens);
    IPromptBuilder WithTemperature(float temperature);
    IPromptBuilder WithTopP(float topP);
    IPromptBuilder AddStopSequence(string sequence);
    string Build();
    InferenceRequest BuildRequest();
    IPromptBuilder Reset();
}

public class PromptBuilder : IPromptBuilder
{
    private string? _systemPrompt;
    private readonly List<(string role, string content)> _messages = new();
    private int _maxTokens = 512;
    private float _temperature = 0.7f;
    private float _topP = 0.9f;
    private readonly List<string> _stopSequences = new();

    public IPromptBuilder WithSystemPrompt(string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
            throw new ArgumentException("System prompt cannot be empty", nameof(systemPrompt));
        _systemPrompt = systemPrompt;
        return this;
    }

    public IPromptBuilder AddUserMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));
        _messages.Add(("user", message));
        return this;
    }

    public IPromptBuilder AddAssistantMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));
        _messages.Add(("assistant", message));
        return this;
    }

    public IPromptBuilder WithTemplate(IPromptTemplate template, IDictionary<string, string> variables)
    {
        var rendered = template.Render(variables);
        _messages.Add(("user", rendered));
        return this;
    }

    public IPromptBuilder WithMaxTokens(int maxTokens)
    {
        if (maxTokens < 1 || maxTokens > 4096)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "Must be between 1 and 4096");
        _maxTokens = maxTokens;
        return this;
    }

    public IPromptBuilder WithTemperature(float temperature)
    {
        if (temperature < 0.0f || temperature > 2.0f)
            throw new ArgumentOutOfRangeException(nameof(temperature), "Must be between 0.0 and 2.0");
        _temperature = temperature;
        return this;
    }

    public IPromptBuilder WithTopP(float topP)
    {
        if (topP < 0.0f || topP > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(topP), "Must be between 0.0 and 1.0");
        _topP = topP;
        return this;
    }

    public IPromptBuilder AddStopSequence(string sequence)
    {
        if (!string.IsNullOrWhiteSpace(sequence))
            _stopSequences.Add(sequence);
        return this;
    }

    public string Build()
    {
        var parts = new List<string>();

        if (_systemPrompt != null)
            parts.Add($"<|system|>\n{_systemPrompt}\n");

        foreach (var (role, content) in _messages)
        {
            parts.Add($"<|{role}|>\n{content}\n");
        }

        parts.Add("<|assistant|>\n");

        return string.Join("", parts);
    }

    public InferenceRequest BuildRequest()
    {
        return new InferenceRequest
        {
            Prompt = Build(),
            MaxTokens = _maxTokens,
            Temperature = _temperature,
            TopP = _topP,
            StopSequences = new List<string>(_stopSequences)
        };
    }

    public IPromptBuilder Reset()
    {
        _systemPrompt = null;
        _messages.Clear();
        _maxTokens = 512;
        _temperature = 0.7f;
        _topP = 0.9f;
        _stopSequences.Clear();
        return this;
    }
}
