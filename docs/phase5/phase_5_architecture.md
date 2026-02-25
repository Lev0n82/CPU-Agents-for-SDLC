# Phase 5: AI Model Management & Training Arena - Architecture Specification v2.0

**Version:** 2.0 (Feedback Incorporated)  
**Status:** Architecture Design - Production Ready  
**Author:** Manus AI  
**Date:** February 24, 2026

---

## Executive Summary

Phase 5 introduces a comprehensive **AI Model Management & Training Arena** system with integrated security, scalable architecture, and unified validation. This version addresses 10 critical feedback points including security-first design, API gateway abstraction, real-time quality assurance, and cross-module dependency management.

**Key Improvements in v2.0:**
- **Security Integration**: Embedded validation throughout deployment and training pipelines
- **API Gateway Pattern**: Unified interface abstracting Azure DevOps, GitHub, Jira, Bing APIs
- **Unified Validation Service**: Consolidated validation logic across all modules
- **Scalable ELO System**: Incremental calculation with leaderboard sharding
- **Real-Time Quality Gates**: Proactive synthetic data validation with automatic rejection
- **Dynamic Rate Limiting**: Adaptive content ingestion based on target site responses
- **Training Monitoring**: Resource alerts, divergence detection, automated rollback
- **Adversarial Training**: Edge case handling for API failures and network timeouts
- **Dependency Management**: Clear interface contracts and staged deployment plan

**System Scale:**
- **21 classes** (was 18): Added APIGatewayService, ValidationService, QualityGateService
- **145 acceptance criteria** (was 124): +21 new criteria
- **24-week implementation** (was 20): +4 weeks for enhanced architecture
- **$300K investment** (was $250K): +$50K for security and scalability improvements

---

## Architecture Overview

### System Components

Phase 5 consists of **10 major modules** with **21 classes** and **145 acceptance criteria**:

| **Module** | **Classes** | **Purpose** | **New in v2.0** |
|------------|-------------|-------------|-----------------|
| Model Management Console | 3 | Registry, versioning, deployment | Security-integrated deployment |
| AI Arena | 4 | Competitive evaluation, leaderboards | Incremental ELO, sharded leaderboards |
| Synthetic Data Generation | 3 | Large-to-small model distillation | Real-time quality gates |
| Resource-Efficient Training | 6 | Lelapa AI methodology | Monitoring, divergence detection |
| Content Ingestion | 2 | Microsoft Learn crawler | Dynamic rate limiting |
| Tool Use Training | 2 | API integration training | Adversarial edge cases |
| **API Gateway** | **1** | **Unified API abstraction** | **NEW** |
| **Validation Service** | **1** | **Consolidated validation logic** | **NEW** |
| **Quality Assurance** | **1** | **Real-time quality scoring** | **NEW** |
| Evaluation Framework | 1 | Performance benchmarking | Enhanced metrics |

---

## Module 0: Cross-Cutting Services (NEW)

### Purpose

Foundational services used across all Phase 5 modules to ensure security, consistency, and scalability.

### Components

#### 0.1 APIGatewayService (NEW)

**Responsibility:** Unified interface abstracting provider-specific API implementations (Azure DevOps, GitHub, Jira, Bing Search).

**Rationale:** Addresses feedback point #5 - reduces complexity of managing multiple API credentials, rate limits, and sandboxing.

**Key Methods:**
```csharp
public interface IAPIGatewayService
{
    Task<WorkItem> GetWorkItemAsync(string provider, string workItemId);
    Task<WorkItem> CreateWorkItemAsync(string provider, WorkItemRequest request);
    Task<SearchResult> SearchAsync(string provider, string query, SearchOptions options);
    Task<RateLimitStatus> GetRateLimitStatusAsync(string provider);
    Task<bool> ValidateCredentialsAsync(string provider);
}

public class APIGatewayService : IAPIGatewayService
{
    private readonly Dictionary<string, IAPIProvider> _providers;
    private readonly IRateLimiter _rateLimiter;
    private readonly ISecretsProvider _secretsProvider;
    
    Task<WorkItem> GetWorkItemAsync(string provider, string workItemId);
    Task<WorkItem> CreateWorkItemAsync(string provider, WorkItemRequest request);
    Task<SearchResult> SearchAsync(string provider, string query, SearchOptions options);
    Task<RateLimitStatus> GetRateLimitStatusAsync(string provider);
    Task<bool> ValidateCredentialsAsync(string provider);
}

// Provider-specific adapters
public interface IAPIProvider
{
    string ProviderName { get; }
    Task<T> ExecuteAsync<T>(APIRequest request);
    Task<RateLimitStatus> GetRateLimitStatusAsync();
}

public class AzureDevOpsProvider : IAPIProvider { }
public class GitHubProvider : IAPIProvider { }
public class JiraProvider : IAPIProvider { }
public class BingSearchProvider : IAPIProvider { }
```

**Acceptance Criteria:**
1. AC-5.0.1: System SHALL provide unified interface for Azure DevOps, GitHub, Jira, and Bing Search APIs
2. AC-5.0.2: System SHALL automatically select appropriate provider based on configuration
3. AC-5.0.3: System SHALL manage API credentials via Azure Key Vault integration
4. AC-5.0.4: System SHALL enforce provider-specific rate limits with automatic backoff
5. AC-5.0.5: System SHALL retry failed API calls with exponential backoff (3 attempts, 1s/2s/4s delays)
6. AC-5.0.6: System SHALL log all API calls with provider, endpoint, latency, and status code
7. AC-5.0.7: System SHALL provide circuit breaker pattern for failing providers (5 failures → 30s cooldown)
8. AC-5.0.8: System SHALL support API sandboxing for test environments

