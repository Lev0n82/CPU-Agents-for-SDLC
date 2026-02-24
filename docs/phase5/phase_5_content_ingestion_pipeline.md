# Phase 5: Microsoft Learn Content Ingestion Pipeline
## Overview

The **Content Ingestion Pipeline** crawls, parses, and structures technical documentation from Microsoft Learn, Azure DevOps API docs, and other authoritative sources to build a comprehensive knowledge base for AI model training. The pipeline extracts text, code samples, and metadata, constructs a knowledge graph of entities and relationships, and generates synthetic training data for model fine-tuning.

**Key Capabilities:**
- Automated crawling of 100,000+ documentation pages
- Intelligent parsing extracting text, code, images, and metadata
- Knowledge graph construction with 50,000+ entities and 200,000+ relationships
- Incremental updates capturing new content daily
- Synthetic question generation for AI Arena
- Support for on-premise deployment with complete data sovereignty

---

## Architecture

### System Components

| **Component** | **Responsibility** | **Technology** |
|---------------|-------------------|----------------|
| ContentCrawler | Web scraping and HTML parsing | Playwright + BeautifulSoup |
| DocumentParser | Extract structured data from HTML | Custom parsers + Regex |
| KnowledgeGraphBuilder | Construct entity-relationship graph | Neo4j + spaCy NER |
| SyntheticQuestionGenerator | Generate training questions | GPT-4 + validation pipeline |
| IncrementalUpdater | Detect and ingest new content | Content hashing + diff detection |
| StorageManager | Persist raw and processed data | PostgreSQL + S3-compatible storage |

---

## Module 1: Content Crawler

### Purpose

Automated web scraping system that respects robots.txt, enforces rate limits, and handles authentication for restricted content.

### Crawl Strategy

**Seed URLs:**
```
https://learn.microsoft.com/en-us/azure/devops/
https://learn.microsoft.com/en-us/dotnet/csharp/
https://learn.microsoft.com/en-us/aspnet/core/
https://learn.microsoft.com/en-us/azure/devops/rest-api/
https://playwright.dev/docs/intro
https://www.selenium.dev/documentation/
```

**Crawl Depth:** 5 levels from seed URL  
**Max Pages:** 100,000 pages  
**Rate Limit:** 10 requests/second per domain  
**Timeout:** 30 seconds per page  
**Retry Policy:** 3 retries with exponential backoff (1s, 2s, 4s)

### Implementation

```csharp
public class ContentCrawler
{
    private readonly IPlaywrightService _playwright;
    private readonly IRobotsTxtParser _robotsParser;
    private readonly IRateLimiter _rateLimiter;
    private readonly IContentHasher _hasher;
    
    public async Task<CrawlJob> StartCrawlAsync(CrawlConfig config)
    {
        var job = new CrawlJob
        {
            Id = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            Status = CrawlStatus.Running,
            Config = config
        };
        
        var queue = new Queue<CrawlTask>();
        var visited = new HashSet<string>();
        
        // Initialize queue with seed URLs
        foreach (var seedUrl in config.SeedUrls)
        {
            queue.Enqueue(new CrawlTask { Url = seedUrl, Depth = 0 });
        }
        
        while (queue.Count > 0 && visited.Count < config.MaxPages)
        {
            var task = queue.Dequeue();
            
            // Skip if already visited or exceeds max depth
            if (visited.Contains(task.Url) || task.Depth > config.MaxDepth)
                continue;
            
            // Check robots.txt
            if (!await _robotsParser.IsAllowedAsync(task.Url, config.UserAgent))
            {
                _logger.LogWarning($"Blocked by robots.txt: {task.Url}");
                continue;
            }
            
            // Enforce rate limit
            await _rateLimiter.WaitAsync(GetDomain(task.Url));
            
            try
            {
                // Fetch page content
                var document = await FetchDocumentAsync(task.Url);
                
                // Check if content changed (for incremental crawls)
                var contentHash = _hasher.ComputeHash(document.Html);
                if (await IsContentUnchangedAsync(task.Url, contentHash))
                {
                    _logger.LogInformation($"Skipping unchanged content: {task.Url}");
                    continue;
                }
                
                // Store document
                await _storage.SaveDocumentAsync(document);
                visited.Add(task.Url);
                
                // Extract and queue child links
                var links = ExtractLinks(document.Html, task.Url);
                foreach (var link in links.Where(l => IsInScope(l, config)))
                {
                    queue.Enqueue(new CrawlTask { Url = link, Depth = task.Depth + 1 });
                }
                
                _logger.LogInformation($"Crawled: {task.Url} (Depth: {task.Depth}, Queue: {queue.Count})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to crawl: {task.Url}");
                job.FailedUrls.Add(task.Url);
            }
        }
        
        job.CompletedAt = DateTime.UtcNow;
        job.Status = CrawlStatus.Completed;
        job.TotalPages = visited.Count;
        
        return job;
    }
    
    private async Task<Document> FetchDocumentAsync(string url)
    {
        using var page = await _playwright.NewPageAsync();
        
        var response = await page.GotoAsync(url, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });
        
        if (!response.Ok)
        {
            throw new HttpRequestException($"HTTP {response.Status}: {url}");
        }
        
        var html = await page.ContentAsync();
        var title = await page.TitleAsync();
        
        return new Document
        {
            Url = url,
            Title = title,
            Html = html,
            FetchedAt = DateTime.UtcNow,
            StatusCode = response.Status
        };
    }
    
    private List<string> ExtractLinks(string html, string baseUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var links = doc.DocumentNode
            .SelectNodes("//a[@href]")
            ?.Select(node => node.GetAttributeValue("href", ""))
            .Select(href => new Uri(new Uri(baseUrl), href).ToString())
            .Where(url => url.StartsWith("http"))
            .Distinct()
            .ToList() ?? new List<string>();
        
        return links;
    }
    
    private bool IsInScope(string url, CrawlConfig config)
    {
        // Only crawl URLs matching allowed domains
        var uri = new Uri(url);
        return config.AllowedDomains.Any(domain => uri.Host.Contains(domain));
    }
}
```

