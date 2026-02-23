#!/bin/bash
# OpenTelemetry Observability Stack Deployment Script
# Deploys Jaeger, Prometheus, and Grafana using Podman

set -e

echo "🚀 Deploying OpenTelemetry Observability Stack with Podman..."

# Create Podman network for observability
echo "📡 Creating Podman network..."
podman network create otel-network 2>/dev/null || echo "Network already exists"

# Deploy Jaeger (Distributed Tracing)
echo "🔍 Deploying Jaeger for distributed tracing..."
podman run -d \
  --name jaeger \
  --network otel-network \
  -p 16686:16686 \
  -p 4317:4317 \
  -p 4318:4318 \
  jaegertracing/all-in-one:latest

# Deploy Prometheus (Metrics Collection)
echo "📊 Deploying Prometheus for metrics..."
podman run -d \
  --name prometheus \
  --network otel-network \
  -p 9090:9090 \
  -v $(pwd)/prometheus.yml:/etc/prometheus/prometheus.yml:Z \
  prom/prometheus:latest

# Deploy Grafana (Visualization)
echo "📈 Deploying Grafana for visualization..."
podman run -d \
  --name grafana \
  --network otel-network \
  -p 3000:3000 \
  -e GF_SECURITY_ADMIN_PASSWORD=admin \
  grafana/grafana:latest

echo "✅ OpenTelemetry stack deployed successfully!"
echo ""
echo "📍 Access URLs:"
echo "  Jaeger UI:     http://localhost:16686"
echo "  Prometheus UI: http://localhost:9090"
echo "  Grafana UI:    http://localhost:3000 (admin/admin)"
echo ""
echo "🔧 OTLP Endpoints:"
echo "  gRPC:  localhost:4317"
echo "  HTTP:  localhost:4318"
echo ""
echo "💡 Configure your application to send telemetry to:"
echo "   OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318"
