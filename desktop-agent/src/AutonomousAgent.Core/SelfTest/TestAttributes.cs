namespace AutonomousAgent.Core.SelfTest
{
    /// <summary>
    /// Attribute to mark a method as a Function-Level (Unit) Test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FunctionTestAttribute : Attribute
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Attribute to mark a method as a Class-Level (Integration) Test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ClassTestAttribute : Attribute
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Attribute to mark a method as a Module-Level (Component) Test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ModuleTestAttribute : Attribute
    {
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Attribute to mark a method as a System-Level (End-to-End) Test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SystemTestAttribute : Attribute
    {
        public string Description { get; set; } = string.Empty;
    }
}