### Robots.txt Compliance

**Implementation:**
```csharp
public class RobotsTxtParser
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, RobotsTxt> _cache;
    
    public async Task<bool> IsAllowedAsync(string url, string userAgent)
    {
        var domain = GetDomain(url);
        var robotsTxt = await GetRobotsTxtAsync(domain);
        
        if (robotsTxt == null)
            return true; // No robots.txt = allow all
        
        var rules = robotsTxt.GetRulesForUserAgent(userAgent);
        
        foreach (var rule in rules.Disallow)
        {
            if (url.Contains(rule))
                return false;
        }
        
        return true;
    }
    
    private async Task<RobotsTxt> GetRobotsTxtAsync(string domain)
    {
        if (_cache.TryGetValue(domain, out var cached))
            return cached;
        
        var robotsUrl = $"{domain}/robots.txt";
        
        try
        {
            var response = await _httpClient.GetAsync(robotsUrl);
            if (!response.IsSuccessStatusCode)
                return null;
            
            var content = await response.Content.ReadAsStringAsync();
            var robotsTxt = RobotsTxt.Parse(content);
            
            _cache[domain] = robotsTxt;
            return robotsTxt;
        }
        catch
        {
            return null;
        }
    }
}
```

### Rate Limiting

**Token Bucket Algorithm:**
```csharp
public class RateLimiter
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets;
    
    public async Task WaitAsync(string domain)
    {
        var bucket = _buckets.GetOrAdd(domain, _ => new TokenBucket(
            capacity: 10,      // 10 requests
            refillRate: 10,    // per second
            refillInterval: TimeSpan.FromSeconds(1)
        ));
        
        await bucket.ConsumeAsync(1);
    }
}

public class TokenBucket
{
    private readonly int _capacity;
    private readonly int _refillRate;
    private readonly TimeSpan _refillInterval;
    private int _tokens;
    private DateTime _lastRefill;
    private readonly SemaphoreSlim _semaphore;
    
    public TokenBucket(int capacity, int refillRate, TimeSpan refillInterval)
    {
        _capacity = capacity;
        _refillRate = refillRate;
        _refillInterval = refillInterval;
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
        _semaphore = new SemaphoreSlim(1, 1);
    }
    
    public async Task ConsumeAsync(int tokens)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            Refill();
            
            while (_tokens < tokens)
            {
                _semaphore.Release();
                await Task.Delay(100);
                await _semaphore.WaitAsync();
                Refill();
            }
            
            _tokens -= tokens;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private void Refill()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastRefill;
        
        if (elapsed >= _refillInterval)
        {
            var intervalsElapsed = (int)(elapsed.TotalMilliseconds / _refillInterval.TotalMilliseconds);
            _tokens = Math.Min(_capacity, _tokens + intervalsElapsed * _refillRate);
            _lastRefill = now;
        }
    }
}
```

### Acceptance Criteria

1. **AC-5.5.1:** System SHALL crawl Microsoft Learn documentation (docs.microsoft.com)
2. **AC-5.5.2:** System SHALL crawl Azure DevOps REST API documentation
3. **AC-5.5.3:** System SHALL respect robots.txt and rate limits (max 10 requests/second)
4. **AC-5.5.4:** System SHALL parse HTML content extracting text, code samples, and metadata
5. **AC-5.5.5:** System SHALL detect and skip duplicate content based on URL and content hash
6. **AC-5.5.6:** System SHALL schedule incremental crawls to capture new/updated content
7. **AC-5.5.7:** System SHALL store raw HTML and parsed content for future reprocessing

---

## Module 2: Document Parser

### Purpose