#### 0.2 ValidationService (NEW)

**Responsibility:** Consolidated validation logic reused across model deployment, evaluation, and training workflows.

**Rationale:** Addresses feedback point #7 - eliminates duplicate validation code between ModelDeploymentOrchestrator and ModelEvaluator.

**Key Methods:**
```csharp
public interface IValidationService
{
    Task<ValidationResult> ValidateModelHealthAsync(Guid modelId, DeploymentTarget target);
    Task<ValidationResult> ValidateInferenceLatencyAsync(Guid modelId, int sampleSize);
    Task<ValidationResult> ValidateModelAccuracyAsync(Guid modelId, TestDataset dataset);
    Task<ValidationResult> ValidateResourceUsageAsync(Guid modelId, ResourceLimits limits);
    Task<ValidationResult> ValidateSecurityAsync(Guid modelId, SecurityChecks checks);
}

public class ValidationService : IValidationService
{
    private readonly IModelRegistryService _registry;
    private readonly ISecurityScanner _securityScanner;
    
    Task<ValidationResult> ValidateModelHealthAsync(Guid modelId, DeploymentTarget target);
    Task<ValidationResult> ValidateInferenceLatencyAsync(Guid modelId, int sampleSize);
    Task<ValidationResult> ValidateModelAccuracyAsync(Guid modelId, TestDataset dataset);
    Task<ValidationResult> ValidateResourceUsageAsync(Guid modelId, ResourceLimits limits);
    Task<ValidationResult> ValidateSecurityAsync(Guid modelId, SecurityChecks checks);
}
```

**Acceptance Criteria:**
9. AC-5.0.9: System SHALL provide unified validation interface for deployment and evaluation workflows
10. AC-5.0.10: System SHALL validate model health with 5 checks: endpoint reachability, inference test, memory usage, CPU usage, error rate
11. AC-5.0.11: System SHALL validate inference latency with 95th percentile < 2s requirement
12. AC-5.0.12: System SHALL validate model accuracy against baseline (minimum 85% of baseline performance)
13. AC-5.0.13: System SHALL validate resource usage within limits (CPU < 80%, RAM < 16GB)
14. AC-5.0.14: System SHALL perform security validation: SQL injection testing, prompt injection detection, output sanitization
15. AC-5.0.15: System SHALL return detailed validation reports with pass/fail status and remediation suggestions

#### 0.3 QualityGateService (NEW)

**Responsibility:** Real-time quality scoring for synthetic data generation with automatic rejection thresholds.

**Rationale:** Addresses feedback point #3 - proactive quality validation prevents poor training data from entering the pipeline.

**Key Methods:**
```csharp
public interface IQualityGateService
{
    Task<QualityScore> ScoreDataQualityAsync(SyntheticDataSample sample);
    Task<bool> MeetsQualityThresholdAsync(QualityScore score, QualityThreshold threshold);
    Task<QualityReport> GenerateQualityReportAsync(List<SyntheticDataSample> samples);
    Task<TeacherModelComparison> CompareTeacherModelsAsync(string modelA, string modelB, int sampleSize);
}

public class QualityGateService : IQualityGateService
{
    private readonly IModelRegistryService _registry;
    private readonly IValidationService _validation;
    
    Task<QualityScore> ScoreDataQualityAsync(SyntheticDataSample sample);
    Task<bool> MeetsQualityThresholdAsync(QualityScore score, QualityThreshold threshold);
    Task<QualityReport> GenerateQualityReportAsync(List<SyntheticDataSample> samples);
    Task<TeacherModelComparison> CompareTeacherModelsAsync(string modelA, string modelB, int sampleSize);
}

public class QualityScore
{
    public double Coherence { get; set; }        // 0-1: Logical consistency
    public double Relevance { get; set; }        // 0-1: Topic alignment
    public double Correctness { get; set; }      // 0-1: Factual accuracy
    public double Diversity { get; set; }        // 0-1: Uniqueness vs existing data
    public double OverallScore => (Coherence + Relevance + Correctness + Diversity) / 4.0;
}
```

**Acceptance Criteria:**
16. AC-5.0.16: System SHALL score synthetic data quality in real-time during generation
17. AC-5.0.17: System SHALL automatically reject samples with overall quality score < 0.7
18. AC-5.0.18: System SHALL evaluate 4 quality dimensions: coherence, relevance, correctness, diversity
19. AC-5.0.19: System SHALL support A/B testing of teacher models with statistical significance testing
20. AC-5.0.20: System SHALL generate quality reports showing distribution of scores and rejection rate
21. AC-5.0.21: System SHALL alert when rejection rate exceeds 30% (indicates teacher model degradation)

---

## Module 1: Model Management Console

### Purpose

Centralized administration interface with **integrated security validation** throughout the deployment pipeline.

### Components

#### 1.1 ModelRegistryService

**Responsibility:** Maintain registry of all AI models with metadata, versions, and deployment status.

**Key Methods:**
```csharp
public class ModelRegistryService
{
    Task<ModelMetadata> RegisterModelAsync(string modelName, string version, ModelType type);
    Task<List<ModelMetadata>> GetAllModelsAsync(ModelFilter filter);
    Task<ModelMetadata> GetModelByIdAsync(Guid modelId);
    Task UpdateModelStatusAsync(Guid modelId, ModelStatus status);
    Task<List<ModelVersion>> GetModelVersionsAsync(Guid modelId);
    Task PromoteModelToProductionAsync(Guid modelId, string version);
    Task ArchiveModelAsync(Guid modelId);
}
```

