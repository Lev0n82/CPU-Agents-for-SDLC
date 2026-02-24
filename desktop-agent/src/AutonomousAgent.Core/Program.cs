using AutonomousAgent.Core;
using AutonomousAgent.Core.SelfTest;
using AutonomousAgent.Core.Scheduling;
using AutonomousAgent.Core.Telemetry;

var builder = Host.CreateApplicationBuilder(args);

// Add OpenTelemetry instrumentation
builder.AddOpenTelemetryInstrumentation(
    serviceName: "AutonomousAgent",
    collectorEndpoint: builder.Configuration.GetValue<string>("OpenTelemetry:Endpoint") ?? "http://localhost:18418");

// Configure scheduling settings
builder.Services.Configure<SchedulingConfig>(
    builder.Configuration.GetSection("Scheduler"));

// Register services
builder.Services.AddSingleton<SelfTestManager>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SchedulingService>();

var host = builder.Build();
host.Run();
