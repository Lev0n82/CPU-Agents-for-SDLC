#!/usr/bin/env python3
"""
OpenTelemetry Instrumentation Test Script
This script tests the OpenTelemetry instrumentation setup by sending sample metrics and traces.
"""

import requests
import time
import random
from opentelemetry import metrics, trace
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor

# Configure OpenTelemetry
endpoint = "http://localhost:18418"

# Set up metrics
metric_reader = PeriodicExportingMetricReader(
    exporter=OTLPMetricExporter(endpoint=endpoint + "/v1/metrics"),
    export_interval_millis=5000
)

meter_provider = MeterProvider(metric_readers=[metric_reader])
metrics.set_meter_provider(meter_provider)

# Set up tracing
span_processor = BatchSpanProcessor(OTLPSpanExporter(endpoint=endpoint + "/v1/traces"))
tracer_provider = TracerProvider()
tracer_provider.add_span_processor(span_processor)
trace.set_tracer_provider(tracer_provider)

# Get meter and tracer
meter = metrics.get_meter(__name__)
tracer = trace.get_tracer(__name__)

# Create metrics
test_counter = meter.create_counter("test.counter", "requests", "Test requests counter")
response_time_histogram = meter.create_histogram("test.response_time", "ms", "Response time histogram")

def test_instrumentation():
    """Test the instrumentation by generating sample data"""
    print("Testing OpenTelemetry instrumentation...")
    
    # Generate sample traces and metrics
    for i in range(10):
        with tracer.start_as_current_span("test.transaction") as span:
            # Add span attributes
            span.set_attribute("transaction.id", f"test-{i}")
            span.set_attribute("service.name", "test-service")
            
            # Simulate work
            time.sleep(random.uniform(0.1, 1.0))
            
            # Record metrics
            test_counter.add(1, {"operation": "test", "iteration": str(i)})
            response_time = random.uniform(50, 500)
            response_time_histogram.record(response_time, {"operation": "test"})
            
            print(f"Sent trace {i+1}/10 (response time: {response_time:.2f}ms)")
    
    print("Instrumentation test completed successfully")

def check_collector_status():
    """Check if the collector is reachable"""
    try:
        response = requests.get("http://localhost:19133/", timeout=5)
        if response.status_code == 200:
            print("✓ OpenTelemetry Collector is reachable")
            return True
    except Exception as e:
        print(f"✗ OpenTelemetry Collector is not reachable: {e}")
        return False

def check_service_endpoints():
    """Check if all services are accessible"""
    services = [
        ("Jaeger", "http://localhost:17686"),
        ("Prometheus", "http://localhost:19090"),
        ("Grafana", "http://localhost:13000")
    ]
    
    all_healthy = True
    for name, url in services:
        try:
            response = requests.get(url, timeout=5)
            if response.status_code == 200:
                print(f"✓ {name} is accessible")
            else:
                print(f"⚠ {name} returned status {response.status_code}")
                all_healthy = False
        except Exception as e:
            print(f"✗ {name} is not accessible: {e}")
            all_healthy = False
    
    return all_healthy

if __name__ == "__main__":
    print("OpenTelemetry Instrumentation Test")
    print("================================")
    
    # Check if services are running
    print("\nChecking service status...")
    if not check_collector_status():
        print("Please start the OpenTelemetry stack first (docker-compose up -d)")
        exit(1)
    
    all_healthy = check_service_endpoints()
    
    if all_healthy:
        print("\nAll services are accessible. Running instrumentation test...")
        test_instrumentation()
    else:
        print("\nSome services are not accessible. Please check the stack deployment.")
    
    # Shut down the meter provider
    meter_provider.shutdown()
    tracer_provider.shutdown()