Extract structured data from HTML documents including text content, code samples, headings, metadata, and images.

### Parsing Strategy

**Content Extraction:**
1. **Main Content:** Identify article body using CSS selectors (`.main-content`, `article`, `#content`)
2. **Headings:** Extract H1-H6 tags for document structure
3. **Code Blocks:** Extract `<pre><code>` blocks with language detection
4. **Lists:** Extract ordered and unordered lists
5. **Tables:** Extract table data with headers
6. **Images:** Extract image URLs and alt text
7. **Metadata:** Extract title, description, keywords, author, publish date

### Implementation

```csharp
public class DocumentParser
{
    public ParsedContent Parse(Document document)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(document.Html);
        
        var parsed = new ParsedContent
        {
            Url = document.Url,
            Title = ExtractTitle(htmlDoc),
            MainContent = ExtractMainContent(htmlDoc),
            Headings = ExtractHeadings(htmlDoc),
            CodeBlocks = ExtractCodeBlocks(htmlDoc),
            Lists = ExtractLists(htmlDoc),
            Tables = ExtractTables(htmlDoc),
            Images = ExtractImages(htmlDoc, document.Url),
            Metadata = ExtractMetadata(htmlDoc),
            ParsedAt = DateTime.UtcNow
        };
        
        return parsed;
    }
    
    private string ExtractMainContent(HtmlDocument doc)
    {
        // Try common content selectors
        var selectors = new[]
        {
            ".main-content",
            "article",
            "#content",
            ".content",
            "main"
        };
        
        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//{selector}");
            if (node != null)
            {
                return CleanText(node.InnerText);
            }
        }
        
        // Fallback: extract body text
        return CleanText(doc.DocumentNode.SelectSingleNode("//body")?.InnerText ?? "");
    }
    
    private List<Heading> ExtractHeadings(HtmlDocument doc)
    {
        var headings = new List<Heading>();
        
        for (int level = 1; level <= 6; level++)
        {
            var nodes = doc.DocumentNode.SelectNodes($"//h{level}");
            if (nodes == null) continue;
            
            foreach (var node in nodes)
            {
                headings.Add(new Heading
                {
                    Level = level,
                    Text = CleanText(node.InnerText),
                    Id = node.GetAttributeValue("id", "")
                });
            }
        }
        
        return headings;
    }
    
    private List<CodeBlock> ExtractCodeBlocks(HtmlDocument doc)
    {
        var codeBlocks = new List<CodeBlock>();
        var nodes = doc.DocumentNode.SelectNodes("//pre/code | //code[@class]");
        
        if (nodes == null) return codeBlocks;
        
        foreach (var node in nodes)
        {
            var code = node.InnerText;
            var language = DetectLanguage(node);
            
            codeBlocks.Add(new CodeBlock
            {
                Code = code,
                Language = language,
                LineCount = code.Split('\n').Length
            });
        }
        
        return codeBlocks;
    }
    
    private string DetectLanguage(HtmlNode node)
    {
        // Check class attribute (e.g., "language-csharp", "lang-python")
        var classAttr = node.GetAttributeValue("class", "");
        
        var patterns = new Dictionary<string, string>
        {
            { "csharp|cs|c#", "csharp" },
            { "python|py", "python" },
            { "javascript|js", "javascript" },
            { "typescript|ts", "typescript" },
            { "rust|rs", "rust" },
            { "sql", "sql" },
            { "bash|shell|sh", "bash" },
            { "json", "json" },
            { "yaml|yml", "yaml" },
            { "xml", "xml" }
        };
        
        foreach (var (pattern, language) in patterns)
        {
            if (Regex.IsMatch(classAttr, pattern, RegexOptions.IgnoreCase))
                return language;
        }
        
        return "unknown";
    }
    
    private ContentMetadata ExtractMetadata(HtmlDocument doc)
    {
        var metadata = new ContentMetadata();
        
        // Extract meta tags
        var metaTags = doc.DocumentNode.SelectNodes("//meta");
        if (metaTags != null)
        {
            foreach (var tag in metaTags)
            {
                var name = tag.GetAttributeValue("name", "") ?? tag.GetAttributeValue("property", "");
                var content = tag.GetAttributeValue("content", "");
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
                    continue;
                
                switch (name.ToLower())
                {
                    case "description":
                    case "og:description":
                        metadata.Description = content;
                        break;
                    case "keywords":
                        metadata.Keywords = content.Split(',').Select(k => k.Trim()).ToList();
                        break;
                    case "author":
                        metadata.Author = content;
                        break;
                    case "article:published_time":
                        metadata.PublishedDate = DateTime.Parse(content);
                        break;
                    case "article:modified_time":
                        metadata.ModifiedDate = DateTime.Parse(content);
                        break;
                }
            }
        }
        
        return metadata;
    }
    
    private string CleanText(string text)
    {
        // Remove excessive whitespace
        text = Regex.Replace(text, @"\s+", " ");
        
        // Remove HTML entities
        text = WebUtility.HtmlDecode(text);
        
        return text.Trim();
    }
}
```

