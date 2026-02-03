namespace AutonomousAgent.Core.SelfTest
{
    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public string TestLevel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class TestSummary
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public List<TestResult> Results { get; set; } = new();

        public bool AllTestsPassed => FailedTests == 0;
    }
}
