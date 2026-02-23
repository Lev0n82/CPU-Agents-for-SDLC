#!/bin/bash
# Setup script for Azure DevOps integration testing

set -e

echo "=========================================="
echo "Azure DevOps Integration Test Setup"
echo "=========================================="
echo ""

# Configuration
ORGANIZATION_URL="https://dev.azure.com/LevonMinasyan2"
PROJECT_NAME="CPU-Agents-Test"
PAT="${AZURE_DEVOPS_PAT}"

if [ -z "$PAT" ]; then
    echo "Error: AZURE_DEVOPS_PAT environment variable not set"
    echo "Please set your Personal Access Token:"
    echo "  export AZURE_DEVOPS_PAT='your-pat-here'"
    exit 1
fi

echo "Organization: $ORGANIZATION_URL"
echo "Project: $PROJECT_NAME"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "Azure CLI not found. Installing..."
    curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
fi

# Install Azure DevOps extension
echo "Installing Azure DevOps CLI extension..."
az extension add --name azure-devops --yes 2>/dev/null || az extension update --name azure-devops

# Configure Azure DevOps CLI
echo "Configuring Azure DevOps CLI..."
export AZURE_DEVOPS_EXT_PAT=$PAT
az devops configure --defaults organization=$ORGANIZATION_URL project=$PROJECT_NAME

# Check if project exists
echo "Checking if project exists..."
PROJECT_EXISTS=$(az devops project show --project "$PROJECT_NAME" --query "name" -o tsv 2>/dev/null || echo "")

if [ -z "$PROJECT_EXISTS" ]; then
    echo "Project '$PROJECT_NAME' not found. Creating..."
    az devops project create --name "$PROJECT_NAME" \
        --description "CPU Agents Integration Test Project" \
        --source-control git \
        --visibility private
    echo "✓ Project created successfully"
else
    echo "✓ Project '$PROJECT_NAME' already exists"
fi

# Create test work items
echo ""
echo "Creating test work items..."

# Create a User Story
STORY_ID=$(az boards work-item create \
    --title "[IntegrationTest] Sample User Story" \
    --type "User Story" \
    --description "Sample user story for integration testing" \
    --query "id" -o tsv)

echo "✓ Created User Story: $STORY_ID"

# Create a Task
TASK_ID=$(az boards work-item create \
    --title "[IntegrationTest] Sample Task" \
    --type "Task" \
    --description "Sample task for integration testing" \
    --query "id" -o tsv)

echo "✓ Created Task: $TASK_ID"

# Create a Bug
BUG_ID=$(az boards work-item create \
    --title "[IntegrationTest] Sample Bug" \
    --type "Bug" \
    --description "Sample bug for integration testing" \
    --query "id" -o tsv)

echo "✓ Created Bug: $BUG_ID"

echo ""
echo "=========================================="
echo "Setup Complete!"
echo "=========================================="
echo ""
echo "Test work items created:"
echo "  - User Story: $STORY_ID"
echo "  - Task: $TASK_ID"
echo "  - Bug: $BUG_ID"
echo ""
echo "You can now run integration tests:"
echo "  cd tests/Phase3.IntegrationTests"
echo "  dotnet test"
echo ""
