using AutonomousAgent.LLM.Prompts;
using AutonomousAgent.LLM.Exceptions;
using Xunit;

namespace AutonomousAgent.LLM.Tests.Prompts;

public class TestModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class ResponseParserTests
{
    [Fact]
    public void Parse_ValidJson_ParsesCorrectly()
    {
        var parser = new ResponseParser();
        var json = "{\"name\": \"Alice\", \"age\": 30}";
        
        var result = parser.Parse<TestModel>(json);
        
        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Parse_JsonInCodeBlock_ExtractsAndParses()
    {
        var parser = new ResponseParser();
        var response = "Here is the data:\n```json\n{\"name\": \"Bob\", \"age\": 25}\n```";
        
        var result = parser.Parse<TestModel>(response);
        
        Assert.NotNull(result);
        Assert.Equal("Bob", result.Name);
        Assert.Equal(25, result.Age);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsException()
    {
        var parser = new ResponseParser();
        var invalid = "not json";
        
        Assert.Throws<ParseException>(() => parser.Parse<TestModel>(invalid));
    }

    [Fact]
    public void TryParse_ValidJson_ReturnsTrue()
    {
        var parser = new ResponseParser();
        var json = "{\"name\": \"Charlie\", \"age\": 35}";
        
        var success = parser.TryParse<TestModel>(json, out var result);
        
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("Charlie", result.Name);
    }

    [Fact]
    public void TryParse_InvalidJson_ReturnsFalse()
    {
        var parser = new ResponseParser();
        var invalid = "not json";
        
        var success = parser.TryParse<TestModel>(invalid, out var result);
        
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void ExtractCodeBlock_WithLanguage_ExtractsCorrectBlock()
    {
        var parser = new ResponseParser();
        var response = "```csharp\nvar x = 10;\n```";
        
        var code = parser.ExtractCodeBlock(response, "csharp");
        
        Assert.Equal("var x = 10;", code);
    }

    [Fact]
    public void ExtractCodeBlock_WithoutLanguage_ExtractsFirstBlock()
    {
        var parser = new ResponseParser();
        var response = "```\nsome code\n```";
        
        var code = parser.ExtractCodeBlock(response);
        
        Assert.Equal("some code", code);
    }

    [Fact]
    public void ExtractList_BulletPoints_ExtractsItems()
    {
        var parser = new ResponseParser();
        var response = "- Item 1\n- Item 2\n- Item 3";
        
        var list = parser.ExtractList(response);
        
        Assert.Equal(3, list.Count);
        Assert.Contains("Item 1", list);
        Assert.Contains("Item 2", list);
        Assert.Contains("Item 3", list);
    }

    [Fact]
    public void ExtractList_NumberedList_ExtractsItems()
    {
        var parser = new ResponseParser();
        var response = "1. First\n2. Second\n3. Third";
        
        var list = parser.ExtractList(response);
        
        Assert.Equal(3, list.Count);
        Assert.Contains("First", list);
    }

    [Fact]
    public void ExtractKeyValuePairs_ValidFormat_ExtractsPairs()
    {
        var parser = new ResponseParser();
        var response = "Name: Alice\nAge: 30\nCity: NYC";
        
        var pairs = parser.ExtractKeyValuePairs(response);
        
        Assert.Equal(3, pairs.Count);
        Assert.Equal("Alice", pairs["Name"]);
        Assert.Equal("30", pairs["Age"]);
        Assert.Equal("NYC", pairs["City"]);
    }
}