### Code Sample Extraction

**Purpose:** Extract high-quality code examples for training data.

**Quality Criteria:**
- Minimum 5 lines of code
- Valid syntax (validated by language parser)
- Includes comments or context
- Not generic "Hello World" examples

**Example Output:**
```json
{
  "code": "public async Task<WorkItem> CreateWorkItemAsync(string title, string description)\n{\n    var document = new JsonPatchDocument();\n    document.Add(new JsonPatchOperation\n    {\n        Operation = Operation.Add,\n        Path = \"/fields/System.Title\",\n        Value = title\n    });\n    document.Add(new JsonPatchOperation\n    {\n        Operation = Operation.Add,\n        Path = \"/fields/System.Description\",\n        Value = description\n    });\n    \n    return await _workItemClient.CreateWorkItemAsync(document, \"MyProject\", \"Task\");\n}",
  "language": "csharp",
  "context": "Creating a work item in Azure DevOps using the REST API",
  "source_url": "https://learn.microsoft.com/en-us/azure/devops/integrate/quickstarts/work-item-quickstart",
  "quality_score": 0.92
}
```

### Acceptance Criteria

8. **AC-5.5.8:** System SHALL extract main content text with 95%+ accuracy (validated on sample set)
9. **AC-5.5.9:** System SHALL extract code blocks with language detection (90%+ accuracy)
10. **AC-5.5.10:** System SHALL extract document structure (headings, lists, tables)
11. **AC-5.5.11:** System SHALL extract metadata (title, description, author, publish date)
12. **AC-5.5.12:** System SHALL validate code syntax using language-specific parsers
13. **AC-5.5.13:** System SHALL filter low-quality code samples (< 5 lines, generic examples)

---

## Module 3: Knowledge Graph Builder

### Purpose

Construct structured knowledge graph from parsed documents, identifying entities (classes, methods, APIs, concepts) and relationships (inherits, implements, uses, related_to).

### Entity Extraction

**Entity Types:**
- **Class:** C# classes, interfaces, structs
- **Method:** Functions, procedures, endpoints
- **API:** REST APIs, SDK methods
- **Concept:** Design patterns, principles, best practices
- **Tool:** Software tools, frameworks, libraries
- **Configuration:** Settings, parameters, environment variables

**Named Entity Recognition (NER):**
```csharp
public class EntityExtractor
{
    private readonly ISpacyNLP _spacy;
    private readonly ICodeParser _codeParser;
    
    public List<Entity> ExtractEntities(ParsedContent content)
    {
        var entities = new List<Entity>();
        
        // Extract entities from text using spaCy NER
        var textEntities = _spacy.ExtractEntities(content.MainContent);
        entities.AddRange(textEntities);
        
        // Extract entities from code blocks
        foreach (var codeBlock in content.CodeBlocks)
        {
            var codeEntities = _codeParser.ExtractEntities(codeBlock);
            entities.AddRange(codeEntities);
        }
        
        // Extract entities from headings (often contain key concepts)
        foreach (var heading in content.Headings)
        {
            if (IsConceptHeading(heading.Text))
            {
                entities.Add(new Entity
                {
                    Type = EntityType.Concept,
                    Name = heading.Text,
                    SourceUrl = content.Url
                });
            }
        }
        
        return entities.DistinctBy(e => e.Name).ToList();
    }
    
    private bool IsConceptHeading(string text)
    {
        var conceptPatterns = new[]
        {
            "pattern", "principle", "practice", "approach", "strategy",
            "architecture", "design", "method", "technique"
        };
        
        return conceptPatterns.Any(p => text.ToLower().Contains(p));
    }
}
```

**Code Entity Extraction:**
```csharp
public class CSharpCodeParser : ICodeParser
{
    public List<Entity> ExtractEntities(CodeBlock codeBlock)
    {
        if (codeBlock.Language != "csharp")
            return new List<Entity>();
        
        var tree = CSharpSyntaxTree.ParseText(codeBlock.Code);
        var root = tree.GetRoot();
        
        var entities = new List<Entity>();
        
        // Extract classes
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var cls in classes)
        {
            entities.Add(new Entity
            {
                Type = EntityType.Class,
                Name = cls.Identifier.Text,
                Namespace = GetNamespace(cls),
                SourceCode = cls.ToString()
            });
        }
        
        // Extract methods
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var method in methods)
        {
            entities.Add(new Entity
            {
                Type = EntityType.Method,
                Name = method.Identifier.Text,
                ParentClass = GetParentClass(method),
                Signature = GetMethodSignature(method),
                SourceCode = method.ToString()
            });
        }
        
        return entities;
    }
}
```

### Relationship Extraction

