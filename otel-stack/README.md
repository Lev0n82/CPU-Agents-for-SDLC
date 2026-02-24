# OpenTelemetry Instrumentation Stack

This repository provides a complete OpenTelemetry observability stack for monitoring CPU Agents for SDLC applications.

## Overview

The stack includes:
- **OpenTelemetry Collector** - Receives and processes telemetry data
- **Jaeger** - Distributed tracing backend
- **Prometheus** - Metrics collection and storage
- **Grafana** - Visualization and alerting dashboards
- **Custom Dashboards** - Application-specific monitoring
- **Alerting Rules** - Proactive monitoring alerts

## Quick Start

### 1. Start the OpenTelemetry Stack

```powershell
# Navigate to the otel-stack directory
cd otel-stack

# Deploy the stack
./deploy-stack.ps1
```

### 2. Build and Run Instrumented Applications

For Autonomous Agent:
```bash
cd desktop-agent
# Add OpenTelemetry packages should already be included
# Build and run the service
```

For Azure DevOps Agent:
```bash
cd src/Phase3.AzureDevOps
# OpenTelemetry packages are already included
# Build and run tests
```

### 3. Access Monitoring Interfaces

- **Grafana Dashboard**: http://localhost:13000
  - Username: `admin`
  - Password: `admin123` (or set via GRAFANA_PASSWORD environment variable)

- **Jaeger UI**: http://localhost:17686
- **Prometheus UI**: http://localhost:19090

## Architecture

### Data Flow

```
Applications → OpenTelemetry Collector → [Jaeger | Prometheus] → Grafana
```

1. **Applications** send telemetry data to the OpenTelemetry Collector via OTLP (OpenTelemetry Protocol)
2. **Collector** processes and routes data to appropriate backends
3. **Jaeger** stores traces for distributed tracing analysis
4. **Prometheus** collects and stores metrics
5. **Grafana** visualizes data from both backends

### Port Configuration

| Service | Port | Purpose |
|---------|------|---------|
| Collector HTTP | 18418 | OTLP HTTP endpoint |
| Collector gRPC | 18417 | OTLP gRPC endpoint |
| Collector Metrics | 18889 | Prometheus scraping |
| Jaeger UI | 17686 | Trace visualization |
| Prometheus | 19090 | Metrics query UI |
| Grafana | 13000 | Dashboard UI |

## Instrumentation Details

### Autonomous Agent Instrumentation

#### Metrics Tracked
- `tasks.executed` - Number of tasks executed
- `selftests.completed` - Self-test completions
- `scheduling.iterations` - Scheduling loop iterations
- `llm.requests` - LLM API requests
- `cpu.usage` - CPU utilization percentage
- `memory.usage` - Memory usage in MB
- `task.execution_time` - Histogram of task execution times
- `llm.response_time` - Histogram of LLM response times

#### Traces Tracked
- `agent.startup` - Agent initialization
- `execute.task` - Individual task execution
- `execute.selftest` - Self-test execution
- `schedule.tasks` - Scheduling operations
- `llm.request` - LLM API calls

### Azure DevOps Agent Instrumentation

#### Metrics Tracked
- `workitems.processed` - Work items processed
- `authentication.attempts` - Authentication attempts
- `api.calls` - API calls made
- `workitems.active` - Currently active work items
- `api.response_time` - API response times

#### Traces Tracked
- `process.workitem` - Work item processing
- `authenticate` - Authentication operations
- `api.call` - API calls

## Dashboards

### Enhanced Dashboard Features

**All dashboards include:**
- 🎯 Real-time metrics with threshold-based coloring
- 📊 Interactive charts with configurable time ranges
- ⚡ Performance percentiles (P50, P95)
- 🔔 Alert annotations and status indicators
- 🔄 Auto-refresh intervals (5s to 1d)
- 📈 Custom gauges for resource monitoring

### Available Dashboards

1. **OpenTelemetry Overview** - System health, collector performance, alert status
2. **Autonomous Agent Dashboard** - Task execution, LLM requests, resource usage
3. **Azure DevOps Dashboard** - Work item processing, API metrics, authentication

### Quick Dashboard Navigation
- **Green indicators**: Healthy operation
- **Yellow indicators**: Warning levels
- **Red indicators**: Critical issues needing attention
- **Annotations**: Active alerts appear as timeline markers

### Dashboard URLs
- **Grafana**: http://localhost:13000 (admin/admin123)
- **Dashboards automatically provisioned on startup**

## Alerting

### Alert Rules Configured

#### Autonomous Agent Alerts
- High Memory Usage (>500MB for 5min)
- High CPU Usage (>80% for 2min)
- Low Task Processing Rate (<0.1 tasks/sec for 5min)
- Self-Test Failures (no completions for 10min)

#### Azure DevOps Agent Alerts
- High API Response Time (>2000ms for 3min)
- Low Work Item Processing Rate (<0.05 items/sec for 5min)
- High Authentication Failure Rate (>50% for 2min)

#### General Alerts
- Service Down (any service unavailable for 1min)

## Configuration

### Application Configuration

Add OpenTelemetry to your application:

```csharp
// .NET example
builder.AddOpenTelemetryInstrumentation(
    serviceName: "YourServiceName",
    collectorEndpoint: "http://localhost:18418");
```

### Environment Variables

- `GRAFANA_PASSWORD` - Grafana admin password (default: `admin123`)
- `OTEL_COLLECTOR_ENDPOINT` - Collector endpoint for applications

## Deployment

### Docker Compose Stack

The stack is deployed using Docker Compose:

```yaml
services:
  otel-collector:  # OpenTelemetry data collection
  jaeger:          # Distributed tracing
  prometheus:      # Metrics storage
  grafana:         # Visualization
```

### Customization

To customize the configuration:

1. Update `config/otel-collector-config.yaml` for collector settings
2. Modify `config/prometheus.yml` for metrics scraping
3. Edit dashboard files in `config/grafana/dashboards/`
4. Update alert rules in `config/prometheus/alerts.yml`

## Troubleshooting

### Common Issues

1. **Services not starting**
   - Check Docker is running
   - Verify no port conflicts
   - Check logs: `docker-compose logs`

2. **Data not appearing in Grafana**
   - Verify applications are sending data
   - Check collector logs for errors
   - Confirm Prometheus data source is configured

3. **Authentication issues**
   - Check Grafana credentials
   - Verify environment variables

### Testing

Run the test script to validate the setup:

```bash
python test-instrumentation.py
```

## Development

### Adding New Metrics

1. Add metric definition in `TelemetryUtilities.cs`
2. Update relevant dashboard JSON
3. Add alert rules if needed
4. Test with sample data

### Extending Dashboards

1. Modify dashboard JSON files
2. Add new panels with appropriate PromQL queries
3. Update dashboard provisioning configuration

## Security Notes

- Grafana password should be changed in production
- Use environment variables for sensitive configuration
- Consider network segmentation for production deployment
- Monitor for unusual traffic patterns

## Resources

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)