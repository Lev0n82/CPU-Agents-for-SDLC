# Autonomous Agent - Windows 11 Deployment & Testing Guide


## 1. Introduction

This guide provides comprehensive instructions for deploying and testing the Autonomous AI Agent on a **Windows 11** machine. It covers three primary deployment methods, each suited for different use cases from development to production.

### Deployment Options

1.  **Direct Execution (Development & Debugging):** The simplest method, ideal for development, debugging, and observing the agent's console output in real-time.
2.  **Windows Service (Production):** The recommended method for production use. The agent runs as a background service, starts automatically with Windows, and runs even when no user is logged in.
3.  **Podman Container (Isolated Testing):** Packages the agent into a container for isolated, reproducible testing environments. This is an advanced option that ensures no conflicts with other software on the host machine.

---

## 2. Prerequisites

Before you begin, ensure your Windows 11 machine meets the following requirements:

-   **Windows 11 Pro/Enterprise** (for full feature support)
-   **Administrator Privileges** (required for Windows Service installation and system reboot functionality)
-   **.NET 8.0 SDK:** [Download and install from Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
-   **Git:** [Download and install from git-scm.com](https://git-scm.com/download/win)
-   **(Optional) Podman:** [Download and install Podman Desktop](https://podman-desktop.io/downloads) for containerized deployment.

---

## 3. Getting the Source Code

First, clone the repository and check out the correct branch.

```powershell
# Open PowerShell or Command Prompt

# Clone the repository
git clone https://github.com/Lev0n82/AskMarilyn.git

# Navigate to the repository directory
cd AskMarilyn

# Switch to the feature branch
git checkout feature/autonomous-agent
```

All subsequent commands should be run from within the `AskMarilyn` directory.

---

## 4. Method 1: Direct Execution (Development)

This method runs the agent in a console window, making it easy to see logs and test changes quickly.

### Step 1: Build the Project

Build the solution to restore dependencies and compile the code.

```powershell
# Navigate to the agent's source directory
cd agent

# Build the project
dotnet build
```

### Step 2: Run the Agent

Execute the agent from its project directory.

```powershell
# Navigate to the core project directory
cd src\AutonomousAgent.Core

# Run the agent
dotnet run
```

### What to Expect

A console window will appear, and you will see the agent's startup sequence:

1.  **Self-Test Sequence:** The agent will run all its internal tests. You will see `[PASS]` messages for each test.
2.  **Operational Mode:** Once tests are complete, the agent will enter its operational mode.
3.  **Scheduling Service:** The scheduling service will be running in the background, and the agent will log that it is waiting for the configured reboot time.

To **stop the agent**, press `Ctrl + C` in the console window.

---

## 5. Method 2: Windows Service (Production)

This is the most robust method for running the agent in a production environment.

### Step 1: Publish the Project

Publish the project to create a self-contained executable and all its dependencies.

```powershell
# Navigate to the agent's source directory
cd agent

# Publish for Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

This command creates a `publish` folder inside the `agent` directory containing all the necessary files.

### Step 2: Install the Windows Service

You must run PowerShell or Command Prompt **as an Administrator** for this step.

```powershell
# Open PowerShell as Administrator

# Get the full path to the executable
$publishDir = (Get-Location).Path + "\agent\publish"
$exePath = $publishDir + "\AutonomousAgent.Core.exe"

# Create the Windows Service
sc.exe create "AutonomousAgent" binPath=$exePath

# Configure the service to start automatically
sc.exe config "AutonomousAgent" start=auto

# (Optional) Add a description
sc.exe description "AutonomousAgent" "Self-aware autonomous AI agent for enterprise testing."
```

### Step 3: Start the Service

You can start the service from the command line or the Services app.

```powershell
# Start the service
sc.exe start "AutonomousAgent"
```

### How to Verify

-   Open the **Services** app (run `services.msc`).
-   Find **AutonomousAgent** in the list.
-   The status should be **Running** and the Startup Type should be **Automatic**.

### Viewing Logs

Since the service runs in the background, logs are sent to the **Windows Event Viewer**.

1.  Open **Event Viewer** (run `eventvwr.msc`).
2.  Navigate to **Windows Logs > Application**.
3.  Look for events with the source `.NET Runtime` or `AutonomousAgent`.

### Stopping or Uninstalling the Service

```powershell
# Open PowerShell as Administrator

# Stop the service
sc.exe stop "AutonomousAgent"

# Uninstall (delete) the service
sc.exe delete "AutonomousAgent"
```

---

## 6. Method 3: Podman Container (Isolated Testing)

This method runs the agent inside a lightweight, isolated Podman container. This is perfect for ensuring the agent's environment is consistent and doesn't interfere with other applications.

*(Detailed instructions and a `Containerfile` will be provided in the next phase.)*

---

## 7. Testing and Validation

Once the agent is running (using any of the methods above), you can validate its core features.

### Test 1: Self-Test on Startup

-   **Expected Outcome:** The agent runs its self-test sequence immediately upon starting. If you are using the Direct Execution method, you will see the test results in the console. For the Windows Service method, these logs will be in the Event Viewer.
-   **Validation:** Ensure all tests show `[PASS]` and the agent enters operational mode. To test the failure case, you can intentionally break a test in `ExampleTests.cs` and see the agent refuse to start.

### Test 2: Scheduled Midnight Reboot

This test requires changing the configuration to avoid waiting until midnight.

1.  **Edit the config file:** Open `agent/src/AutonomousAgent.Core/appsettings.json`.
2.  **Change the reboot time:** Set the `Hour` and `Minute` to a time a few minutes in the future.
    ```json
    "Scheduler": {
      "NightlyReboot": {
        "Enabled": true,
        "Hour": 15,   // e.g., 3 PM
        "Minute": 30  // e.g., 30 minutes past the hour
      }
    }
    ```
3.  **Restart the agent.**
4.  **Wait for the configured time.**

-   **Expected Outcome:** At the specified time, the agent will log that it is initiating a reboot, and your Windows 11 machine will restart.
-   **Validation (for Windows Service):** After the reboot, log back into Windows, open the Services app, and confirm that the `AutonomousAgent` service has automatically started again.

---

## 8. Troubleshooting

-   **Service Fails to Start:** Check the Windows Event Viewer for error messages. The most common cause is a failed self-test.
-   **Access Denied:** Ensure you are running PowerShell or Command Prompt **as an Administrator** when creating or starting the Windows Service.
-   **Reboot Doesn't Happen:**
    -   Verify the `Scheduler.NightlyReboot.Enabled` is `true` in `appsettings.json`.
    -   Ensure the agent has the necessary permissions to execute the `shutdown.exe` command.

---

**End of Guide**
