using AutonomousAgent.LLM.Exceptions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AutonomousAgent.LLM.Prompts;

public interface IResponseParser
{
    T Parse<T>(string response) where T : class;
    bool TryParse<T>(string response, out T? result) where T : class;
    string ExtractCodeBlock(string response, string language = "");
    List<string> ExtractList(string response);
    Dictionary<string, string> ExtractKeyValuePairs(string response);
}

public class ResponseParser : IResponseParser
{
    private static readonly Regex CodeBlockRegex = new(
        @"```(?:([a-z]+))?\n([\s\S]*?)```", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static readonly Regex ListItemRegex = new(
        @"^\s*(?:[\d]+\.|[-*+])\s+(.+)$", 
        RegexOptions.Compiled | RegexOptions.Multiline);
    
    private static readonly Regex KeyValueRegex = new(
        @"^\s*([^:]+):\s*(.+)$", 
        RegexOptions.Compiled | RegexOptions.Multiline);

    public T Parse<T>(string response) where T : class
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new ParseException(response, "Response is empty");

        try
        {
            // Try to extract JSON from markdown code block
            var codeBlock = ExtractCodeBlock(response, "json");
            var jsonText = string.IsNullOrWhiteSpace(codeBlock) ? response : codeBlock;

            var result = JsonSerializer.Deserialize<T>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new ParseException(response, "Deserialization returned null");

            return result;
        }
        catch (JsonException ex)
        {
            throw new ParseException(response, $"Failed to parse JSON: {ex.Message}", ex);
        }
    }

    public bool TryParse<T>(string response, out T? result) where T : class
    {
        try
        {
            result = Parse<T>(response);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public string ExtractCodeBlock(string response, string language = "")
    {
        if (string.IsNullOrWhiteSpace(response))
            return string.Empty;

        var matches = CodeBlockRegex.Matches(response);
        
        foreach (Match match in matches)
        {
            var blockLang = match.Groups[1].Value;
            var code = match.Groups[2].Value;

            if (string.IsNullOrEmpty(language) || 
                blockLang.Equals(language, StringComparison.OrdinalIgnoreCase))
            {
                return code.Trim();
            }
        }

        return string.Empty;
    }

    public List<string> ExtractList(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return new List<string>();

        var matches = ListItemRegex.Matches(response);
        return matches.Select(m => m.Groups[1].Value.Trim()).ToList();
    }

    public Dictionary<string, string> ExtractKeyValuePairs(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return new Dictionary<string, string>();

        var matches = KeyValueRegex.Matches(response);
        var result = new Dictionary<string, string>();

        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim();
            result[key] = value;
        }

        return result;
    }
}
