#!/bin/bash
# Test runner script for Phase 3 integration tests

set -e

echo "=========================================="
echo "Phase 3 Integration Tests"
echo "=========================================="
echo ""

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found"
    echo "Please install .NET 8.0 SDK: https://dotnet.microsoft.com/download"
    exit 1
fi

# Navigate to test directory
cd "$(dirname "$0")"

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build project
echo "Building project..."
dotnet build --no-restore

# Run tests
echo ""
echo "Running integration tests..."
echo ""

# Check if specific scenario is requested
if [ -n "$1" ]; then
    SCENARIO=$1
    echo "Running Scenario: $SCENARIO"
    dotnet test --no-build --verbosity normal --filter "FullyQualifiedName~Scenario${SCENARIO}"
else
    echo "Running all scenarios..."
    dotnet test --no-build --verbosity normal
fi

echo ""
echo "=========================================="
echo "Test Execution Complete"
echo "=========================================="
echo ""
echo "Available scenarios:"
echo "  1 - End-to-End Work Item Processing"
echo "  2 - Test Case Automation"
echo "  3 - Git Integration"
echo "  4 - Offline Synchronization"
echo "  5 - Resilience Testing"
echo ""
echo "To run a specific scenario:"
echo "  ./run-tests.sh 1"
echo ""