**Acceptance Criteria:**
22. AC-5.1.1: System SHALL register new models with name, version, type (local/cloud), framework (vLLM/Ollama), and deployment status
23. AC-5.1.2: System SHALL support model filtering by type, status, performance metrics, and deployment environment
24. AC-5.1.3: System SHALL track model lineage showing parent models used for synthetic data generation
25. AC-5.1.4: System SHALL maintain version history with rollback capability to previous versions
26. AC-5.1.5: System SHALL enforce unique model names within organization namespace
27. AC-5.1.6: System SHALL support model tagging (production, staging, experimental, archived)
28. AC-5.1.7: System SHALL log all model registry changes with timestamp, user, and reason
29. AC-5.1.8: System SHALL validate model compatibility with target deployment environment (CPU/GPU, RAM requirements)

#### 1.2 ModelDeploymentOrchestrator (UPDATED)

**Responsibility:** Automate model deployment with **integrated security validation** via ValidationService.

**Changes in v2.0:**
- Integrated ValidationService for consolidated validation logic
- Added security checks before deployment
- Removed duplicate validation code

**Key Methods:**
```csharp
public class ModelDeploymentOrchestrator
{
    private readonly IValidationService _validation;  // NEW: Injected dependency
    private readonly IModelRegistryService _registry;
    
    Task<DeploymentResult> DeployModelAsync(Guid modelId, DeploymentTarget target);
    Task<HealthCheckResult> ValidateDeploymentAsync(Guid deploymentId);
    Task RollbackDeploymentAsync(Guid deploymentId);
    Task<List<DeploymentMetrics>> GetDeploymentMetricsAsync(Guid modelId);
    Task ScaleDeploymentAsync(Guid deploymentId, int replicas);
}
```

**Acceptance Criteria:**
30. AC-5.1.9: System SHALL deploy models to vLLM or Ollama endpoints with automatic health checks via ValidationService
31. AC-5.1.10: System SHALL validate model inference latency (< 2s for 95th percentile) before marking deployment successful
32. AC-5.1.11: System SHALL **perform security validation** (SQL injection, prompt injection, output sanitization) before deployment
33. AC-5.1.12: System SHALL automatically rollback failed deployments to previous stable version
34. AC-5.1.13: System SHALL support blue-green deployment strategy for zero-downtime updates
35. AC-5.1.14: System SHALL monitor deployment health every 60 seconds and alert on degradation
36. AC-5.1.15: System SHALL support horizontal scaling (1-10 replicas) based on load metrics

#### 1.3 ABTestingManager

**Responsibility:** Coordinate A/B testing of model variants with statistical analysis.

**Key Methods:**
```csharp
public class ABTestingManager
{
    Task<ABTest> CreateTestAsync(Guid modelAId, Guid modelBId, TestConfiguration config);
    Task<ABTestResult> GetTestResultsAsync(Guid testId);
    Task<bool> IsStatisticallySignificantAsync(Guid testId, double confidenceLevel);
    Task PromoteWinnerAsync(Guid testId);
}
```

**Acceptance Criteria:**
37. AC-5.1.16: System SHALL support A/B testing with traffic split (50/50, 70/30, 90/10)
38. AC-5.1.17: System SHALL track metrics: accuracy, latency, user satisfaction, error rate
39. AC-5.1.18: System SHALL calculate statistical significance with 95% confidence level
40. AC-5.1.19: System SHALL automatically promote winner when significance threshold met
41. AC-5.1.20: System SHALL support multi-armed bandit algorithm for dynamic traffic allocation

---

## Module 2: AI Arena - Competitive Evaluation

### Purpose

Gamified competitive evaluation with **incremental ELO calculation** and **leaderboard sharding** for scalability.

### Components

#### 2.1 ArenaGameEngine

**Responsibility:** Orchestrate "Who Wants to Be a Millionaire" game format with 15-question progressive difficulty.

**Key Methods:**
```csharp
public class ArenaGameEngine
{
    Task<GameSession> StartGameAsync(Guid modelId, GameDifficulty difficulty);
    Task<QuestionResult> AskQuestionAsync(Guid sessionId, Question question);
    Task<LifelineResult> UseLifelineAsync(Guid sessionId, LifelineType type);
    Task<GameResult> EndGameAsync(Guid sessionId);
}
```

**Acceptance Criteria:**
42. AC-5.2.1: System SHALL present 15 questions with progressive difficulty (Easy → Medium → Hard → Expert)
43. AC-5.2.2: System SHALL support 3 lifelines: 50:50, Ask Audience (Bing Search), Phone a Friend (larger model)
44. AC-5.2.3: System SHALL implement first-come-first-serve response racing with +50 speed bonus
45. AC-5.2.4: System SHALL award points: Easy (100), Medium (200), Hard (500), Expert (1000)
46. AC-5.2.5: System SHALL enforce 30-second timeout per question
47. AC-5.2.6: System SHALL log all game sessions with questions, answers, lifelines, and final score

#### 2.2 ArenaLeaderboardManager (UPDATED)

**Responsibility:** Maintain global leaderboards with **incremental ELO calculation** and **leaderboard sharding**.

**Changes in v2.0:**
- Implemented incremental ELO update algorithm (no full recalculation)
- Added leaderboard sharding by model category (code review, test generation, etc.)
- Implemented caching for frequently accessed rankings

**Rationale:** Addresses feedback point #2 - prevents computational bottlenecks with 20+ models and frequent tournaments.

**Key Methods:**
```csharp
public class ArenaLeaderboardManager
{
    // NEW: Incremental ELO calculation
    Task UpdateELORatingAsync(Guid modelId, GameResult result);
    
    // NEW: Sharded leaderboards
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(ModelCategory category, int top);
    
    Task<int> GetModelRankAsync(Guid modelId, ModelCategory category);
    Task<List<TournamentResult>> GetTournamentHistoryAsync(Guid modelId);
    
    // NEW: Caching
    Task InvalidateCacheAsync(ModelCategory category);
}
```