**Relationship Types:**
- **Inherits:** Class A inherits from Class B
- **Implements:** Class A implements Interface B
- **Uses:** Method A calls Method B
- **Related_To:** Concept A is related to Concept B
- **Part_Of:** Method A is part of Class B
- **Documented_In:** Entity A is documented in URL B

**Implementation:**
```csharp
public class RelationshipExtractor
{
    public List<Relationship> ExtractRelationships(List<Entity> entities, ParsedContent content)
    {
        var relationships = new List<Relationship>();
        
        // Extract inheritance relationships from code
        foreach (var entity in entities.Where(e => e.Type == EntityType.Class))
        {
            if (!string.IsNullOrEmpty(entity.BaseClass))
            {
                relationships.Add(new Relationship
                {
                    Type = RelationshipType.Inherits,
                    SourceEntity = entity.Name,
                    TargetEntity = entity.BaseClass
                });
            }
            
            foreach (var iface in entity.Interfaces ?? new List<string>())
            {
                relationships.Add(new Relationship
                {
                    Type = RelationshipType.Implements,
                    SourceEntity = entity.Name,
                    TargetEntity = iface
                });
            }
        }
        
        // Extract method calls (uses relationships)
        foreach (var entity in entities.Where(e => e.Type == EntityType.Method))
        {
            var calledMethods = ExtractMethodCalls(entity.SourceCode);
            foreach (var called in calledMethods)
            {
                relationships.Add(new Relationship
                {
                    Type = RelationshipType.Uses,
                    SourceEntity = entity.Name,
                    TargetEntity = called
                });
            }
        }
        
        // Extract conceptual relationships from text
        var conceptRelationships = ExtractConceptualRelationships(content.MainContent, entities);
        relationships.AddRange(conceptRelationships);
        
        return relationships;
    }
    
    private List<Relationship> ExtractConceptualRelationships(string text, List<Entity> entities)
    {
        var relationships = new List<Relationship>();
        
        // Look for patterns like "A is a type of B", "A uses B", "A is related to B"
        var patterns = new[]
        {
            @"(\w+)\s+is\s+a\s+type\s+of\s+(\w+)",
            @"(\w+)\s+uses\s+(\w+)",
            @"(\w+)\s+is\s+related\s+to\s+(\w+)",
            @"(\w+)\s+implements\s+(\w+)",
            @"(\w+)\s+inherits\s+from\s+(\w+)"
        };
        
        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var source = match.Groups[1].Value;
                var target = match.Groups[2].Value;
                
                // Verify entities exist
                if (entities.Any(e => e.Name.Equals(source, StringComparison.OrdinalIgnoreCase)) &&
                    entities.Any(e => e.Name.Equals(target, StringComparison.OrdinalIgnoreCase)))
                {
                    relationships.Add(new Relationship
                    {
                        Type = RelationshipType.Related_To,
                        SourceEntity = source,
                        TargetEntity = target
                    });
                }
            }
        }
        
        return relationships;
    }
}
```

### Knowledge Graph Storage

**Neo4j Cypher Queries:**
```cypher
// Create entity node
CREATE (e:Entity {
  id: $id,
  type: $type,
  name: $name,
  namespace: $namespace,
  sourceUrl: $sourceUrl,
  createdAt: datetime()
})

// Create relationship
MATCH (source:Entity {name: $sourceName})
MATCH (target:Entity {name: $targetName})
CREATE (source)-[r:RELATIONSHIP {type: $relType}]->(target)

// Query related entities
MATCH (e:Entity {name: 'WorkItemCoordinator'})-[r]-(related)
RETURN e, r, related

// Find inheritance hierarchy
MATCH path = (child:Entity)-[:INHERITS*]->(parent:Entity)
WHERE child.name = 'MyClass'
RETURN path
```

### Acceptance Criteria

14. **AC-5.5.14:** System SHALL extract 50,000+ entities from ingested documentation
15. **AC-5.5.15:** System SHALL identify 200,000+ relationships between entities
16. **AC-5.5.16:** System SHALL build Neo4j knowledge graph with entity and relationship nodes
17. **AC-5.5.17:** System SHALL support semantic search across knowledge graph
18. **AC-5.5.18:** System SHALL link entities to source documents for traceability
19. **AC-5.5.19:** System SHALL update graph incrementally as new content is ingested

---

## Module 4: Synthetic Question Generator

### Purpose

Generate high-quality multiple-choice questions for AI Arena using GPT-4 based on ingested content.

### Generation Pipeline

**Step 1: Topic Selection**
- Identify high-value topics from knowledge graph (frequently referenced entities)
- Balance question distribution across categories
- Prioritize recent content for up-to-date questions

