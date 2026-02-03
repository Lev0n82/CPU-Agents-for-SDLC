# Distributed Test Execution System with Execution Minions and CRT Live View

## Executive Summary

This document outlines a sophisticated, distributed test execution architecture designed to enhance enterprise quality assurance workflows. The system leverages a network of autonomous "Execution Minions" running on standard Windows PCs to create a scalable and resilient test farm. Each minion is capable of self-provisioning required software dependencies, executing a wide range of automated tests, and providing rich feedback, including high-quality video recordings and real-time, low-latency screen streaming.

A key innovation of this architecture is the **CRT Monitor Channel**, a retro-themed web interface that allows stakeholders to view live test executions in a low-resolution (480p) stream, reminiscent of channel surfing on an old television. This provides immediate, at-a-glance insight into ongoing tests without consuming significant bandwidth. For detailed analysis, high-quality video recordings of the full execution are automatically attached to the corresponding test run in Azure DevOps.

The system is built on a robust Hub-Minion model, where a central Hub orchestrates test distribution and result collection. Minions are lightweight, autonomous agents that handle the entire lifecycle of a test job on their host machine, from environment setup to result reporting. This architecture dramatically reduces test execution time through parallelization, improves test reliability with isolated environments, and provides unparalleled visibility into the testing process.

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Architecture](#2-system-architecture)
3. [Execution Minion Architecture](#3-execution-minion-architecture)
4. [Auto-Provisioning and Dependency Management](#4-auto-provisioning-and-dependency-management)
5. [Test Execution Workflow](#5-test-execution-workflow)
6. [Video Streaming and CRT Monitor Interface](#6-video-streaming-and-crt-monitor-interface)
7. [Azure DevOps Integration](#7-azure-devops-integration)
8. [Implementation Specifications](#8-implementation-specifications)
9. [Security Considerations](#9-security-considerations)
10. [Deployment Strategy](#10-deployment-strategy)
11. [Acceptance Criteria](#11-acceptance-criteria)
12. [References](#12-references)

---

## 1. Introduction

### 1.1 Background

As software complexity grows, the need for rapid, reliable, and scalable automated testing has become paramount. Centralized testing infrastructure often becomes a bottleneck, leading to long queue times and delayed feedback for development teams. Furthermore, managing diverse test environments with specific software dependencies (browsers, runtimes like Java, .NET, Python) across multiple machines is a significant operational burden.

This architecture addresses these challenges by distributing the test execution workload across a farm of autonomous agents, or "Minions." These minions transform any standard Windows PC into a self-sufficient testing node, capable of preparing its own environment and executing tests on demand. The addition of real-time video streaming provides unprecedented visibility, while the nostalgic CRT monitor interface offers a unique and engaging user experience.

### 1.2 Design Goals

- **Scalability:** Easily scale test execution capacity by adding more Windows PCs to the minion network.
- **Autonomy:** Minions must operate autonomously, handling environment setup, test execution, and result reporting with minimal human intervention.
- **Auto-Provisioning:** Minions must automatically install required software dependencies for each test job, ensuring a correct and consistent environment.
- **Real-Time Visibility:** Provide a low-latency, low-resolution live stream of test executions for real-time monitoring.
- **Comprehensive Reporting:** Attach high-quality video recordings and detailed logs to Azure DevOps test runs for thorough analysis.
- **Resilience:** The system must be resilient to minion failures, automatically re-queuing tests if a minion goes offline.
- **Ease of Use:** The CRT monitor interface should be intuitive and engaging for stakeholders to observe testing activities.

---

## 2. System Architecture

The system is based on a classic Hub-Minion (or Master-Slave) architecture, which is proven for distributed task management and is used by systems like Selenium Grid [1].

### 2.1 High-Level Diagram

```
┌──────────────────┐       ┌──────────────────────────┐       ┌──────────────────┐
│   Azure DevOps   │◄─────►│        Test Hub        │◄─────►│   CRT Monitor    │
│ (Test Plans)     │       │ (Orchestrator & API)     │       │ (Web Interface)  │
└──────────────────┘       └────────────┬───────────┘       └──────────────────┘
                                        │
                                        │ (Distributes Jobs)
                                        │
         ┌──────────────────────────────┼──────────────────────────────┐
         │                              │                              │
         ▼                              ▼                              ▼
┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│ Execution Minion │       │ Execution Minion │       │ Execution Minion │
│  (Windows PC 1)  │       │  (Windows PC 2)  │       │  (Windows PC n)  │
└──────────────────┘       └──────────────────┘       └──────────────────┘
```

### 2.2 Components

- **Test Hub (Orchestrator):**
  - A central web service (e.g., ASP.NET Core) that manages the entire system.
  - **API Endpoint:** Exposes a REST API for minions to register, request jobs, and report status.
  - **Job Queue:** Pulls test runs from Azure DevOps Test Plans and places them in a queue.
  - **Minion Registry:** Maintains a list of available minions, their capabilities (e.g., installed software), and their current status (idle, busy).
  - **Dispatcher:** Assigns jobs from the queue to idle minions that meet the job's requirements.

- **Execution Minion:**
  - A lightweight agent running as a background service on each Windows PC in the test farm.
  - Polls the Hub for jobs.
  - Manages the entire test lifecycle on the host machine.

- **Azure DevOps:**
  - The source of truth for test plans and test cases.
  - The final destination for test results, logs, and high-quality video recordings.

- **CRT Monitor Interface:**
  - A web-based front-end (e.g., React, Vue.js) that communicates with the Hub.
  - Displays a grid of "channels," each corresponding to an active minion.
  - Streams the low-resolution video feed from the minions for live viewing.

---

## 3. Execution Minion Architecture

The Execution Minion is the core of the distributed system. It is designed to be autonomous, resilient, and efficient.

### 3.1 Minion Internal Components

```
┌──────────────────────────────────────────────────┐
│                 Execution Minion                 │
│ ┌──────────────────────────────────────────────┐ │
│ │                Heartbeat & Polling             │ │
│ └──────────────────────────────────────────────┘ │
│ ┌──────────────────────────────────────────────┐ │
│ │                  Task Runner                   │ │
│ │ ┌────────────────┐  ┌───────────────────────┐  │ │
│ │ │ Provisioning   │  │   Test Executor       │  │ │
│ │ │ Engine         │  │ (Playwright, etc.)    │  │ │
│ │ └────────────────┘  └───────────────────────┘  │ │
│ │ ┌────────────────┐  ┌───────────────────────┐  │ │
│ │ │ Screen Recorder│  │   Log & Artifact      │  │ │
│ │ │ (FFmpeg)       │  │   Collector           │  │ │
│ │ └────────────────┘  └───────────────────────┘  │ │
│ └──────────────────────────────────────────────┘ │
│ ┌──────────────────────────────────────────────┐ │
│ │                 Status Reporter                │ │
│ └──────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────┘
```

### 3.2 Component Descriptions

- **Heartbeat & Polling Service:**
  - Runs on a regular interval (e.g., every 15 seconds).
  - Sends a heartbeat to the Hub's API to signal that it is alive and idle.
  - In the heartbeat request, it includes its current capabilities (e.g., `"java": "11.0.2"`, `"chrome": "120.0"`).
  - If the Hub has a matching job, the heartbeat response includes the job details.

- **Task Runner:**
  - The main engine that executes a received job. It orchestrates the other internal components in a sequence.

- **Provisioning Engine:**
  - **Responsibility:** To ensure all software dependencies required by the test job are installed on the host machine.
  - This is a critical component for the minion's autonomy. See Section 4 for a detailed design.

- **Test Executor:**
  - Invokes the test automation framework specified in the job (e.g., Playwright, Selenium, Robot Framework).
  - It runs the test command-line interface with the specified arguments.

- **Screen Recorder:**
  - Uses **FFmpeg** to capture the desktop screen during the test execution [2].
  - It performs two simultaneous operations:
    1. **High-Quality Recording:** Records a high-resolution (e.g., 1080p, 30fps) video and saves it locally to a file (e.g., `execution.mp4`).
    2. **Low-Quality Live Stream:** Encodes a separate low-resolution (480p, 15fps) stream and pushes it to a real-time streaming endpoint (e.g., an RTMP server) specified by the Hub.

- **Log & Artifact Collector:**
  - Gathers all outputs from the test execution, including:
    - Test framework logs (e.g., JUnit XML, Allure results).
    - The high-quality `execution.mp4` video file.
    - Screenshots taken during the test.
    - Application log files.
  - It packages these artifacts into a single compressed file (e.g., `results.zip`).

- **Status Reporter:**
  - After the job is complete (or has failed), it sends the final status to the Hub.
  - It uploads the `results.zip` package to a storage location (e.g., Azure Blob Storage) and provides the URL to the Hub.

### 3.3 State Management

The minion maintains a simple state machine:
- **IDLE:** Waiting for a job. Polling the Hub.
- **PROVISIONING:** Installing required software dependencies.
- **EXECUTING:** Running the test and streaming video.
- **COLLECTING:** Gathering logs and artifacts after execution.
- **REPORTING:** Uploading results and reporting completion to the Hub.
- **ERROR:** An unrecoverable error occurred. Reports error to Hub and returns to IDLE after a cooldown period.

---

## 4. Auto-Provisioning and Dependency Management

The ability for a minion to provision its own environment is central to the architecture's autonomy and scalability.

### 4.1 Dependency Declaration

Test jobs dispatched by the Hub will include a `dependencies` section, for example:

```json
{
  "jobId": "run-123",
  "testSuite": "SmokeTests",
  "dependencies": {
    "chocolatey": {
      "java": "11",
      "python3": "3.9.0",
      "googlechrome": "stable"
    }
  },
  "executionCommand": "mvn test -Dsuite=SmokeTests"
}
```

### 4.2 Provisioning Engine Design

The Provisioning Engine will leverage a command-line package manager for Windows to handle software installation. **Chocolatey** [3] is an ideal choice due to its extensive package repository and scripting capabilities.

**Workflow:**

1. **Check for Chocolatey:** The minion first checks if Chocolatey is installed. If not, it installs it using the official PowerShell command.
2. **Parse Dependencies:** The engine parses the `dependencies` object from the job description.
3. **Check Installed Packages:** For each required package, the engine runs `choco list --local-only <package_name>` to see if it's already installed and at the correct version.
4. **Install Missing Packages:** If a package is missing or the wrong version, the engine runs `choco install <package_name> --version <version> -y` to install it.
5. **Update Capabilities:** After provisioning is complete, the minion updates its internal capability list to reflect the newly installed software.
6. **Cache Capabilities:** The minion caches its list of installed packages locally to speed up future checks.

This approach ensures that the environment is correctly configured for every test run without manual intervention.

---

*This document is a work in progress. Sections 5-12 will be detailed in subsequent steps.*

---

## 5. Test Execution Workflow

The end-to-end workflow is designed for automation and minimal latency from test initiation to result reporting.

### 5.1 Workflow Diagram

```
┌──────────────┐   1. Trigger Test Run   ┌──────────┐   2. Poll for Jobs   ┌───────────┐
│ Azure DevOps │─────────────────────────►│ Test Hub │◄────────────────────│ Minion    │
└───────┬──────┘                         └────┬─────┘                      └────┬──────┘
        │                                     │ 3. Assign Job                 │
        │                                     └───────────────────────────────► │
        │                                                                     │ 4. Provision
        │                                                                     │    Dependencies
        │                                                                     │
        │                                                                     │ 5. Start Test
        │                                                                     │    & Streaming
        │                                                                     │
        │                                                                     │ 6. Execute Test
        │                                                                     │
        │                                                                     │ 7. Collect
        │                                                                     │    Artifacts
        │                                                                     │
        │                                     ┌───────────────────────────────┐ │
        │                                     │ 8. Report Completion & Upload │ │
        │                                     └───────────────────────────────┘ │
        │                                                                     │
        │ 10. Attach Video & Logs             │ 9. Process Results            │
        └─────────────────────────────────────◄───────────────────────────────┘
```

### 5.2 Step-by-Step Process

1.  **Trigger Test Run:** A user or a CI/CD pipeline initiates a test run in **Azure DevOps Test Plans**.
2.  **Hub Polls for Jobs:** The **Test Hub** periodically queries the Azure DevOps API for new, queued test runs. It adds these to its internal job queue.
3.  **Minion Requests Job:** An **Execution Minion** in the `IDLE` state sends a heartbeat to the Hub. The Hub's dispatcher sees the idle minion, finds a suitable job in the queue (matching any required capabilities), and assigns it to the minion in the heartbeat response.
4.  **Provision Dependencies:** The minion transitions to the `PROVISIONING` state. The **Provisioning Engine** checks the job's dependencies and uses Chocolatey to install any missing software.
5.  **Start Execution & Streaming:** The minion transitions to the `EXECUTING` state. The **Screen Recorder** component starts FFmpeg, which begins capturing the screen and initiating both the high-quality file recording and the low-latency RTMP stream to the designated streaming server.
6.  **Execute Test:** The **Test Executor** runs the test command (e.g., `npx playwright test`). The test automation framework interacts with the browser or application, and its actions are captured in the video stream.
7.  **Collect Artifacts:** Once the test command finishes, the minion transitions to `COLLECTING`. The **Log & Artifact Collector** gathers the test logs, the high-quality video file, and any other generated files into a `results.zip` archive.
8.  **Report Completion:** The minion enters the `REPORTING` state. It uploads the `results.zip` file to a pre-configured Azure Blob Storage container and then calls the Hub's API to report the job as complete, providing the URL to the results archive and the final test status (passed/failed).
9.  **Hub Processes Results:** The Hub receives the completion notice. It marks the job as finished and updates its internal state. It then queues a task to update Azure DevOps.
10. **Update Azure DevOps:** The Hub calls the Azure DevOps API to update the test run status, publish the test results, and add a link to the high-quality video recording in the test run's attachments or comments section.

---

## 6. Video Streaming and CRT Monitor Interface

This feature provides a unique combination of real-time visibility and nostalgic user experience.

### 6.1 Streaming Architecture

The architecture is designed to separate the low-latency live stream from the high-quality archival recording.

```
On Minion PC:
┌──────────────┐   ┌──────────────────────────────────────────────────────────┐
│              │   │                          FFmpeg                          │
│  Desktop     │──►│ ┌──────────────────┐               ┌───────────────────┐ │
│  Screen      │   │ │ Encode (H.264)   │───(1080p, 30fps)──►│ Save to File      │ │
│              │   │ │ High Quality     │               │ (execution.mp4)   │ │
│              │   │ └──────────────────┘               └───────────────────┘ │
│              │   │ ┌──────────────────┐               ┌───────────────────┐ │
│              │   │ │ Encode (H.264)   │───(480p, 15fps)───►│ Push to RTMP      │ │
│              │   │ │ Low Latency      │               │ Server            │ │
│              │   │ └──────────────────┘               └───────────────────┘ │
│              │   └──────────────────────────────────────────────────────────┘
└──────────────┘

Server & Client:
┌──────────────┐       ┌──────────────────┐       ┌──────────────────┐
│ RTMP Server  │◄──────┤   Minion Stream  │       │ CRT Monitor UI   │
│ (e.g., nginx)│       └──────────────────┘       │ (Web Browser)    │
└──────┬───────┘                                  └────────┬─────────┘
       │                                                   │
       └──────────────────(Play Stream)────────────────────┘
```

-   **RTMP Server:** A dedicated server is required to handle the incoming RTMP streams from all active minions. An open-source solution like the **Nginx RTMP module** [4] is a cost-effective and powerful choice. It can receive the streams and make them available for playback.
-   **FFmpeg Command Example:** The command run by the minion would look something like this:

    ```bash
    ffmpeg -f gdigrab -framerate 30 -i desktop ^
      -c:v libx264 -preset ultrafast -pix_fmt yuv420p -s 1920x1080 -r 30 -b:v 4000k execution.mp4 ^
      -c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -s 854x480 -r 15 -b:v 500k -f flv rtmp://your-rtmp-server/live/minion-007
    ```
    This command simultaneously records a 1080p file and streams a 480p feed.

### 6.2 CRT Monitor Interface Design

The interface is a web application designed to evoke the feeling of watching an old CRT television.

**Visual Elements:**

-   **CRT Bezel:** The entire interface is framed within a realistic image of a vintage television set.
-   **Scan Lines & Curvature:** A subtle overlay on the video player mimics the scan lines and screen curvature of a CRT.
-   **Channel Dial:** A skeuomorphic dial with satisfying click sounds allows the user to 
switch between active minion streams.
-   **Static Noise:** When switching channels or if a minion is idle, a classic "snow" or static noise effect is displayed.
-   **Minion Grid:** A simple grid or list next to the CRT shows all registered minions, their status (Idle, Busy, Offline), and the test they are currently running.

**Functionality:**

1.  **Get Active Streams:** The web UI fetches a list of active minions and their corresponding RTMP stream URLs from the **Test Hub** API.
2.  **Channel Surfing:** When the user turns the dial to a new channel (e.g., Channel 7), the UI loads the video stream for the corresponding minion (e.g., `minion-007`).
3.  **Video Player:** A lightweight HTML5 video player (like **video.js** [5] or **hls.js**) is embedded within the CRT frame to play the RTMP stream (or HLS for broader compatibility).
4.  **Real-Time Status:** The UI periodically polls the Hub for status updates to keep the minion grid current.

### 6.3 User Experience

-   **Instant Gratification:** Stakeholders can immediately see tests running without waiting for a final report.
-   **Engaging Interface:** The retro theme makes the process of monitoring tests more engaging and less monotonous.
-   **Low Bandwidth:** The 480p stream is suitable for viewing over standard office networks or even VPNs without causing congestion.
-   **Dual-Fidelity:** The system provides both a quick, low-fidelity live view and a detailed, high-fidelity recorded view, catering to different needs.

---

## 7. Azure DevOps Integration

Deep integration with Azure DevOps is crucial for a seamless workflow.

### 7.1 Test Plan and Job Ingestion

-   The **Test Hub** will use the Azure DevOps REST API to interact with **Test Plans**.
-   It will query for test runs with a specific status (e.g., `Queued` or `NotStarted`).
-   The Hub will map Azure DevOps test points to individual jobs, including the test case ID, configuration, and any associated requirements.

### 7.2 Result and Artifact Reporting

-   After a minion completes a job, the Hub is responsible for reporting back to Azure DevOps.
-   **Update Test Run Status:** The Hub will update the overall test run status (e.g., `InProgress`, `Completed`).
-   **Publish Test Results:** It will publish the results for each individual test point (Pass/Fail) using the Test Results API.
-   **Attach Video Recording:** The Hub will upload the high-quality video recording from Azure Blob Storage as an attachment to the test run. This provides a permanent, high-fidelity record of the execution directly within Azure DevOps.

**API Example (C#): Attaching a video to a test run**

```csharp
public async Task AttachVideoToTestRun(int testRunId, string videoUrl, string fileName)
{
    var attachmentRequest = new TestAttachmentRequestModel
    {
        Stream = await new HttpClient().GetStreamAsync(videoUrl),
        FileName = fileName,
        Comment = "Test Execution Video Recording",
        AttachmentType = "GeneralAttachment"
    };

    await _testManagementHttpClient.CreateTestRunAttachmentAsync(
        attachmentRequest, _projectName, testRunId);
}
```

---

*This document is a work in progress. Sections 8-12 will be detailed in subsequent steps.*

---

## 8. Implementation Specifications

This section provides detailed specifications for building each component of the system.

### 8.1 Test Hub Implementation

-   **Technology Stack:** ASP.NET Core 8 Web API
-   **Database:** PostgreSQL for storing minion registry, job queue, and status.
-   **Authentication:** API endpoints should be secured using API keys or JWT tokens.
-   **Hosting:** Can be hosted as an Azure App Service for scalability and ease of management.

**API Endpoints:**

| Method | Endpoint | Description | Request Body | Response Body |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/minions/register` | A new minion registers with the Hub. | `MinionInfo` | `MinionRegistrationResponse` |
| `POST` | `/api/minions/heartbeat` | A minion sends a heartbeat and polls for a job. | `MinionStatus` | `JobInstruction` (if job available) |
| `POST` | `/api/jobs/{jobId}/complete` | A minion reports that a job is complete. | `JobCompletionReport` | `200 OK` |
| `GET` | `/api/streams` | The CRT Monitor UI fetches the list of active streams. | (None) | `List<ActiveStreamInfo>` |

### 8.2 Execution Minion Implementation

-   **Technology Stack:** .NET 8 Worker Service. This provides a robust framework for running background tasks on Windows.
-   **Installation:** The minion should be packaged as a Windows Service installer (e.g., using WiX Toolset) for easy deployment on host PCs.
-   **Configuration:** A local `appsettings.json` file will configure the Hub URL, minion ID, and other parameters.
-   **Process Management:** The minion will use `System.Diagnostics.Process` to launch and manage the FFmpeg and test executor processes.

**Podman Integration for Containerized Tests:**

For tests that require a fully isolated environment, the minion can leverage **Podman** [6]. This is a daemonless container engine that provides enhanced security over Docker.

-   **Workflow:**
    1.  The job description will specify a `containerImage` (e.g., `mcr.microsoft.com/playwright:v1.39.0`).
    2.  The **Provisioning Engine** will pull the image using `podman pull`.
    3.  The **Test Executor** will run the tests inside the container using `podman run`.
    4.  Screen capture will still happen on the host machine, recording the container's window or the entire desktop.

### 8.3 CRT Monitor UI Implementation

-   **Technology Stack:** React or Vue.js, using a modern build tool like Vite.
-   **Video Player:** **Video.js** with the `videojs-flash` plugin for RTMP support, or **HLS.js** if the streaming server transcodes to HLS.
-   **Styling:** CSS with animations for the CRT effects (scan lines, static, screen flicker).

---

## 9. Security Considerations

-   **API Security:** All Hub APIs must be authenticated to prevent unauthorized minions from joining the network or reporting false results.
-   **Minion Isolation:** While minions run on host PCs, using Podman for test execution can provide an extra layer of isolation, preventing tests from interfering with the host system.
-   **Secret Management:** Test secrets (e.g., passwords, API keys) should be managed through Azure Key Vault and passed securely to the minion at runtime, not stored in the test code.
-   **Network Security:** The Hub and RTMP server should be placed in a secured network zone. Communication should be over HTTPS/RTMPS.

---

## 10. Deployment Strategy

1.  **Hub & RTMP Server:** Deploy the Test Hub (ASP.NET Core app) and the Nginx RTMP server to Azure (e.g., using Azure App Service and a Linux VM, respectively).
2.  **Minion Installation:** Create a standard installer package for the Execution Minion Windows Service. Use enterprise deployment tools (e.g., SCCM, Intune) to push the installer to all designated Windows PCs in the test farm.
3.  **Configuration:** Configure each minion's `appsettings.json` to point to the deployed Hub URL.
4.  **CRT Monitor UI:** Deploy the web application as a static website (e.g., on Azure Static Web Apps).

---

## 11. Acceptance Criteria

-   **Functional:**
    -   Minions must successfully register with the Hub.
    -   The Hub must successfully pull test runs from Azure DevOps.
    -   Minions must automatically install specified dependencies using Chocolatey.
    -   Live video stream must be viewable on the CRT Monitor UI with less than 3 seconds of latency.
    -   High-quality video recordings must be successfully attached to the corresponding Azure DevOps test run.
-   **Performance:**
    -   The system must support at least 50 concurrent minions without significant degradation in Hub performance.
    -   Job assignment latency (from minion heartbeat to job reception) should be less than 500ms.
-   **Resilience:**
    -   If a minion goes offline during a test, the Hub must re-queue the job after a timeout period.
    -   The system should handle network interruptions gracefully.

---

## 12. References

[1] Selenium. (n.d.). *Selenium Grid*. Retrieved from https://www.selenium.dev/documentation/grid/

[2] FFmpeg. (n.d.). *FFmpeg Documentation*. Retrieved from https://ffmpeg.org/ffmpeg.html

[3] Chocolatey. (n.d.). *Chocolatey - The Package Manager for Windows*. Retrieved from https://chocolatey.org/

[4] Nginx, Inc. (n.d.). *NGINX RTMP Module*. Retrieved from https://github.com/arut/nginx-rtmp-module

[5] Video.js. (n.d.). *Video.js - The Player Framework*. Retrieved from https://videojs.com/

[6] Podman. (n.d.). *Podman: A daemonless container engine*. Retrieved from https://podman.io/

---

**End of Document**