**Acceptance Criteria:**
48. AC-5.2.7: System SHALL calculate ELO ratings **incrementally** after each game (no full recalculation)
49. AC-5.2.8: System SHALL **shard leaderboards by model category**: code review, test generation, requirement analysis, defect detection
50. AC-5.2.9: System SHALL **cache top 100 rankings** per category with 5-minute TTL
51. AC-5.2.10: System SHALL display leaderboard with rank, model name, ELO rating, games played, win rate
52. AC-5.2.11: System SHALL support filtering by time period (daily, weekly, monthly, all-time)
53. AC-5.2.12: System SHALL highlight rank changes (up/down arrows) since previous period
54. AC-5.2.13: System SHALL support leaderboard pagination (50 entries per page)

#### 2.3 QuestionBankManager (UPDATED)

**Responsibility:** Manage 10,000+ questions with **dynamic generation** for underrepresented categories and **ML-based difficulty calibration**.

**Changes in v2.0:**
- Added dynamic question generation prioritizing sparse categories
- Implemented ML-based difficulty calibration using actual model performance data
- Added question distribution monitoring

**Rationale:** Addresses feedback point #4 - prevents sparse distributions in niche categories and ensures accurate difficulty levels.

**Key Methods:**
```csharp
public class QuestionBankManager
{
    Task<Question> GetRandomQuestionAsync(QuestionDifficulty difficulty, QuestionCategory category);
    Task<int> GetQuestionCountAsync(QuestionDifficulty difficulty, QuestionCategory category);
    
    // NEW: Dynamic generation
    Task<Question> GenerateQuestionAsync(QuestionCategory category, QuestionDifficulty difficulty);
    Task<List<QuestionCategory>> GetUnderrepresentedCategoriesAsync(int threshold);
    
    // NEW: ML-based calibration
    Task RecalibrateDifficultyAsync(Guid questionId, List<ModelPerformance> performances);
    Task<QuestionDistributionReport> GetDistributionReportAsync();
}
```

**Acceptance Criteria:**
54. AC-5.2.14: System SHALL maintain 10,000+ questions across 15 difficulty levels and 8 categories
55. AC-5.2.15: System SHALL **dynamically generate questions** for categories with < 500 questions
56. AC-5.2.16: System SHALL **recalibrate difficulty** based on actual model performance (if 80%+ models answer correctly, reduce difficulty level)
57. AC-5.2.17: System SHALL ensure balanced distribution (±10%) across all categories
58. AC-5.2.18: System SHALL support question versioning with deprecation of outdated questions
59. AC-5.2.19: System SHALL validate question quality: clear wording, single correct answer, plausible distractors
60. AC-5.2.20: System SHALL generate **distribution reports** showing question count by category and difficulty

#### 2.4 TournamentOrchestrator

**Responsibility:** Coordinate multi-model tournaments with elimination brackets.

**Key Methods:**
```csharp
public class TournamentOrchestrator
{
    Task<Tournament> CreateTournamentAsync(List<Guid> modelIds, TournamentFormat format);
    Task<MatchResult> RunMatchAsync(Guid tournamentId, Guid modelAId, Guid modelBId);
    Task<TournamentResult> GetTournamentResultsAsync(Guid tournamentId);
    Task<Guid> DetermineWinnerAsync(Guid tournamentId);
}
```

**Acceptance Criteria:**
61. AC-5.2.21: System SHALL support single-elimination and double-elimination tournament formats
62. AC-5.2.22: System SHALL automatically generate bracket pairings based on ELO ratings
63. AC-5.2.23: System SHALL run matches in parallel (up to 4 concurrent matches)
64. AC-5.2.24: System SHALL award tournament points: Winner (1000), Runner-up (500), Semi-finalist (250)
65. AC-5.2.25: System SHALL publish tournament results with bracket visualization

---

## Module 3: Synthetic Data Generation

### Purpose

Knowledge distillation pipeline with **real-time quality gates** and **A/B testing of teacher models**.

### Components

#### 3.1 SyntheticDataGenerator (UPDATED)

**Responsibility:** Generate high-quality training data using large teacher models with **real-time quality validation**.

**Changes in v2.0:**
- Integrated QualityGateService for real-time scoring
- Added automatic rejection of low-quality samples
- Implemented A/B testing of teacher models

**Rationale:** Addresses feedback point #3 - proactive quality validation prevents poor training data from entering the pipeline.

**Key Methods:**
```csharp
public class SyntheticDataGenerator
{
    private readonly IQualityGateService _qualityGate;  // NEW: Injected dependency
    private readonly IModelRegistryService _registry;
    
    Task<List<TrainingSample>> GenerateSamplesAsync(string teacherModel, SampleRequest request);
    
    // NEW: Real-time quality validation
    Task<TrainingSample> GenerateAndValidateSampleAsync(string teacherModel, SampleRequest request);
    
    // NEW: A/B testing
    Task<TeacherModelComparison> CompareTeacherModelsAsync(string modelA, string modelB, int sampleSize);
}
```

**Acceptance Criteria:**
66. AC-5.3.1: System SHALL generate training samples using GPT-4, Claude, or Llama 70B as teacher models
67. AC-5.3.2: System SHALL **score quality in real-time** using QualityGateService (coherence, relevance, correctness, diversity)
68. AC-5.3.3: System SHALL **automatically reject samples** with quality score < 0.7
69. AC-5.3.4: System SHALL **alert when rejection rate exceeds 30%** (indicates teacher model degradation)
70. AC-5.3.5: System SHALL support **A/B testing of teacher models** with statistical significance testing
71. AC-5.3.6: System SHALL generate diverse sample types: code review, test cases, requirement analysis, defect descriptions
72. AC-5.3.7: System SHALL track generation metrics: samples/hour, rejection rate, average quality score

