# Grafana Configuration Guide

This document explains the Grafana dashboards and configurations for monitoring CPU Agents for SDLC applications.

## Dashboard Overview

### 1. OpenTelemetry Overview Dashboard
- **Purpose**: Monitor the OpenTelemetry collector and overall system health
- **Key Metrics**:
  - Service health status (up/down)
  - Collector uptime
  - Memory and CPU usage
  - Metrics collection rate
  - Span processing rate
  - Error rate and active alerts
- **Features**: 
  - Real-time health monitoring
  - Threshold-based coloring
  - Alert indicators
  - Performance trends

### 2. Autonomous Agent Dashboard
- **Purpose**: Monitor autonomous agent performance and resource usage
- **Key Metrics**:
  - Agent status (up/down)
  - Task execution rate
  - Self-test completion rate
  - LLM request rate
  - Scheduling iterations
  - CPU and memory utilization
  - Task execution time percentiles (P50, P95)
  - LLM response time percentiles
- **Features**:
  - Real-time thresholds
  - Performance trends
  - Resource utilization gauges
  - Percentile analysis

### 3. Azure DevOps Agent Dashboard
- **Purpose**: Monitor Azure DevOps agent performance and API interactions
- **Key Metrics**:
  - Agent status
  - Work item processing rate
  - Authentication attempts
  - API call rate
  - Active work items
  - Authentication success rate (%)
  - Memory usage
  - API response time percentiles
  - Processing trends
- **Features**:
  - Success rate gauges
  - Processing trend analysis
  - API performance monitoring
  - Authentication metrics

## Metric Details

### Autonomous Agent Metrics
- `up{job="autonomous-agent"}` - Service availability
- `tasks_executed_total` - Total tasks executed
- `selftests_completed_total` - Self-tests completed
- `scheduling_iterations_total` - Scheduling iterations
- `llm_requests_total` - LLM API requests
- `cpu_usage` - Percentage CPU usage
- `memory_usage` - Memory usage in MB
- `task_execution_time_bucket` - Task execution time histogram
- `llm_response_time_bucket` - LLM response time histogram

### Azure DevOps Agent Metrics
- `up{job="azure-devops-agent"}` - Service availability
- `workitems_processed_total` - Work items processed
- `authentication_attempts_total` - Authentication attempts
- `api_calls_total` - API calls made
- `workitems_active` - Currently active work items
- `memory_usage` - Memory usage in MB
- `api_response_time_bucket` - API response time histogram

## Alert Configuration

### Alert Rules
All alerts are configured in `config/prometheus/alerts.yml` and include:
- High memory usage alerts
- High CPU usage alerts
- Low processing rate alerts
- Service downtime alerts
- Authentication failure alerts

### Alert Annotations
Prometheus alerts automatically appear as annotations on Grafana dashboards with:
- Red annotations for active alerts
- Tags for filtering
- Configurable appearance

## Data Source Configuration

### Prometheus Data Source
- **URL**: `http://prometheus:9090`
- **Type**: Proxy
- **Default**: Yes

### Annotations
- **Source**: Prometheus alerts
- **Query**: `ALERTS{alertstate="firing"}`
- **Color**: Red for active alerts

## Dashboard Features

### Time Selection
- Default: Last 6 hours
- Refresh intervals: 5s, 10s, 30s, 1m, 5m, 15m, 30m, 1h, 2h, 1d
- Custom ranges supported

### Panel Types
- **Stat Panels**: Single value displays with threshold coloring
- **Graph Panels**: Time-series data with multiple metrics
- **Gauge Panels**: Circular gauges with configurable ranges
- **Heatmaps**: Not yet configured

### Threshold Configuration
All dashboards use configurable thresholds:
- **Green**: Normal operation
- **Yellow**: Warning level
- **Red**: Critical level

## Setup Instructions

### 1. Start the Stack
```bash
cd otel-stack
./deploy-stack.ps1
```

### 2. Access Grafana
- **URL**: http://localhost:13000
- **Username**: admin
- **Password**: admin123 (or set via GRAFANA_PASSWORD environment variable)

### 3. Dashboards
All dashboards are automatically provisioned:
- OpenTelemetry Overview
- Autonomous Agent Dashboard
- Azure DevOps Agent Dashboard

### 4. Data Verification
Use the test script to generate sample data:
```bash
python test-instrumentation.py
```

## Troubleshooting

### Common Issues

#### No Data Showing
- Check if Prometheus is scraping the applications
- Verify applications are sending metrics to the collector
- Check collector logs for errors

#### Dashboards Not Loading
- Verify Prometheus data source is correctly configured
- Check dashboard JSON syntax
- Ensure Grafana restarted after configuration changes

#### Missing Annotations
- Verify alertmanager is running
- Check alert rules are firing
- Confirm annotation configuration

### Debug Commands
```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs -f grafana
docker-compose logs -f prometheus

# Check Prometheus targets
curl http://localhost:19090/api/v1/targets
```

## Customization

### Adding New Metrics
1. Add metric instrumentation to application code
2. Include metric in Prometheus scrape configuration
3. Create or update dashboard panels
4. Add alert rules if needed

### Dashboard Creation
1. Use JSON dashboard format
2. Follow existing patterns for consistency
3. Include threshold configuration
4. Test with real data

### Alert Configuration
1. Define alert rules in `config/prometheus/alerts.yml`
2. Include appropriate labels and annotations
3. Test alert triggers
4. Configure notification channels

## Performance Optimization

### Query Optimization
- Use appropriate time ranges
- Leverage aggregation when possible
- Avoid expensive regex patterns
- Use recording rules for complex queries

### Dashboard Optimization
- Limit number of panels per dashboard
- Use appropriate refresh intervals
- Consider using Grafana explorer for ad-hoc queries
- Enable caching where appropriate

## Security Considerations

### Access Control
- Change default credentials
- Configure authentication providers
- Set up proper user permissions
- Enable HTTPS in production

### Data Protection
- Monitor for PII in metrics
- Set up data retention policies
- Configure secure data transmission
- Regular security audits

This configuration provides comprehensive monitoring for CPU Agents for SDLC applications with production-ready dashboards and alerting capability.