# Execution Minions

**Distributed test execution system with autonomous Windows PC agents**

## Status: Coming Soon

The execution minions system is currently in the design phase. Implementation will begin in Phase 5 of the project roadmap.

## Planned Features

### Autonomous Minions
- Self-provisioning test executors for Windows PCs
- Automatic dependency installation (Java, .NET, Python, browsers)
- Podman-based test isolation
- Heartbeat monitoring and health checks

### Test Hub
- Central orchestrator (ASP.NET Core 8)
- Job queue management (PostgreSQL)
- Minion registration and discovery
- Load balancing and failover

### Video Streaming
- Dual-quality recording:
  - **High-quality**: 1080p @ 30fps → Azure DevOps attachments
  - **Low-quality**: 480p @ 15fps → Real-time CRT monitor
- FFmpeg-based capture
- Nginx RTMP streaming server

### CRT Monitor Interface
- Retro-themed web interface
- Channel dial to switch between minion streams
- Real-time viewing with <3 second latency
- Authentic CRT effects (scan lines, curvature, static noise)

## Architecture

```
Test Hub (Orchestrator)
    ↓
Job Queue (PostgreSQL)
    ↓
Execution Minions (Windows PCs)
    ↓
RTMP Server (Nginx)
    ↓
CRT Monitor (React/Vue.js)
```

## Documentation

See the [Distributed Test Execution Design](../docs/distributed_test_execution_design.md) for complete specifications.

---

**Stay tuned for updates!**
