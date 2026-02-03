# Autonomous AI Agent for Enterprise Testing & Requirements Management

A self-aware, CPU-optimized autonomous AI agent designed for Windows 11 enterprise desktops, featuring comprehensive self-testing on startup and configurable scheduled maintenance.

## Features

### 🔍 Multi-Level Self-Testing Framework

The agent includes a comprehensive self-testing framework that validates its health at four distinct levels:

1. **Function-Level Tests**: Unit tests for individual methods and functions
2. **Class-Level Tests**: Integration tests for classes and their dependencies
3. **Module-Level Tests**: Component tests for complete modules
4. **System-Level Tests**: End-to-end tests for the entire system

All tests are automatically executed on startup before the agent begins its operational tasks.

### ⏰ Proactive Scheduling System

The agent includes a self-aware scheduling service that can:

- **Configurable Midnight Reboot**: Automatically reboot the system at a configured time (default: midnight)
- **Graceful Shutdown**: Safely terminates all tasks before initiating a reboot
- **Extensible Rules**: Framework for adding additional scheduled tasks and self-awareness triggers

### 🏗️ Architecture

- **Worker Service**: Main background service that orchestrates agent operations
- **Self-Test Manager**: Discovers and executes all self-tests using reflection
- **Scheduling Service**: Background service for proactive task execution
- **Modular Design**: Easy to extend with new capabilities

## Configuration

Edit `appsettings.json` to configure the agent:

```json
{
  "Scheduler": {
    "NightlyReboot": {
      "Enabled": true,
      "Hour": 0,
      "Minute": 0
    }
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `Scheduler.NightlyReboot.Enabled` | Enable/disable automatic nightly reboot | `true` |
| `Scheduler.NightlyReboot.Hour` | Hour to perform reboot (0-23) | `0` (midnight) |
| `Scheduler.NightlyReboot.Minute` | Minute to perform reboot (0-59) | `0` |

## Building the Project

```bash
cd /home/ubuntu/agent_src
dotnet build
```

## Running the Agent

### Development Mode

```bash
cd /home/ubuntu/agent_src/src/AutonomousAgent.Core
dotnet run
```

### Production Mode (Windows Service)

To install as a Windows Service:

1. Build the project in Release mode:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Install as a Windows Service using sc.exe:
   ```cmd
   sc create "AutonomousAgent" binPath="C:\Path\To\AutonomousAgent.Core.exe"
   sc config "AutonomousAgent" start=auto
   sc start "AutonomousAgent"
   ```

## Adding New Self-Tests

To add a new self-test, create a method and decorate it with the appropriate attribute:

```csharp
[FunctionTest(Description = "Verify my new function works")]
public void TestMyNewFunction()
{
    var result = MyNewFunction();
    if (result != expectedValue)
    {
        throw new Exception("Test failed");
    }
}
```

Available test attributes:
- `[FunctionTest]` - Function-level (unit) tests
- `[ClassTest]` - Class-level (integration) tests
- `[ModuleTest]` - Module-level (component) tests
- `[SystemTest]` - System-level (end-to-end) tests

## Project Structure

```
agent_src/
├── src/
│   └── AutonomousAgent.Core/
│       ├── Program.cs                 # Entry point
│       ├── Worker.cs                  # Main worker service
│       ├── appsettings.json          # Configuration
│       ├── SelfTest/
│       │   ├── TestAttributes.cs     # Test attribute definitions
│       │   ├── TestResult.cs         # Test result models
│       │   ├── SelfTestManager.cs    # Test discovery and execution
│       │   └── ExampleTests.cs       # Example self-tests
│       └── Scheduling/
│           ├── SchedulingConfig.cs   # Configuration models
│           └── SchedulingService.cs  # Scheduling service implementation
└── tests/
    └── (Future unit tests)
```

## Requirements

- .NET 8.0 SDK or later
- Windows 10/11 (for Windows Service deployment)
- Administrator privileges (for system reboot functionality)

## Future Enhancements

This is a foundational implementation. Future enhancements will include:

- Azure DevOps integration for requirements and test case management
- LLM/SLM integration via llama.cpp for autonomous reasoning
- Test case generation from requirements
- Accessibility testing (WCAG 2.2 AAA compliance)
- Integration with Playwright for end-to-end testing
- Database connectivity (PostgreSQL, Oracle)
- Distributed test execution with execution minions

## License

[To be determined]

## Author

Manus AI - January 31, 2026