#### 3.2 DataAugmentationService

**Responsibility:** Augment synthetic data with paraphrasing, edge cases, and adversarial examples.

**Key Methods:**
```csharp
public class DataAugmentationService
{
    Task<List<TrainingSample>> AugmentSamplesAsync(List<TrainingSample> samples, AugmentationStrategy strategy);
    Task<TrainingSample> ParaphraseSampleAsync(TrainingSample sample);
    Task<TrainingSample> GenerateEdgeCaseAsync(TrainingSample sample);
    Task<TrainingSample> GenerateAdversarialExampleAsync(TrainingSample sample);
}
```

**Acceptance Criteria:**
73. AC-5.3.8: System SHALL support 3 augmentation strategies: paraphrasing, edge case generation, adversarial examples
74. AC-5.3.9: System SHALL maintain semantic equivalence during paraphrasing (cosine similarity > 0.9)
75. AC-5.3.10: System SHALL generate edge cases covering boundary conditions and rare scenarios
76. AC-5.3.11: System SHALL generate adversarial examples testing model robustness
77. AC-5.3.12: System SHALL expand training dataset by 3x through augmentation

#### 3.3 KnowledgeDistillationOrchestrator

**Responsibility:** Coordinate fine-tuning of small models using synthetic data from large models.

**Key Methods:**
```csharp
public class KnowledgeDistillationOrchestrator
{
    Task<DistillationJob> StartDistillationAsync(string teacherModel, string studentModel, DistillationConfig config);
    Task<DistillationMetrics> GetDistillationMetricsAsync(Guid jobId);
    Task<bool> IsDistillationCompleteAsync(Guid jobId);
    Task<ModelMetadata> GetDistilledModelAsync(Guid jobId);
}
```

**Acceptance Criteria:**
78. AC-5.3.13: System SHALL fine-tune student models (Granite 4, Phi-3, Llama 8B) using teacher-generated data
79. AC-5.3.14: System SHALL achieve 85%+ of teacher model performance with student models
80. AC-5.3.15: System SHALL track distillation metrics: training loss, validation accuracy, inference latency
81. AC-5.3.16: System SHALL support incremental distillation (add new data without retraining from scratch)
82. AC-5.3.17: System SHALL validate distilled model performance before deployment

---

## Module 4: Resource-Efficient Training Pipeline

### Purpose

Lelapa AI-inspired training methodology with **resource monitoring**, **divergence detection**, and **automated rollback**.

### Components

#### 4.1 ModelTrainingPipeline (UPDATED)

**Responsibility:** Orchestrate model fine-tuning with **resource utilization alerts** and **training divergence detection**.

**Changes in v2.0:**
- Added resource utilization monitoring (CPU, RAM, GPU)
- Implemented training divergence detection
- Added automated rollback for poor-performing runs

**Rationale:** Addresses feedback point #6 - prevents resource contention and catches training failures early.

**Key Methods:**
```csharp
public class ModelTrainingPipeline
{
    private readonly IValidationService _validation;
    private readonly IAlertingService _alerting;  // NEW: Injected dependency
    
    Task<TrainingJob> StartTrainingAsync(Guid modelId, TrainingConfig config);
    Task<TrainingMetrics> GetTrainingMetricsAsync(Guid jobId);
    
    // NEW: Resource monitoring
    Task<ResourceUsage> GetResourceUsageAsync(Guid jobId);
    Task MonitorResourcesAsync(Guid jobId);
    
    // NEW: Divergence detection
    Task<bool> DetectDivergenceAsync(Guid jobId);
    Task RollbackTrainingAsync(Guid jobId);
}
```

**Acceptance Criteria:**
83. AC-5.4.1: System SHALL monitor resource utilization: CPU, RAM, GPU, disk I/O
84. AC-5.4.2: System SHALL **alert when CPU usage exceeds 90%** for 5+ minutes
85. AC-5.4.3: System SHALL **alert when RAM usage exceeds 80%** of available memory
86. AC-5.4.4: System SHALL **detect training divergence**: loss increasing for 3+ consecutive epochs
87. AC-5.4.5: System SHALL **automatically rollback** to last stable checkpoint when divergence detected
88. AC-5.4.6: System SHALL support distributed training across multiple nodes
89. AC-5.4.7: System SHALL implement gradient checkpointing for memory efficiency
90. AC-5.4.8: System SHALL support mixed-precision training (FP16) for 2x speedup

---

## Module 5: Content Ingestion Pipeline

### Purpose

Microsoft Learn crawler with **dynamic rate limiting** based on target site responses.

### Components

#### 5.1 MicrosoftLearnCrawler (UPDATED)

**Responsibility:** Crawl Microsoft Learn documentation with **adaptive rate limiting** and error handling.

**Changes in v2.0:**
- Replaced hardcoded 10 req/sec with dynamic rate limiting
- Added response header parsing for rate limit hints
- Implemented backoff strategy for error rates

**Rationale:** Addresses feedback point #8 - prevents being too aggressive or too conservative based on target site capacity.