**Step 2: Question Generation**
```csharp
public class SyntheticQuestionGenerator
{
    private readonly IOpenAIClient _openai;
    private readonly IQuestionValidator _validator;
    
    public async Task<List<Question>> GenerateQuestionsAsync(string topic, int count, int difficultyLevel)
    {
        var prompt = BuildPrompt(topic, difficultyLevel);
        
        var response = await _openai.ChatCompletionAsync(new ChatRequest
        {
            Model = "gpt-4",
            Messages = new[]
            {
                new ChatMessage { Role = "system", Content = SYSTEM_PROMPT },
                new ChatMessage { Role = "user", Content = prompt }
            },
            Temperature = 0.7,
            MaxTokens = 2000
        });
        
        var questions = ParseQuestions(response.Choices[0].Message.Content);
        
        // Validate questions
        var validQuestions = new List<Question>();
        foreach (var question in questions)
        {
            var validation = await _validator.ValidateAsync(question);
            if (validation.IsValid)
            {
                validQuestions.Add(question);
            }
            else
            {
                _logger.LogWarning($"Invalid question: {validation.Errors}");
            }
        }
        
        return validQuestions.Take(count).ToList();
    }
    
    private string BuildPrompt(string topic, int difficultyLevel)
    {
        return $@"
Generate {count} multiple-choice questions about {topic} for difficulty level {difficultyLevel} (1=Easy, 15=Expert).

Requirements:
- One correct answer and three plausible distractors
- Question should test understanding, not just memorization
- Include brief explanation of correct answer with source reference
- Format as JSON array with structure:
  {{
    ""text"": ""Question text"",
    ""options"": [
      {{""id"": ""A"", ""text"": ""Option A"", ""isCorrect"": false}},
      {{""id"": ""B"", ""text"": ""Option B"", ""isCorrect"": true}},
      {{""id"": ""C"", ""text"": ""Option C"", ""isCorrect"": false}},
      {{""id"": ""D"", ""text"": ""Option D"", ""isCorrect"": false}}
    ],
    ""explanation"": ""Why B is correct..."",
    ""source"": ""https://learn.microsoft.com/...""
  }}

Topic Context:
{GetTopicContext(topic)}
";
    }
}
```

**Step 3: Quality Validation**
```csharp
public class QuestionValidator
{
    public ValidationResult Validate(Question question)
    {
        var errors = new List<string>();
        
        // Check exactly one correct answer
        var correctCount = question.Options.Count(o => o.IsCorrect);
        if (correctCount != 1)
        {
            errors.Add($"Must have exactly 1 correct answer, found {correctCount}");
        }
        
        // Check all options are distinct
        var distinctOptions = question.Options.Select(o => o.Text).Distinct().Count();
        if (distinctOptions != question.Options.Count)
        {
            errors.Add("Options must be distinct");
        }
        
        // Check question clarity (no ambiguous words)
        var ambiguousWords = new[] { "sometimes", "maybe", "possibly", "might" };
        if (ambiguousWords.Any(w => question.Text.ToLower().Contains(w)))
        {
            errors.Add("Question contains ambiguous language");
        }
        
        // Check explanation exists and references source
        if (string.IsNullOrEmpty(question.Explanation))
        {
            errors.Add("Explanation is required");
        }
        
        if (string.IsNullOrEmpty(question.Source) || !Uri.IsWellFormedUriString(question.Source, UriKind.Absolute))
        {
            errors.Add("Valid source URL is required");
        }
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
```

**Step 4: Difficulty Calibration**
- Initial difficulty assigned by GPT-4
- Adjusted based on historical model performance in AI Arena
- Questions with > 90% accuracy moved to easier levels
- Questions with < 30% accuracy flagged for review

### Acceptance Criteria

20. **AC-5.5.20:** System SHALL generate 10,000+ questions across 15 difficulty levels
21. **AC-5.5.21:** System SHALL validate question quality (single correct answer, distinct options, clear wording)
22. **AC-5.5.22:** System SHALL include source references for all questions
23. **AC-5.5.23:** System SHALL balance question distribution across categories
24. **AC-5.5.24:** System SHALL adjust difficulty based on historical model performance

---

## Module 5: Incremental Updater

### Purpose

Detect new and updated content on crawled sites, minimizing redundant processing.

### Change Detection

**Content Hashing:**
```csharp
public class ContentHasher
{
    public string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
```

