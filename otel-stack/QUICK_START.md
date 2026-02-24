# Quick Start Guide

## Get Started in 3 Steps

### 1. Deploy the Stack
```powershell
# Start Docker Desktop first
cd otel-stack
.\deploy-stack.ps1
```

This will start:
- OpenTelemetry Collector
- Jaeger (distributed tracing)
- Prometheus (metrics storage)
- Grafana (visualization)

### 2. Test the Setup
```bash
# Generate sample telemetry data
python test-instrumentation.py
```

### 3. Access Dashboards

| Service | URL | Credentials |
|---------|-----|-------------|
| **Grafana** | http://localhost:13000 | admin / admin123 |
| **Jaeger** | http://localhost:17686 | No authentication |
| **Prometheus** | http://localhost:19090 | No authentication |

## Dashboard Quick Reference

### Autonomous Agent Dashboard
- ✅ Agent status
- 📊 Task execution rate
- 🧪 Self-test completion
- 🤖 LLM request metrics
- ⚡ CPU/Memory usage
- ⏱️ Performance percentiles

### Azure DevOps Dashboard
- ✅ Agent status
- 📋 Work item processing
- 🔐 Authentication metrics
- 🌐 API call statistics
- ⚡ Resource utilization
- 📈 Performance trends

### OpenTelemetry Overview
- 🏥 System health
- 📊 Collection rates
- 🔥 Alert status
- 📈 Error rates
- ⏱️ Uptime tracking

## Useful Commands

```bash
# Stop the stack
docker-compose down

# Restart services
docker-compose restart

# View logs
docker-compose logs -f

# Check service status
docker-compose ps
```

## Next Steps

1. **Instrument your applications**: Add OpenTelemetry SDK to your code
2. **Customize dashboards**: Modify JSON files in `config/grafana/dashboards/`
3. **Configure alerts**: Edit `config/prometheus/alerts.yml`
4. **Scale for production**: Update configurations for production deployment

## Troubleshooting

**Docker not starting?**
- Ensure Docker Desktop is running
- Check for port conflicts

**No data in Grafana?**
- Verify applications are sending metrics
- Check Prometheus targets: http://localhost:19090/targets

**Dashboards not loading?** 
- Try refreshing browser cache
- Check Grafana logs: `docker-compose logs -f grafana`

---

**Need help?** Check the full documentation in [README.md](./README.md) or [GRAFANA_CONFIGURATION.md](./GRAFANA_CONFIGURATION.md)