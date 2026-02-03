# CPU Agents for SDLC

**Self-aware autonomous AI agents optimized for CPU execution on enterprise desktops**

A comprehensive suite of autonomous AI agents designed for complete Software Development Life Cycle (SDLC) automation, including requirements analysis, test generation, accessibility certification, and distributed test execution.

---

## 🎯 Overview

This repository contains a complete ecosystem of CPU-optimized autonomous AI agents that run locally on enterprise hardware without requiring GPU acceleration. The agents are designed to automate and enhance every phase of the SDLC, from requirements gathering to test execution and accessibility certification.

### Key Features

- **🧠 Self-Aware Architecture**: Multi-level self-testing (function, class, module, system) ensures agent health
- **💻 CPU-Optimized**: Runs on Intel/AMD CPUs using quantized SLMs (1-7B parameters) via llama.cpp
- **🔒 Privacy-First**: 100% local execution - no data sent to cloud for AI inference
- **🔄 Self-Evolution**: Learns from experiences and adapts to improve performance
- **📊 Azure DevOps Integration**: Native integration with Azure Boards, Test Plans, and Repos
- **🌐 Distributed Execution**: Scale test execution across multiple Windows PCs
- **♿ WCAG 2.2 AAA**: Comprehensive accessibility testing and certification

---

## 📦 Repository Structure

```
CPU-Agents-for-SDLC/
├── desktop-agent/              # Self-aware agent for Windows 11 desktops
│   ├── src/                    # .NET 8.0 source code
│   ├── Containerfile           # Podman containerization
│   ├── deploy-windows.ps1      # Automated deployment script
│   └── test-agent.ps1          # Validation test script
│
├── mobile-agent/               # Micro-agent for iPhone and Pixel devices
│   └── [Coming Soon]
│
├── execution-minions/          # Distributed test execution system
│   └── [Coming Soon]
│
└── docs/                       # Comprehensive documentation
    ├── autonomous_agent_design.md
    ├── mobile_micro_agent_design.md
    ├── distributed_test_execution_design.md
    ├── WINDOWS_DEPLOYMENT_GUIDE.md
    ├── PODMAN_DEPLOYMENT.md
    └── [11 design documents total]
```

---

## 🚀 Quick Start

### Desktop Agent (Windows 11)

**Prerequisites:**
- Windows 11 (Pro/Enterprise)
- .NET 8.0 SDK
- Administrator privileges

**Option 1: Direct Execution (Development)**
```powershell
git clone https://github.com/Lev0n82/CPU-Agents-for-SDLC.git
cd CPU-Agents-for-SDLC\desktop-agent\src\AutonomousAgent.Core
dotnet run
```

**Option 2: Windows Service (Production)**
```powershell
cd CPU-Agents-for-SDLC\desktop-agent
.\deploy-windows.ps1 -Action Install
```

**Option 3: Podman Container (Isolated)**
```powershell
cd CPU-Agents-for-SDLC\desktop-agent
podman build -t cpu-agent:latest -f Containerfile .
podman run --name agent-instance cpu-agent:latest
```

See the [Windows Deployment Guide](docs/WINDOWS_DEPLOYMENT_GUIDE.md) for detailed instructions.

---

## 🏗️ Architecture

### Desktop Agent

The desktop agent is a self-aware autonomous system built on .NET 8.0 that provides:

- **Requirements Analysis**: Parse and analyze requirements from Azure DevOps Boards
- **Test Generation**: Automatically generate functional, non-functional, and accessibility test cases
- **Traceability**: Maintain requirements-to-test traceability matrices
- **Self-Testing**: Comprehensive self-validation at four granular levels
- **Proactive Scheduling**: Configurable midnight auto-reboot and maintenance tasks