**Incremental Crawl Strategy:**
```csharp
public class IncrementalUpdater
{
    public async Task<UpdateResult> CheckForUpdatesAsync()
    {
        var result = new UpdateResult();
        
        // Get all previously crawled URLs
        var previousCrawl = await _storage.GetLastCrawlAsync();
        
        foreach (var url in previousCrawl.Urls)
        {
            var currentHash = await FetchAndHashAsync(url);
            var previousHash = previousCrawl.GetHash(url);
            
            if (currentHash != previousHash)
            {
                result.UpdatedUrls.Add(url);
                await _crawler.RecrawlAsync(url);
            }
        }
        
        // Check for new pages (via sitemap or seed URL crawl)
        var newUrls = await DiscoverNewUrlsAsync();
        result.NewUrls.AddRange(newUrls);
        
        foreach (var url in newUrls)
        {
            await _crawler.CrawlAsync(url);
        }
        
        return result;
    }
    
    private async Task<List<string>> DiscoverNewUrlsAsync()
    {
        var newUrls = new List<string>();
        
        // Check sitemaps
        var sitemaps = new[]
        {
            "https://learn.microsoft.com/sitemap.xml",
            "https://playwright.dev/sitemap.xml"
        };
        
        foreach (var sitemapUrl in sitemaps)
        {
            var urls = await _sitemapParser.ParseAsync(sitemapUrl);
            var existingUrls = await _storage.GetCrawledUrlsAsync();
            
            newUrls.AddRange(urls.Except(existingUrls));
        }
        
        return newUrls;
    }
}
```

### Scheduling

**Cron Schedule:**
- **Daily Incremental Crawl:** 02:00 UTC (check for updates)
- **Weekly Full Crawl:** Sunday 00:00 UTC (re-crawl all URLs)
- **Monthly Deep Crawl:** 1st of month 00:00 UTC (discover new seed URLs)

### Acceptance Criteria

25. **AC-5.5.25:** System SHALL detect content changes using SHA-256 hashing
26. **AC-5.5.26:** System SHALL perform daily incremental crawls
27. **AC-5.5.27:** System SHALL perform weekly full re-crawls
28. **AC-5.5.28:** System SHALL discover new URLs via sitemaps
29. **AC-5.5.29:** System SHALL skip unchanged content to reduce processing time

---

## Storage Architecture

### PostgreSQL Schema

```sql
-- Documents table (raw HTML)
CREATE TABLE documents (
    id UUID PRIMARY KEY,
    url TEXT UNIQUE NOT NULL,
    title TEXT,
    html TEXT,
    content_hash TEXT,
    fetched_at TIMESTAMP,
    status_code INT
);

-- Parsed content table
CREATE TABLE parsed_content (
    id UUID PRIMARY KEY,
    document_id UUID REFERENCES documents(id),
    main_content TEXT,
    parsed_at TIMESTAMP
);

-- Headings table
CREATE TABLE headings (
    id UUID PRIMARY KEY,
    parsed_content_id UUID REFERENCES parsed_content(id),
    level INT,
    text TEXT,
    heading_id TEXT
);

-- Code blocks table
CREATE TABLE code_blocks (
    id UUID PRIMARY KEY,
    parsed_content_id UUID REFERENCES parsed_content(id),
    code TEXT,
    language TEXT,
    line_count INT
);

-- Questions table (AI Arena)
CREATE TABLE questions (
    id UUID PRIMARY KEY,
    level INT,
    difficulty TEXT,
    category TEXT,
    text TEXT,
    options JSONB,
    explanation TEXT,
    source TEXT,
    created_at TIMESTAMP,
    difficulty_score FLOAT,
    avg_response_time FLOAT
);

-- Crawl jobs table
CREATE TABLE crawl_jobs (
    id UUID PRIMARY KEY,
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    status TEXT,
    total_pages INT,
    failed_urls TEXT[]
);
```

### S3-Compatible Storage

**Use Cases:**
- Store raw HTML files (large documents)
- Store extracted images
- Store knowledge graph exports
- Store training datasets

**Bucket Structure:**
```
/raw-html/{domain}/{year}/{month}/{hash}.html
/images/{domain}/{hash}.{ext}
/knowledge-graph/{version}/graph-export.json
/training-data/{dataset-id}/examples.jsonl
```

### Acceptance Criteria

30. **AC-5.5.30:** System SHALL store raw HTML in S3-compatible storage
31. **AC-5.5.31:** System SHALL store parsed content in PostgreSQL
32. **AC-5.5.32:** System SHALL support on-premise MinIO deployment
33. **AC-5.5.33:** System SHALL compress HTML files (gzip) to reduce storage costs
34. **AC-5.5.34:** System SHALL implement retention policy (delete raw HTML after 90 days)

---

## Performance Optimization

### Parallel Processing

**Crawl Parallelization:**
```csharp
public async Task CrawlParallelAsync(List<string> urls, int maxParallelism = 10)
{
    var semaphore = new SemaphoreSlim(maxParallelism);
    var tasks = urls.Select(async url =>
    {
        await semaphore.WaitAsync();
        try
        {
            await CrawlAsync(url);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
}
```

**Parsing Parallelization:**
```csharp
public async Task ParseBatchAsync(List<Document> documents)
{
    var parsedContents = await Task.WhenAll(
        documents.Select(doc => Task.Run(() => _parser.Parse(doc)))
    );
    
    await _storage.SaveBatchAsync(parsedContents);
}
```

### Caching

**Redis Cache:**
- Cache robots.txt (TTL: 24 hours)
- Cache parsed content (TTL: 7 days)
- Cache knowledge graph queries (TTL: 1 hour)

