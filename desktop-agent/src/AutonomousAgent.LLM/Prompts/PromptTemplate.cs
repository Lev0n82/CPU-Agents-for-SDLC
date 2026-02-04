using AutonomousAgent.LLM.Exceptions;
using System.Text.RegularExpressions;

namespace AutonomousAgent.LLM.Prompts;

public interface IPromptTemplate
{
    string TemplateId { get; }
    string Name { get; }
    string Description { get; }
    string Content { get; }
    IReadOnlyList<string> RequiredVariables { get; }
    IReadOnlyDictionary<string, string> OptionalVariables { get; }
    string Render(IDictionary<string, string> variables);
    bool Validate();
}

public class PromptTemplate : IPromptTemplate
{
    private static readonly Regex VariableRegex = new(@"\{\{\s*([a-zA-Z0-9_]+)\s*\}\}", RegexOptions.Compiled);
    
    public string TemplateId { get; }
    public string Name { get; }
    public string Description { get; }
    public string Content { get; }
    public IReadOnlyList<string> RequiredVariables { get; }
    public IReadOnlyDictionary<string, string> OptionalVariables { get; }

    public PromptTemplate(
        string templateId,
        string name,
        string description,
        string content,
        IEnumerable<string>? requiredVariables = null,
        IDictionary<string, string>? optionalVariables = null)
    {
        TemplateId = templateId;
        Name = name;
        Description = description;
        Content = content;
        RequiredVariables = (requiredVariables ?? Array.Empty<string>()).ToList();
        OptionalVariables = (optionalVariables ?? new Dictionary<string, string>())
            .ToDictionary(k => k.Key, v => v.Value);
    }

    public string Render(IDictionary<string, string> variables)
    {
        // Check for missing required variables
        var missingVars = RequiredVariables.Where(v => !variables.ContainsKey(v)).ToList();
        if (missingVars.Any())
            throw new MissingVariableException(missingVars.First());

        // Merge with optional variables
        var allVariables = new Dictionary<string, string>(OptionalVariables);
        foreach (var kvp in variables)
            allVariables[kvp.Key] = kvp.Value;

        // Replace variables
        return VariableRegex.Replace(Content, match =>
        {
            var varName = match.Groups[1].Value;
            return allVariables.TryGetValue(varName, out var value) ? value : match.Value;
        });
    }

    public bool Validate()
    {
        try
        {
            // Check for unclosed braces
            var openCount = Content.Count(c => c == '{');
            var closeCount = Content.Count(c => c == '}');
            if (openCount != closeCount)
                return false;

            // Extract all variables from content
            var contentVars = VariableRegex.Matches(Content)
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToHashSet();

            // Check if all variables are defined
            var definedVars = RequiredVariables.Concat(OptionalVariables.Keys).ToHashSet();
            return contentVars.IsSubsetOf(definedVars);
        }
        catch
        {
            return false;
        }
    }
}
