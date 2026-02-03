# Azure DevOps Integration Notes

## Azure Test Plans Capabilities

### Work Item Types
- **Test Plans**: Container for test suites
- **Test Suites**: Groups of test cases (static, requirements-based, query-based)
- **Test Cases**: Individual test scenarios with steps
- **Shared Steps**: Reusable test steps across multiple test cases
- **Shared Parameters**: Reusable test data across test cases

### Requirements Management
- **User Stories**: Track requirements as work items
- **Features**: Group related user stories
- **Epics**: High-level business initiatives
- **Requirements-based test suites**: Automatically link test cases to requirements
- **Traceability**: Built-in linking between requirements, test cases, bugs, and builds

### Test Management Features
- Browser-based test management
- Manual test execution with Test Runner
- Exploratory testing support
- Test configurations (OS, browser, device combinations)
- Test parameters for data-driven testing
- Rich diagnostic data collection (screenshots, logs, recordings)
- Test result tracking and analytics

### API Integration
- Azure DevOps REST API 7.1
- Test Plans API
- Test Suites API
- Test Cases API
- Test Results API
- Work Items API

### Key Integration Points
1. **Requirements**: Work Items API (User Stories, Features, Epics)
2. **Test Cases**: Test Plans API (create, update, query)
3. **Test Execution**: Test Results API (publish results)
4. **Traceability**: Work Item Links API (link requirements to test cases)
5. **Artifacts**: Azure Repos Git API (store generated artifacts)