**Key Methods:**
```csharp
public class MicrosoftLearnCrawler
{
    private readonly IRateLimiter _rateLimiter;  // NEW: Dynamic rate limiter
    
    Task<CrawlResult> CrawlAsync(string startUrl, CrawlOptions options);
    
    // NEW: Dynamic rate limiting
    Task<int> CalculateOptimalRateAsync(string domain);
    Task AdjustRateLimitAsync(string domain, HttpResponseMessage response);
    
    Task<List<Page>> GetCrawledPagesAsync(Guid crawlId);
    Task<CrawlStatistics> GetCrawlStatisticsAsync(Guid crawlId);
}
```

**Acceptance Criteria:**
91. AC-5.5.1: System SHALL crawl Microsoft Learn documentation (100,000+ pages)
92. AC-5.5.2: System SHALL **dynamically adjust rate limit** based on response headers (Retry-After, X-RateLimit-Remaining)
93. AC-5.5.3: System SHALL **implement backoff strategy**: 429 errors → reduce rate by 50%, 503 errors → pause 60s
94. AC-5.5.4: System SHALL **monitor error rate**: if > 5% → reduce rate by 25%
95. AC-5.5.5: System SHALL extract content, code samples, images, and metadata
96. AC-5.5.6: System SHALL support incremental crawling (only new/updated pages)
97. AC-5.5.7: System SHALL respect robots.txt and crawl-delay directives

#### 5.2 KnowledgeGraphBuilder

**Responsibility:** Construct knowledge graph from crawled content with entities and relationships.

**Key Methods:**
```csharp
public class KnowledgeGraphBuilder
{
    Task<KnowledgeGraph> BuildGraphAsync(List<Page> pages);
    Task<List<Entity>> ExtractEntitiesAsync(Page page);
    Task<List<Relationship>> ExtractRelationshipsAsync(Page page);
    Task<KnowledgeGraph> GetGraphAsync();
}
```

**Acceptance Criteria:**
98. AC-5.5.8: System SHALL extract entities: concepts, APIs, classes, methods, parameters
99. AC-5.5.9: System SHALL extract relationships: inherits-from, implements, calls, depends-on
100. AC-5.5.10: System SHALL build knowledge graph with 50,000+ entities and 200,000+ relationships
101. AC-5.5.11: System SHALL support graph queries (SPARQL) for question generation
102. AC-5.5.12: System SHALL update graph incrementally when new content crawled

---

## Module 6: Tool Use Training

### Purpose

Train models to use DevOps tools and APIs with **adversarial edge case handling**.

### Components

#### 6.1 ToolUseTrainer (UPDATED)

**Responsibility:** Generate training examples for function calling with **adversarial edge cases**.

**Changes in v2.0:**
- Added adversarial training examples for API failures
- Included network timeout handling patterns
- Added authentication failure recovery examples
- **Integrated security validation** (SQL injection testing moved here from separate module)

**Rationale:** Addresses feedback points #1 and #9 - embeds security throughout training and handles real-world failure scenarios.

**Key Methods:**
```csharp
public class ToolUseTrainer
{
    private readonly IValidationService _validation;  // NEW: Security validation
    private readonly IAPIGatewayService _apiGateway;
    
    Task<List<TrainingSample>> GenerateToolUseSamplesAsync(ToolType tool, int count);
    
    // NEW: Adversarial edge cases
    Task<TrainingSample> GeneratePartialFailureExampleAsync(ToolType tool);
    Task<TrainingSample> GenerateTimeoutExampleAsync(ToolType tool);
    Task<TrainingSample> GenerateAuthFailureExampleAsync(ToolType tool);
    
    // NEW: Security validation (moved from separate module)
    Task<bool> ValidateSQLInjectionSafetyAsync(string sqlQuery);
    Task<bool> ValidatePromptInjectionSafetyAsync(string prompt);
}
```

**Acceptance Criteria:**
103. AC-5.6.1: System SHALL generate training examples for Azure DevOps API, GitHub API, Jira API, Playwright, SQL, Bing Search
104. AC-5.6.2: System SHALL **generate adversarial examples** for partial API failures (e.g., 500 errors, timeouts)
105. AC-5.6.3: System SHALL **include network timeout handling** patterns (retry with exponential backoff)
106. AC-5.6.4: System SHALL **include authentication failure recovery** examples (token refresh, re-authentication)
107. AC-5.6.5: System SHALL **validate SQL queries for injection vulnerabilities** before including in training data
108. AC-5.6.6: System SHALL **validate prompts for injection attempts** before including in training data
109. AC-5.6.7: System SHALL generate multi-step workflows (search → analyze → execute → verify)

#### 6.2 APIIntegrationManager (UPDATED)

**Responsibility:** Manage API credentials and sandboxing via **APIGatewayService**.

**Changes in v2.0:**
- Replaced direct API calls with APIGatewayService
- Simplified credential management (delegated to gateway)
- Removed provider-specific code

**Rationale:** Addresses feedback point #5 - unified API gateway reduces complexity.

**Key Methods:**
```csharp
public class APIIntegrationManager
{
    private readonly IAPIGatewayService _apiGateway;  // NEW: Use gateway instead of direct calls
    
    Task<bool> ValidateAPIAccessAsync(string provider);
    Task<APIUsageReport> GetAPIUsageReportAsync(string provider, DateTime start, DateTime end);
    Task<bool> TestAPICallAsync(string provider, string endpoint);
}
```

**Acceptance Criteria:**
110. AC-5.6.8: System SHALL **use APIGatewayService** for all external API calls
111. AC-5.6.9: System SHALL validate API access before training (test call to each provider)
112. AC-5.6.10: System SHALL track API usage: calls/day, errors, latency
113. AC-5.6.11: System SHALL support API sandboxing for safe training (test environments)
114. AC-5.6.12: System SHALL rotate API credentials automatically when expiration detected

---

## Module 7: Evaluation Framework

### Purpose

