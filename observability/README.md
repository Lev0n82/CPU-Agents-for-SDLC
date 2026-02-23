# OpenTelemetry Observability Stack

Complete observability stack for CPU Agents using OpenTelemetry, Jaeger, Prometheus, and Grafana.

## Components

### Jaeger (Distributed Tracing)
- **Purpose**: Visualize distributed traces across agent workflows
- **UI**: http://localhost:16686
- **OTLP gRPC**: localhost:4317
- **OTLP HTTP**: localhost:4318

### Prometheus (Metrics)
- **Purpose**: Collect and store time-series metrics
- **UI**: http://localhost:9090
- **Metrics Endpoint**: /metrics

### Grafana (Visualization)
- **Purpose**: Create dashboards and alerts
- **UI**: http://localhost:3000
- **Default Credentials**: admin/admin

## Quick Start

### Prerequisites
- Podman installed and running
- Ports 3000, 4317, 4318, 9090, 16686 available

### Deployment

```bash
cd observability
chmod +x deploy-otel-stack.sh
./deploy-otel-stack.sh
```

### Verify Deployment

```bash
podman ps | grep -E "(jaeger|prometheus|grafana)"
```

You should see three running containers.

## Configuration

### Application Configuration

Add to your `appsettings.json`:

```json
{
  "OpenTelemetry": {
    "ServiceName": "cpu-agents",
    "ServiceVersion": "3.0.0",
    "OtlpEndpoint": "http://localhost:4318",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true
  }
}
```

### Environment Variables

```bash
export OTEL_SERVICE_NAME="cpu-agents"
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318"
export OTEL_TRACES_EXPORTER="otlp"
export OTEL_METRICS_EXPORTER="otlp"
export OTEL_LOGS_EXPORTER="otlp"
```

## Accessing the Stack

### Jaeger UI
1. Open http://localhost:16686
2. Select "cpu-agents" from the Service dropdown
3. Click "Find Traces" to view distributed traces

### Prometheus UI
1. Open http://localhost:9090
2. Go to Status → Targets to verify scraping
3. Use PromQL to query metrics

### Grafana UI
1. Open http://localhost:3000
2. Login with admin/admin
3. Add Prometheus data source:
   - URL: http://prometheus:9090
   - Access: Server (default)
4. Add Jaeger data source:
   - URL: http://jaeger:16686
   - Access: Server (default)

## Key Metrics

### Agent Performance
- `agent_workflow_duration_seconds` - Workflow execution time
- `agent_workflow_success_total` - Successful workflow count
- `agent_workflow_failure_total` - Failed workflow count

### AI Decision Module
- `ai_decision_duration_seconds` - AI inference time
- `ai_decision_token_count` - Tokens processed
- `ai_decision_cache_hit_total` - Cache hit rate

### Work Items
- `workitem_processing_duration_seconds` - Work item processing time
- `workitem_queue_size` - Current queue depth
- `workitem_claim_duration_seconds` - Time to claim work item

### System Health
- `process_cpu_usage` - CPU utilization
- `process_memory_usage_bytes` - Memory consumption
- `dotnet_gc_collection_count` - GC collections

## Grafana Dashboards

### Creating Custom Dashboard

1. Login to Grafana
2. Click "+" → "Dashboard"
3. Add Panel
4. Select Prometheus data source
5. Enter PromQL query
6. Configure visualization

### Example PromQL Queries

**Workflow Success Rate**:
```promql
rate(agent_workflow_success_total[5m]) / 
(rate(agent_workflow_success_total[5m]) + rate(agent_workflow_failure_total[5m]))
```

**Average Workflow Duration**:
```promql
rate(agent_workflow_duration_seconds_sum[5m]) / 
rate(agent_workflow_duration_seconds_count[5m])
```

**Work Item Queue Depth**:
```promql
workitem_queue_size
```

## Troubleshooting

### Containers Not Starting

```bash
# Check Podman status
podman ps -a

# View container logs
podman logs jaeger
podman logs prometheus
podman logs grafana

# Restart containers
podman restart jaeger prometheus grafana
```

### No Data in Jaeger

1. Verify OTLP endpoint configuration
2. Check application logs for telemetry export errors
3. Verify network connectivity: `curl http://localhost:4318/v1/traces`

### Prometheus Not Scraping

1. Check prometheus.yml configuration
2. Verify target URL is accessible
3. Check Prometheus logs: `podman logs prometheus`

### Grafana Data Source Issues

1. Verify data source URL uses container name (e.g., `http://prometheus:9090`)
2. Check network connectivity between containers
3. Test connection in Grafana data source settings

## Cleanup

### Stop Stack

```bash
podman stop jaeger prometheus grafana
```

### Remove Stack

```bash
podman rm jaeger prometheus grafana
podman network rm otel-network
```

### Complete Cleanup

```bash
podman stop jaeger prometheus grafana
podman rm jaeger prometheus grafana
podman network rm otel-network
podman volume prune -f
```

## Production Considerations

### Persistence

Add volumes for data persistence:

```bash
# Prometheus data
podman run -d \
  --name prometheus \
  -v prometheus-data:/prometheus:Z \
  prom/prometheus:latest

# Grafana data
podman run -d \
  --name grafana \
  -v grafana-data:/var/lib/grafana:Z \
  grafana/grafana:latest
```

### Security

1. Change default Grafana password
2. Enable authentication for Prometheus
3. Use TLS for OTLP endpoints
4. Restrict network access with firewall rules

### Scalability

For production deployments:
- Use Jaeger with Elasticsearch backend
- Deploy Prometheus with remote storage
- Use Grafana with external database
- Implement high availability with multiple replicas

## References

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
