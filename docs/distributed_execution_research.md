# Distributed Test Execution Research Notes

## Distributed Testing Architectures

### Hub-Node (Master-Slave) Architecture
- **Selenium Grid**: Industry standard for distributed browser testing
- **Hub**: Central coordinator that receives test requests and distributes to nodes
- **Nodes**: Worker machines that execute tests on specific browser/OS combinations
- **Benefits**: Highly scalable, parallel execution, cross-platform testing

### Key Concepts
- **Parallel Execution**: Run multiple tests simultaneously across different machines
- **Cross-Platform Testing**: Test on different OS, browser, and device combinations
- **Dynamic Node Registration**: Nodes can join/leave the grid dynamically
- **Load Balancing**: Hub distributes tests based on node availability and capabilities

## Screen Recording Technologies

### FFmpeg for Screen Capture
- **gdigrab (Windows)**: Built-in screen grabber for Windows
  - `ffmpeg -f gdigrab -framerate 30 -i desktop output.mp4`
- **Low Latency Streaming**: Can stream to RTMP/HLS endpoints
- **Configurable Quality**: Adjust framerate, resolution, bitrate for bandwidth

### Test Execution Recording
- **Monte Screen Recorder**: Popular for Selenium test recording
- **Attach to Test Results**: Videos can be attached to test reports
- **Debugging Aid**: Visual record of test execution for failure analysis

## Video Streaming Approaches

### Low Latency Streaming
- **RTMP**: Real-Time Messaging Protocol (1-3 second latency)
- **WebRTC**: Ultra-low latency (<500ms) but complex setup
- **HLS**: HTTP Live Streaming (5-10 second latency) but widely compatible
- **Low Resolution**: 480p significantly reduces bandwidth and latency

### Streaming Architecture
1. **Capture**: FFmpeg captures screen during test execution
2. **Encode**: H.264 encoding for compression
3. **Stream**: Push to streaming server (RTMP/WebRTC)
4. **View**: Clients connect to streaming server
5. **Record**: Simultaneously save to file for Azure DevOps upload

## Relevant Technologies

### Selenium Grid 4
- Hub-Node architecture
- Podman support for easy deployment
- Observability features (metrics, logging, tracing)
- Dynamic node registration

### Podman for Test Nodes
- Containerized test environments
- Easy deployment and scaling
- Consistent environment across nodes

### AWS Device Farm / Testing Farm
- Managed device farms for testing
- Real devices and emulators
- Parallel execution across multiple devices