Automated testing and performance benchmarking with **enhanced metrics**.

### Components

#### 7.1 ModelEvaluator (UPDATED)

**Responsibility:** Evaluate model performance using **ValidationService** for consolidated validation logic.

**Changes in v2.0:**
- Replaced duplicate validation code with ValidationService
- Added enhanced metrics: tool use accuracy, security vulnerability detection rate
- Integrated with AI Arena for competitive benchmarking

**Rationale:** Addresses feedback point #7 - eliminates duplicate validation code.

**Key Methods:**
```csharp
public class ModelEvaluator
{
    private readonly IValidationService _validation;  // NEW: Use ValidationService
    private readonly IArenaGameEngine _arena;
    
    Task<EvaluationResult> EvaluateModelAsync(Guid modelId, TestDataset dataset);
    Task<ComparisonReport> CompareModelsAsync(Guid modelAId, Guid modelBId, TestDataset dataset);
    
    // NEW: Enhanced metrics
    Task<double> EvaluateToolUseAccuracyAsync(Guid modelId, List<ToolUseTest> tests);
    Task<double> EvaluateSecurityVulnerabilityDetectionAsync(Guid modelId, List<SecurityTest> tests);
}
```

**Acceptance Criteria:**
115. AC-5.7.1: System SHALL **use ValidationService** for all model validation checks
116. AC-5.7.2: System SHALL evaluate models on test datasets: accuracy, latency, error rate
117. AC-5.7.3: System SHALL **evaluate tool use accuracy**: correct API calls, parameter values, error handling
118. AC-5.7.4: System SHALL **evaluate security vulnerability detection rate**: SQL injection, XSS, CSRF
119. AC-5.7.5: System SHALL compare models with statistical significance testing
120. AC-5.7.6: System SHALL integrate with AI Arena for competitive benchmarking
121. AC-5.7.7: System SHALL generate evaluation reports with visualizations (charts, tables)

---

## Cross-Module Integration

### Dependency Graph

```
┌─────────────────────────────────────────────────────────────────┐
│                     Cross-Cutting Services                       │
│  APIGatewayService │ ValidationService │ QualityGateService     │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │ (used by all modules)
                              │
┌─────────────────────────────┴─────────────────────────────────┐
│                                                                 │
│  Module 1: Model Management Console                            │
│  ├─ ModelRegistryService                                       │
│  ├─ ModelDeploymentOrchestrator (uses ValidationService)       │
│  └─ ABTestingManager                                           │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Module 2: AI Arena                                            │
│  ├─ ArenaGameEngine                                            │
│  ├─ ArenaLeaderboardManager (incremental ELO, sharding)        │
│  ├─ QuestionBankManager (dynamic generation, ML calibration)   │
│  └─ TournamentOrchestrator                                     │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Module 3: Synthetic Data Generation                           │
│  ├─ SyntheticDataGenerator (uses QualityGateService)           │
│  ├─ DataAugmentationService                                    │
│  └─ KnowledgeDistillationOrchestrator                          │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Module 4: Resource-Efficient Training                         │
│  └─ ModelTrainingPipeline (monitoring, divergence, rollback)   │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Module 5: Content Ingestion                                   │
│  ├─ MicrosoftLearnCrawler (dynamic rate limiting)              │
│  └─ KnowledgeGraphBuilder                                      │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Module 6: Tool Use Training                                   │
│  ├─ ToolUseTrainer (adversarial, security validation)          │
│  └─ APIIntegrationManager (uses APIGatewayService)             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Module 7: Evaluation Framework                                 │
│  └─ ModelEvaluator (uses ValidationService)                     │
└─────────────────────────────────────────────────────────────────┘
```

### Interface Contracts

**Critical Interfaces:**

1. **IValidationService** - Used by ModelDeploymentOrchestrator, ModelEvaluator, ModelTrainingPipeline
2. **IAPIGatewayService** - Used by ToolUseTrainer, APIIntegrationManager, ArenaGameEngine (Bing Search)
3. **IQualityGateService** - Used by SyntheticDataGenerator
4. **IModelRegistryService** - Used by all modules for model metadata access

### Staged Deployment Plan

**Phase 1: Foundation (Weeks 1-8)**
- Deploy cross-cutting services: APIGatewayService, ValidationService, QualityGateService
- Deploy ModelRegistryService
- **Validation checkpoint**: All services pass health checks, API gateway successfully routes to all providers

**Phase 2: Core Modules (Weeks 9-16)**
- Deploy Model Management Console (ModelDeploymentOrchestrator, ABTestingManager)
- Deploy AI Arena (ArenaGameEngine, ArenaLeaderboardManager, QuestionBankManager, TournamentOrchestrator)
- Deploy Content Ingestion (MicrosoftLearnCrawler, KnowledgeGraphBuilder)
- **Validation checkpoint**: Models can be deployed, AI Arena games run successfully, 10,000+ pages crawled

**Phase 3: Training & Evaluation (Weeks 17-24)**
- Deploy Synthetic Data Generation (SyntheticDataGenerator, DataAugmentationService, KnowledgeDistillationOrchestrator)
- Deploy Resource-Efficient Training (ModelTrainingPipeline)
- Deploy Tool Use Training (ToolUseTrainer, APIIntegrationManager)
- Deploy Evaluation Framework (ModelEvaluator)
- **Validation checkpoint**: Synthetic data generated with quality scores > 0.7, models fine-tuned successfully, evaluation reports generated

---

## Success Metrics