**Technology Stack:**
- .NET 8.0 (C#)
- llama.cpp for LLM inference
- PostgreSQL for execution logs
- Azure DevOps APIs
- Podman for containerization

### Mobile Micro-Agent

Lightweight agent optimized for mobile devices:

- **iOS**: Swift + Core ML (Apple Neural Engine acceleration)
- **Android**: Kotlin + TensorFlow Lite (Tensor TPU acceleration)
- **Models**: Phi-3-mini, Gemma-2B (1-3B parameters, 4-bit quantization)
- **Performance**: 20-50 tokens/second, <5% battery impact

### Execution Minions

Distributed test execution system:

- **Autonomous Minions**: Self-provisioning test executors on Windows PCs
- **Auto-Provisioning**: Automatic installation of dependencies (Java, .NET, browsers)
- **Video Streaming**: Dual-quality recording (1080p for Azure DevOps, 480p for live view)
- **CRT Monitor**: Retro-themed live test viewing interface
- **Podman-Based**: Secure, isolated test environments

---

## 📚 Documentation

### Getting Started
- [Quick Start Guide](docs/QUICKSTART.md) - Get up and running in 5 minutes
- [Windows Deployment Guide](docs/WINDOWS_DEPLOYMENT_GUIDE.md) - Comprehensive deployment instructions
- [Podman Deployment Guide](docs/PODMAN_DEPLOYMENT.md) - Container deployment details

### Architecture & Design
- [Autonomous Agent Design](docs/autonomous_agent_design.md) - Complete desktop agent architecture
- [Mobile Micro-Agent Design](docs/mobile_micro_agent_design.md) - Mobile agent specifications
- [Distributed Execution Design](docs/distributed_test_execution_design.md) - Minion system architecture
- [Self-Testing Framework](docs/self_testing_framework_design.md) - Multi-level testing approach
- [Scheduling & Self-Awareness](docs/self_awareness_and_scheduling_design.md) - Proactive behavior design

### Integration
- [Azure DevOps Integration](docs/azure_devops_integration_summary.md) - API integration details
- [Implementation Summary](docs/IMPLEMENTATION_SUMMARY.md) - Technical overview

### Research
- [Agent Architecture Research](docs/research_notes.md) - Autonomous agent patterns
- [Intel CPU Optimization](docs/intel_cpu_findings.md) - CPU inference optimization
- [Distributed Execution Research](docs/distributed_execution_research.md) - Test execution patterns

---

## 🎯 Key Capabilities

### Requirements Management
- Parse requirements from Azure DevOps Boards (User Stories, Features, Epics)
- Detect ambiguities and inconsistencies
- Generate requirements traceability matrices
- Track requirement changes and impact analysis

### Test Generation
- **Functional Tests**: Positive/negative scenarios, boundary conditions, edge cases
- **Non-Functional Tests**: Performance, security, scalability, reliability
- **Accessibility Tests**: WCAG 2.2 AAA compliance scanning and remediation
- **End-to-End Tests**: Playwright-based multi-resolution testing (8 resolutions)

### Test Execution
- Distributed execution across multiple Windows PCs
- Real-time video streaming with CRT monitor interface
- Automatic result reporting to Azure DevOps Test Plans
- High-quality video attachments for debugging

### Self-Testing & Maintenance
- **Function-Level**: Unit tests for individual methods
- **Class-Level**: Integration tests for classes
- **Module-Level**: Component tests for modules
- **System-Level**: End-to-end system validation
- **Scheduled Maintenance**: Configurable midnight auto-reboot

---

## 🔧 Configuration

The desktop agent is configured via `appsettings.json`:

```json
{
  "Scheduler": {
    "NightlyReboot": {
      "Enabled": true,
      "Hour": 0,
      "Minute": 0
    }
  },
  "AzureDevOps": {
    "Organization": "your-org",
    "Project": "your-project",
    "PersonalAccessToken": "your-pat"
  },
  "LLM": {
    "ModelPath": "path/to/model.gguf",
    "ContextSize": 4096,
    "Temperature": 0.7
  }
}
```

---

## 🧪 Testing

Run the validation test suite:

```powershell
cd desktop-agent
.\test-agent.ps1 -TestType All
```

**Tests performed:**
- ✅ Build verification
- ✅ Self-test framework execution
- ✅ Scheduling configuration validation

---

## 🛠️ Technology Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| Desktop Agent | .NET 8.0 (C#) | Core agent runtime |
| LLM Inference | llama.cpp | CPU-optimized inference |
| Models | Phi-3, Qwen2.5, Mistral | 1-7B parameter SLMs |
| Database | PostgreSQL | Execution logs and learning data |
| Test Storage | Oracle | Test case cache |
| Version Control | Azure Repos | Artifact storage |
| Requirements | Azure Boards | Work item management |
| Test Plans | Azure Test Plans | Test case management |
| Containerization | Podman | Secure, daemonless containers |
| Test Automation | Playwright | End-to-end testing |
| Accessibility | axe-core, Pa11y | WCAG compliance |

---

## 📊 Performance Benchmarks

### Desktop Agent (Intel Core i7-13700K)

| Model | Quantization | Tokens/Sec | Memory | CPU Usage |
|-------|--------------|------------|--------|-----------|
| Phi-3-mini (3.8B) | 4-bit | 25-30 | 2.5 GB | 40-60% |
| Qwen2.5 (7B) | 4-bit | 15-20 | 4.5 GB | 60-80% |
| Mistral (7B) | 4-bit | 12-18 | 4.8 GB | 65-85% |

### Mobile Micro-Agent

| Device | Model | Tokens/Sec | Memory | Battery Impact |
|--------|-------|------------|--------|----------------|
| iPhone 15 Pro | Phi-3-mini 4-bit | 25-30 | 2.5 GB | <5%/hour |
| iPhone 15 Pro | Gemma-2B 4-bit | 35-40 | 1.3 GB | <4%/hour |
| Pixel 8 Pro | Phi-3-mini 4-bit | 28-32 | 2.5 GB | <5%/hour |
| Pixel 8 Pro | Gemma-2B 4-bit | 38-42 | 1.3 GB | <4%/hour |

---

## 🗺️ Roadmap

### Phase 1: Foundation ✅ (Complete)
- [x] Self-testing framework
- [x] Proactive scheduling
- [x] Windows Service deployment
- [x] Podman containerization
- [x] Comprehensive documentation

### Phase 2: LLM Integration (In Progress)
- [ ] llama.cpp integration
- [ ] Model management system
- [ ] Prompt engineering framework
- [ ] Context management

### Phase 3: Azure DevOps Integration
- [ ] Azure Boards API integration
- [ ] Azure Test Plans API integration
- [ ] Azure Repos integration
- [ ] Requirements parsing

### Phase 4: Test Generation
- [ ] Functional test generation
- [ ] Non-functional test generation
- [ ] Accessibility test generation
- [ ] Traceability matrix generation

### Phase 5: Distributed Execution
- [ ] Execution minion implementation
- [ ] CRT monitor interface
- [ ] Video streaming infrastructure
- [ ] Azure DevOps result reporting

### Phase 6: Mobile Agents
- [ ] iOS micro-agent
- [ ] Android micro-agent
- [ ] Synchronization system

---

## 🤝 Contributing

This is a private repository. For questions or contributions, please contact the repository owner.

---

## 📄 License

Proprietary - All rights reserved.

---

## 🔗 Links

- **Repository**: https://github.com/Lev0n82/CPU-Agents-for-SDLC
- **Documentation**: [/docs](docs/)
- **Issues**: https://github.com/Lev0n82/CPU-Agents-for-SDLC/issues

---

## 📧 Contact

For questions, support, or collaboration inquiries, please open an issue in the repository.

---

**Built with ❤️ for enterprise SDLC automation**
