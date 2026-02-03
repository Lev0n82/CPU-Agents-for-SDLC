using System.Diagnostics;
using System.Reflection;

namespace AutonomousAgent.Core.SelfTest
{
    public class SelfTestManager
    {
        private readonly ILogger<SelfTestManager> _logger;

        public SelfTestManager(ILogger<SelfTestManager> logger)
        {
            _logger = logger;
        }

        public async Task<TestSummary> RunAllTestsAsync()
        {
            var summary = new TestSummary();
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("=== Starting Self-Test Sequence ===");

            // Discover all test methods in the current assembly
            var assembly = Assembly.GetExecutingAssembly();
            var testMethods = DiscoverTestMethods(assembly);

            _logger.LogInformation("Discovered {count} test methods", testMethods.Count);

            // Execute tests in order: Function -> Class -> Module -> System
            var orderedTests = testMethods
                .OrderBy(t => GetTestLevelOrder(t.Level))
                .ThenBy(t => t.Method.Name);

            foreach (var testInfo in orderedTests)
            {
                var result = await ExecuteTestAsync(testInfo);
                summary.Results.Add(result);

                if (result.Passed)
                {
                    summary.PassedTests++;
                    _logger.LogInformation("[PASS] {level} - {name} ({duration}ms)", 
                        result.TestLevel, result.TestName, result.Duration.TotalMilliseconds);
                }
                else
                {
                    summary.FailedTests++;
                    _logger.LogError("[FAIL] {level} - {name}: {error}", 
                        result.TestLevel, result.TestName, result.ErrorMessage);
                }
            }

            stopwatch.Stop();
            summary.TotalTests = testMethods.Count;
            summary.TotalDuration = stopwatch.Elapsed;

            _logger.LogInformation("=== Self-Test Sequence Complete ===");
            _logger.LogInformation("Total: {total}, Passed: {passed}, Failed: {failed}, Duration: {duration}ms",
                summary.TotalTests, summary.PassedTests, summary.FailedTests, summary.TotalDuration.TotalMilliseconds);

            return summary;
        }

        private List<TestMethodInfo> DiscoverTestMethods(Assembly assembly)
        {
            var testMethods = new List<TestMethodInfo>();

            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
                {
                    var functionTest = method.GetCustomAttribute<FunctionTestAttribute>();
                    if (functionTest != null)
                    {
                        testMethods.Add(new TestMethodInfo
                        {
                            Method = method,
                            Level = "Function",
                            Description = functionTest.Description,
                            DeclaringType = type
                        });
                        continue;
                    }

                    var classTest = method.GetCustomAttribute<ClassTestAttribute>();
                    if (classTest != null)
                    {
                        testMethods.Add(new TestMethodInfo
                        {
                            Method = method,
                            Level = "Class",
                            Description = classTest.Description,
                            DeclaringType = type
                        });
                        continue;
                    }

                    var moduleTest = method.GetCustomAttribute<ModuleTestAttribute>();
                    if (moduleTest != null)
                    {
                        testMethods.Add(new TestMethodInfo
                        {
                            Method = method,
                            Level = "Module",
                            Description = moduleTest.Description,
                            DeclaringType = type
                        });
                        continue;
                    }

                    var systemTest = method.GetCustomAttribute<SystemTestAttribute>();
                    if (systemTest != null)
                    {
                        testMethods.Add(new TestMethodInfo
                        {
                            Method = method,
                            Level = "System",
                            Description = systemTest.Description,
                            DeclaringType = type
                        });
                    }
                }
            }

            return testMethods;
        }

        private async Task<TestResult> ExecuteTestAsync(TestMethodInfo testInfo)
        {
            var result = new TestResult
            {
                TestName = testInfo.Method.Name,
                TestLevel = testInfo.Level,
                Description = testInfo.Description
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Create instance if method is not static
                object? instance = null;
                if (!testInfo.Method.IsStatic)
                {
                    instance = Activator.CreateInstance(testInfo.DeclaringType);
                }

                // Invoke the test method
                var returnValue = testInfo.Method.Invoke(instance, null);

                // Handle async methods
                if (returnValue is Task task)
                {
                    await task;
                }

                result.Passed = true;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }

            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            return result;
        }

        private int GetTestLevelOrder(string level)
        {
            return level switch
            {
                "Function" => 1,
                "Class" => 2,
                "Module" => 3,
                "System" => 4,
                _ => 5
            };
        }

        private class TestMethodInfo
        {
            public MethodInfo Method { get; set; } = null!;
            public string Level { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public Type DeclaringType { get; set; } = null!;
        }
    }
}
