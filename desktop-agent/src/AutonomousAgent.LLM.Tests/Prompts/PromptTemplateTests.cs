using AutonomousAgent.LLM.Prompts;
using AutonomousAgent.LLM.Exceptions;
using Xunit;

namespace AutonomousAgent.LLM.Tests.Prompts;

public class PromptTemplateTests
{
    [Fact]
    public void Constructor_CreatesTemplate()
    {
        var template = new PromptTemplate(
            "test-id",
            "Test Template",
            "A test template",
            "Hello {{name}}!",
            new[] { "name" });
        
        Assert.Equal("test-id", template.TemplateId);
        Assert.Equal("Test Template", template.Name);
    }

    [Fact]
    public void Render_WithAllVariables_RendersCorrectly()
    {
        var template = new PromptTemplate(
            "test",
            "Test",
            "Test",
            "Hello {{name}}, you are {{age}} years old.",
            new[] { "name", "age" });
        
        var result = template.Render(new Dictionary<string, string>
        {
            ["name"] = "Alice",
            ["age"] = "30"
        });
        
        Assert.Equal("Hello Alice, you are 30 years old.", result);
    }

    [Fact]
    public void Render_MissingRequiredVariable_ThrowsException()
    {
        var template = new PromptTemplate(
            "test",
            "Test",
            "Test",
            "Hello {{name}}!",
            new[] { "name" });
        
        Assert.Throws<MissingVariableException>(() => 
            template.Render(new Dictionary<string, string>()));
    }

    [Fact]
    public void Render_WithOptionalVariables_UsesDefaults()
    {
        var template = new PromptTemplate(
            "test",
            "Test",
            "Test",
            "Hello {{name}}, {{greeting}}!",
            new[] { "name" },
            new Dictionary<string, string> { ["greeting"] = "welcome" });
        
        var result = template.Render(new Dictionary<string, string>
        {
            ["name"] = "Bob"
        });
        
        Assert.Contains("welcome", result);
    }

    [Fact]
    public void Validate_ValidTemplate_ReturnsTrue()
    {
        var template = new PromptTemplate(
            "test",
            "Test",
            "Test",
            "Hello {{name}}!",
            new[] { "name" });
        
        Assert.True(template.Validate());
    }

    [Fact]
    public void Validate_UnclosedBraces_ReturnsFalse()
    {
        var template = new PromptTemplate(
            "test",
            "Test",
            "Test",
            "Hello {{name}!",
            new[] { "name" });
        
        Assert.False(template.Validate());
    }
}