### Acceptance Criteria

35. **AC-5.5.35:** System SHALL crawl 10,000 pages in < 2 hours (10 pages/second)
36. **AC-5.5.36:** System SHALL parse 1,000 documents in < 10 minutes
37. **AC-5.5.37:** System SHALL build knowledge graph from 100,000 entities in < 1 hour
38. **AC-5.5.38:** System SHALL cache frequently accessed data in Redis

---

## Monitoring & Observability

### Metrics

**Crawl Metrics:**
- Pages crawled per hour
- Success rate (HTTP 200 responses)
- Average response time
- Rate limit violations

**Parsing Metrics:**
- Documents parsed per hour
- Parsing errors (malformed HTML)
- Code blocks extracted
- Entities extracted

**Knowledge Graph Metrics:**
- Total entities
- Total relationships
- Graph density (relationships / entities)
- Query latency

### Logging

**Structured Logging:**
```csharp
_logger.LogInformation(
    "Crawled {Url} in {Duration}ms with status {StatusCode}",
    url,
    duration,
    statusCode
);
```

**Log Aggregation:**
- Centralized logging to Elasticsearch
- Kibana dashboards for visualization
- Alerts on error rate > 5%

### Acceptance Criteria

39. **AC-5.5.39:** System SHALL expose Prometheus metrics for monitoring
40. **AC-5.5.40:** System SHALL log all crawl operations with structured logging
41. **AC-5.5.41:** System SHALL alert on crawl failures > 5%
42. **AC-5.5.42:** System SHALL provide Grafana dashboards for real-time monitoring

---

## Security & Compliance

### Authentication

**Credential Storage:**
- Azure Key Vault for API keys
- Environment variables for local development
- Kubernetes secrets for production

### Data Privacy

**GDPR Compliance:**
- No personal data collected from crawled sites
- User-generated content (comments) excluded from crawl
- Right to deletion: remove specific URLs from storage

### Acceptance Criteria

43. **AC-5.5.43:** System SHALL store API credentials in Azure Key Vault
44. **AC-5.5.44:** System SHALL exclude user-generated content from crawl
45. **AC-5.5.45:** System SHALL support URL deletion for GDPR compliance
46. **AC-5.5.46:** System SHALL encrypt all stored data at rest (AES-256)

---

## Deployment

### On-Premise Deployment

**Infrastructure Requirements:**
- **Compute:** 16 CPU cores, 64GB RAM
- **Storage:** 2TB SSD (PostgreSQL), 10TB HDD (S3/MinIO)
- **Network:** 1Gbps internet connection

**Software Stack:**
- **Crawler:** Docker container with Playwright
- **Database:** PostgreSQL 15+
- **Object Storage:** MinIO (S3-compatible)
- **Knowledge Graph:** Neo4j 5+
- **Cache:** Redis 7+
- **Orchestration:** Kubernetes or Docker Compose

### Acceptance Criteria

47. **AC-5.5.47:** System SHALL support on-premise deployment with Docker Compose
48. **AC-5.5.48:** System SHALL support Kubernetes deployment with Helm charts
49. **AC-5.5.49:** System SHALL provide installation documentation
50. **AC-5.5.50:** System SHALL complete initial crawl (100,000 pages) in < 24 hours

---

## Success Metrics

### Coverage Metrics

- **Documentation Coverage:** 100% of Microsoft Learn Azure DevOps docs
- **Code Sample Coverage:** 10,000+ code examples extracted
- **Entity Coverage:** 50,000+ entities in knowledge graph
- **Relationship Coverage:** 200,000+ relationships

### Quality Metrics

- **Parsing Accuracy:** 95%+ correct extraction of main content
- **Entity Extraction Accuracy:** 90%+ correct entity identification
- **Question Quality:** 95%+ questions pass validation
- **Crawl Success Rate:** 98%+ pages successfully crawled

### Performance Metrics

- **Crawl Speed:** 10 pages/second sustained
- **Parsing Speed:** 100 documents/minute
- **Knowledge Graph Build Time:** < 1 hour for 100,000 entities
- **Incremental Update Time:** < 30 minutes daily

---

## References

1. **Web Scraping Best Practices:** https://www.scrapinghub.com/guides/web-scraping-best-practices/
2. **Robots.txt Specification:** https://www.robotstxt.org/robotstxt.html
3. **Playwright Documentation:** https://playwright.dev/docs/intro
4. **Neo4j Knowledge Graphs:** https://neo4j.com/developer/graph-database/
5. **spaCy NER:** https://spacy.io/usage/linguistic-features#named-entities
6. **Microsoft Learn:** https://learn.microsoft.com/en-us/azure/devops/
7. **GPT-4 API:** https://platform.openai.com/docs/guides/gpt

---

**End of Content Ingestion Pipeline Specification**
