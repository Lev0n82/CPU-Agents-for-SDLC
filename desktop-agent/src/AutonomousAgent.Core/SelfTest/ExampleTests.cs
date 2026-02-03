namespace AutonomousAgent.Core.SelfTest
{
    /// <summary>
    /// Example self-tests to demonstrate the framework
    /// </summary>
    public class ExampleTests
    {
        [FunctionTest(Description = "Verify basic arithmetic operations")]
        public void TestBasicArithmetic()
        {
            var result = 2 + 2;
            if (result != 4)
            {
                throw new Exception($"Expected 4, but got {result}");
            }
        }

        [FunctionTest(Description = "Verify string operations")]
        public void TestStringOperations()
        {
            var str = "Hello, World!";
            if (!str.Contains("World"))
            {
                throw new Exception("String does not contain 'World'");
            }
        }

        [ClassTest(Description = "Verify DateTime functionality")]
        public void TestDateTimeOperations()
        {
            var now = DateTime.Now;
            var tomorrow = now.AddDays(1);
            
            if (tomorrow <= now)
            {
                throw new Exception("Tomorrow should be after today");
            }
        }

        [ModuleTest(Description = "Verify file system access")]
        public void TestFileSystemAccess()
        {
            var tempPath = Path.GetTempPath();
            if (!Directory.Exists(tempPath))
            {
                throw new Exception("Temp directory does not exist");
            }
        }

        [SystemTest(Description = "Verify system environment variables")]
        public void TestSystemEnvironment()
        {
            var userName = Environment.UserName;
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("User name is not set");
            }

            var machineName = Environment.MachineName;
            if (string.IsNullOrEmpty(machineName))
            {
                throw new Exception("Machine name is not set");
            }
        }

        [SystemTest(Description = "Verify async operations")]
        public async Task TestAsyncOperations()
        {
            await Task.Delay(100);
            var result = await Task.FromResult(42);
            
            if (result != 42)
            {
                throw new Exception($"Expected 42, but got {result}");
            }
        }
    }
}
