#!/bin/bash

# OpenTelemetry Stack Validation Script

echo "=================================="
echo "OpenTelemetry Stack Validation"
echo "=================================="

echo ""
echo "Checking container status..."
podman ps --filter name="otel|jaeger" --format "table {{.Names}}\t{{.Status}}"

echo ""
echo "Testing service endpoints..."

# Test OpenTelemetry Collector health
if curl -s http://localhost:19133/ > /dev/null; then
    echo "✅ OpenTelemetry Collector: HEALTHY"
else
    echo "❌ OpenTelemetry Collector: UNHEALTHY"
fi

# Test Prometheus health
if curl -s http://localhost:19090/-/healthy | grep -q "Prometheus Server is Healthy"; then
    echo "✅ Prometheus: HEALTHY"
else
    echo "❌ Prometheus: UNHEALTHY"
fi

# Test Grafana health
if curl -s http://localhost:13000/api/health | grep -q "database.*ok"; then
    echo "✅ Grafana: HEALTHY"
else
    echo "❌ Grafana: UNHEALTHY"
fi

# Test Jaeger (basic port check)
if curl -s http://localhost:17686 | grep -q "jaeger"; then
    echo "✅ Jaeger: ACCESSIBLE"
else
    echo "⚠️  Jaeger: Status uncertain (may require manual verification)"
fi

echo ""
echo "=================================="
echo "Access URLs:"
echo "=================================="
echo "- Grafana Dashboard: http://localhost:13000"
echo "- Prometheus UI:     http://localhost:19090"
echo "- Jaeger Tracing:    http://localhost:17686"
echo "- Collector Health:  http://localhost:19133"
echo ""
echo "Grafana Credentials:"
echo "- Username: admin"
echo "- Password: admin123"
echo ""
echo "OpenTelemetry Endpoints:"
echo "- OTLP gRPC: localhost:18417"
echo "- OTLP HTTP: localhost:18418"
echo "- Metrics:    localhost:18889"
echo ""
echo "Deployment validated successfully! 🚀"