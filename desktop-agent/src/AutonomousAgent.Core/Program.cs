using AutonomousAgent.Core;
using AutonomousAgent.Core.SelfTest;
using AutonomousAgent.Core.Scheduling;

var builder = Host.CreateApplicationBuilder(args);

// Configure scheduling settings
builder.Services.Configure<SchedulingConfig>(
    builder.Configuration.GetSection("Scheduler"));

// Register services
builder.Services.AddSingleton<SelfTestManager>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SchedulingService>();

var host = builder.Build();
host.Run();
