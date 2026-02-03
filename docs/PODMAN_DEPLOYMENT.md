# Autonomous Agent - Podman Deployment Guide

**Project:** Self-Aware CPU-Optimized Autonomous AI Agent  
**Date:** January 31, 2026  
**Author:** Manus AI

---

## 1. Introduction

This guide provides detailed instructions for deploying the Autonomous AI Agent using **Podman** on Windows 11. Podman is a daemonless, rootless container engine that provides an isolated, reproducible environment for running the agent without interfering with other software on your system.

### Why Use Podman?

-   **Isolation:** The agent runs in its own container, completely isolated from the host system.
-   **Reproducibility:** The container ensures the agent runs identically on any machine.
-   **Security:** Podman runs rootless by default, reducing security risks.
-   **No Docker Daemon:** Unlike Docker, Podman doesn't require a background daemon, making it lighter and more secure.

---

## 2. Prerequisites

Before you begin, ensure you have the following installed on your Windows 11 machine:

-   **Podman Desktop:** [Download and install from podman-desktop.io](https://podman-desktop.io/downloads)
-   **Git:** [Download and install from git-scm.com](https://git-scm.com/download/win)
-   **Administrator Privileges:** Required for initial Podman setup

### Installing Podman Desktop

1.  Download the Windows installer from the Podman Desktop website.
2.  Run the installer and follow the on-screen instructions.
3.  After installation, launch **Podman Desktop**.
4.  Podman Desktop will automatically set up the Podman machine (a lightweight WSL2 VM).

---

## 3. Getting the Source Code

Clone the repository and navigate to the agent directory.

```powershell
# Open PowerShell

# Clone the repository
git clone https://github.com/Lev0n82/AskMarilyn.git

# Navigate to the agent directory
cd AskMarilyn\agent
```

---

## 4. Building the Container Image

The agent includes a `Containerfile` (Podman's equivalent of a Dockerfile) that defines how to build the container image.

### Step 1: Build the Image

Run the following command from the `agent` directory:

```powershell
podman build -t autonomous-agent:latest -f Containerfile .
```

**Explanation:**
-   `podman build`: Builds a container image
-   `-t autonomous-agent:latest`: Tags the image with the name `autonomous-agent` and version `latest`
-   `-f Containerfile`: Specifies the Containerfile to use
-   `.`: Uses the current directory as the build context

### Step 2: Verify the Image

Check that the image was created successfully:

```powershell
podman images
```

You should see `autonomous-agent` in the list with the tag `latest`.

---

## 5. Running the Container

Once the image is built, you can run the agent in a container.

### Basic Run Command

```powershell
podman run --name autonomous-agent-instance autonomous-agent:latest
```

This command will:
1.  Create a new container named `autonomous-agent-instance`
2.  Run the agent inside the container
3.  Display the agent's console output (including self-test results)

### Running in Detached Mode (Background)

To run the agent in the background:

```powershell
podman run -d --name autonomous-agent-instance autonomous-agent:latest
```

The `-d` flag runs the container in detached mode.

### Viewing Logs

To see the agent's logs when running in detached mode:

```powershell
podman logs autonomous-agent-instance
```

To follow the logs in real-time:

```powershell
podman logs -f autonomous-agent-instance
```

---

## 6. Managing the Container

### Stopping the Container

```powershell
podman stop autonomous-agent-instance
```

### Starting a Stopped Container

```powershell
podman start autonomous-agent-instance
```

### Removing the Container

```powershell
podman rm autonomous-agent-instance
```

**Note:** You must stop the container before removing it, or use the `-f` flag to force removal.

### Checking Container Status

```powershell
podman ps -a
```

This shows all containers, including stopped ones.

---

## 7. Customizing the Configuration

The agent's configuration is stored in `appsettings.json`. To customize it when running in a container, you have two options:

### Option 1: Mount a Custom Configuration File

Create a custom `appsettings.json` on your host machine, then mount it into the container:

```powershell
# Create a custom config file
# Edit C:\path\to\custom\appsettings.json with your settings

# Run the container with the mounted config
podman run -d --name autonomous-agent-instance `
  -v C:\path\to\custom\appsettings.json:/app/appsettings.json:Z `
  autonomous-agent:latest
```

**Explanation:**
-   `-v`: Mounts a volume (binds a host file to a container file)
-   `C:\path\to\custom\appsettings.json`: Path to your custom config on the host
-   `/app/appsettings.json`: Path inside the container where the config is expected
-   `:Z`: Tells Podman to relabel the file for SELinux compatibility (important for security)

### Option 2: Rebuild the Image with Custom Config

1.  Edit `agent/src/AutonomousAgent.Core/appsettings.json` in your local repository.
2.  Rebuild the container image:
    ```powershell
    podman build -t autonomous-agent:latest -f Containerfile .
    ```
3.  Run the new image.

---

## 8. Running as a Systemd Service (Advanced)

For production use on Windows with WSL2, you can configure Podman to auto-start the container using systemd.

### Step 1: Generate a Systemd Unit File

```powershell
podman generate systemd --new --name autonomous-agent-instance > autonomous-agent.service
```

### Step 2: Install the Service

Copy the generated `autonomous-agent.service` file to the systemd user directory and enable it:

```bash
# Inside the WSL2 Podman machine
mkdir -p ~/.config/systemd/user
cp autonomous-agent.service ~/.config/systemd/user/
systemctl --user daemon-reload
systemctl --user enable autonomous-agent.service
systemctl --user start autonomous-agent.service
```

The container will now start automatically when the Podman machine starts.

---

## 9. Testing and Validation

### Test 1: Self-Test on Startup

Run the container and check the logs:

```powershell
podman run --name test-agent autonomous-agent:latest
```

-   **Expected Outcome:** You should see the self-test sequence in the console output, with all tests showing `[PASS]`.
-   **Validation:** The agent should enter operational mode after the tests complete.

### Test 2: Scheduled Reboot (Limited in Container)

**Important Note:** The scheduled reboot functionality is designed to reboot the **host machine**, not the container. In a containerized environment, the agent will attempt to execute the reboot command, but it will fail because the container doesn't have permission to reboot the host.

To test the scheduling logic without actually rebooting:
1.  Modify the `SchedulingService.cs` to log a message instead of executing the reboot command (for testing purposes only).
2.  Rebuild the image.
3.  Run the container with a near-future reboot time in the config.
4.  Check the logs to verify the scheduling logic triggers at the correct time.

For full reboot functionality, use the **Windows Service** deployment method instead.

---

## 10. Troubleshooting

### Issue: "Cannot connect to Podman"

-   **Solution:** Ensure Podman Desktop is running and the Podman machine is started. You can check the status in Podman Desktop or run `podman machine list`.

### Issue: Build fails with "No such file or directory"

-   **Solution:** Ensure you are running the `podman build` command from the `agent` directory, where the `Containerfile` is located.

### Issue: Container exits immediately

-   **Solution:** Check the logs with `podman logs <container-name>`. The most common cause is a failed self-test. Review the test output to identify the failing test.

---

## 11. Cleaning Up

To remove all agent-related containers and images:

```powershell
# Stop and remove all containers
podman stop autonomous-agent-instance
podman rm autonomous-agent-instance

# Remove the image
podman rmi autonomous-agent:latest

# (Optional) Prune unused images and containers
podman system prune -a
```

---

## 12. Summary

Podman provides a robust, isolated environment for running the Autonomous AI Agent. The key commands are:

| Action | Command |
|--------|---------|
| Build image | `podman build -t autonomous-agent:latest -f Containerfile .` |
| Run container | `podman run --name autonomous-agent-instance autonomous-agent:latest` |
| Run in background | `podman run -d --name autonomous-agent-instance autonomous-agent:latest` |
| View logs | `podman logs -f autonomous-agent-instance` |
| Stop container | `podman stop autonomous-agent-instance` |
| Remove container | `podman rm autonomous-agent-instance` |

For production use on Windows 11, the **Windows Service** method is recommended for full functionality, including the system reboot feature. Use Podman for isolated testing and development environments.

---

**End of Guide**