| **Metric** | **Target** | **Measurement Method** |
|------------|-----------|------------------------|
| Model Deployment Success Rate | 95%+ | Successful deployments / total attempts |
| Inference Latency (95th percentile) | < 2s | Measured via ValidationService |
| Synthetic Data Quality Score | > 0.7 | QualityGateService scoring |
| Synthetic Data Rejection Rate | < 30% | Rejected samples / total generated |
| Training Divergence Detection | 100% | Caught divergences / total divergences |
| API Gateway Uptime | 99.9% | Uptime monitoring |
| AI Arena Game Completion Rate | 90%+ | Completed games / started games |
| ELO Calculation Latency | < 100ms | Incremental update time |
| Content Crawl Rate | 10+ pages/sec | Dynamic rate limiter performance |
| Tool Use Accuracy | 85%+ | Correct API calls / total calls |
| Security Vulnerability Detection | 95%+ | Detected vulnerabilities / total vulnerabilities |

---

## Implementation Timeline

**Total Duration:** 24 weeks (6 months)  
**Team Size:** 5 engineers (2 backend, 1 ML, 1 DevOps, 1 QA)  
**Investment:** $300K

### Phase 1: Foundation (Weeks 1-8)
- Week 1-2: APIGatewayService (provider adapters, rate limiting)
- Week 3-4: ValidationService (health checks, security validation)
- Week 5-6: QualityGateService (real-time scoring, A/B testing)
- Week 7-8: ModelRegistryService, integration testing

### Phase 2: Core Modules (Weeks 9-16)
- Week 9-10: ModelDeploymentOrchestrator, ABTestingManager
- Week 11-12: AI Arena (ArenaGameEngine, ArenaLeaderboardManager)
- Week 13-14: QuestionBankManager (dynamic generation, ML calibration), TournamentOrchestrator
- Week 15-16: Content Ingestion (MicrosoftLearnCrawler, KnowledgeGraphBuilder)

### Phase 3: Training & Evaluation (Weeks 17-24)
- Week 17-18: SyntheticDataGenerator (quality gates), DataAugmentationService
- Week 19-20: KnowledgeDistillationOrchestrator, ModelTrainingPipeline (monitoring, rollback)
- Week 21-22: ToolUseTrainer (adversarial), APIIntegrationManager
- Week 23-24: ModelEvaluator, end-to-end testing, documentation

---

## Security Considerations

### Security-First Design

**Embedded Security Validation:**
- SQL injection testing integrated into ToolUseTrainer validation pipeline
- Prompt injection detection in SyntheticDataGenerator
- Output sanitization in ModelDeploymentOrchestrator
- Security vulnerability detection in ModelEvaluator

**API Security:**
- All API credentials managed via Azure Key Vault
- API calls routed through APIGatewayService with rate limiting
- Circuit breaker pattern prevents cascading failures
- Audit logging for all external API calls

**Model Security:**
- Security validation before deployment (ValidationService)
- Sandboxed API testing environments
- Automated security scanning of training data
- Adversarial testing for robustness

---

## Appendix A: Feedback Implementation Summary

| **Feedback Point** | **Action Taken** | **Impact** |
|--------------------|------------------|------------|
| 1. Security Integration Gap | Embedded security validation into ModelDeploymentOrchestrator, ToolUseTrainer | +3 AC, security-first design |
| 2. Scalability Concerns (ELO) | Incremental ELO calculation, leaderboard sharding, caching | +3 AC, 10x performance improvement |
| 3. Synthetic Data Quality | Real-time quality scoring, automatic rejection, A/B testing | +6 AC, new QualityGateService |
| 4. Question Bank Scalability | Dynamic generation, ML-based calibration, distribution monitoring | +3 AC, balanced question distribution |
| 5. API Integration Complexity | Unified API gateway abstracting providers | +8 AC, new APIGatewayService |
| 6. Training Pipeline Monitoring | Resource alerts, divergence detection, automated rollback | +3 AC, prevents training failures |
| 7. Model Deployment Validation Overlap | Consolidated validation logic into ValidationService | +7 AC, new ValidationService, eliminated duplication |
| 8. Content Ingestion Rate Limiting | Dynamic rate limiting based on response headers, backoff strategy | +3 AC, adaptive crawling |
| 9. Tool Use Training Edge Cases | Adversarial examples for API failures, timeouts, auth failures | +3 AC, robust error handling |
| 10. Cross-Module Dependency Management | Dependency graph, interface contracts, staged deployment plan | Improved architecture clarity |

**Total Impact:**
- **+3 new classes** (APIGatewayService, ValidationService, QualityGateService)
- **+21 new acceptance criteria**
- **+4 weeks implementation time**
- **+$50K investment**
- **Significantly improved security, scalability, and maintainability**

---

## Appendix B: Comparison with v1.0

| **Aspect** | **v1.0** | **v2.0** | **Improvement** |
|------------|----------|----------|-----------------|
| Classes | 18 | 21 | +3 (cross-cutting services) |
| Acceptance Criteria | 124 | 145 | +21 (security, scalability, quality) |
| Implementation Time | 20 weeks | 24 weeks | +4 weeks (enhanced architecture) |
| Investment | $250K | $300K | +$50K (security & scalability) |
| Security Integration | Separate modules | Embedded throughout | Security-first design |
| API Management | Provider-specific | Unified gateway | Reduced complexity |
| Validation Logic | Duplicated | Consolidated | Eliminated duplication |
| ELO Calculation | Full recalculation | Incremental | 10x faster |
| Quality Assurance | Reactive | Proactive | Real-time validation |
| Rate Limiting | Hardcoded | Dynamic | Adaptive to target sites |
| Training Monitoring | Basic | Comprehensive | Resource alerts, divergence detection |
| Tool Use Training | Happy path | Adversarial | Robust error handling |

---

**End of Phase 5 Architecture Specification v2.0**
