# Client Agent

The CastleOps Client is a Go-based background service installed on each machine you want to manage. Source: [Tillman32/CastleOps.Client](https://github.com/Tillman32/CastleOps.Client).

## What It Does

- Registers with your CastleOps server on first run and receives an auth token
- Sends **heartbeats** every 30 seconds so the server knows the machine is online
- Collects **CPU, memory, disk, and network metrics** every 60 seconds
- **Polls** the server for commands (e.g., Peon script executions) and executes them
- Caches data locally in **SQLite** and syncs when connectivity is restored (offline-first)

## Platform Support

| Platform | Service Manager | Package Manager |
|----------|-----------------|-----------------|
| macOS Intel (amd64) | launchd | Homebrew |
| macOS Apple Silicon (arm64) | launchd | Homebrew |
| Windows (amd64) | Windows SCM | Chocolatey |
| Linux | *(planned)* systemd | apt / yum / snap |

## Installation

### macOS

```bash
# Install as a system service (requires sudo)
sudo ./castleops-client -config /path/to/config.yaml -install
```

The service registers as `com.castleops.client` and auto-starts at boot.

**File locations:**
```
Binary:   /usr/local/bin/castleops-client
Config:   ~/Library/Application Support/CastleOps/config.yaml
Cache DB: ~/Library/Application Support/CastleOps/data.db
Logs:     ~/Library/Logs/CastleOps/
```

### Windows

Run as Administrator:

```powershell
.\castleops-client.exe -config C:\path\to\config.yaml -install
```

The service registers as `CastleOpsClient` and starts automatically.

**File locations:**
```
Binary:   C:\Program Files\CastleOps\castleops-client.exe
Config:   %LOCALAPPDATA%\CastleOps\config.yaml
Cache DB: %LOCALAPPDATA%\CastleOps\data.db
Logs:     %LOCALAPPDATA%\CastleOps\Logs\
```

## Configuration

```yaml
server:
  url: "http://your-castleops-server:5000"
  tls_verify: true
  timeout: 30s

client:
  id: ""     # Auto-assigned on first registration
  token: ""  # Auto-assigned on first registration

heartbeat:
  interval: 30s
  retry_attempts: 3

metrics:
  collection_interval: 60s
  batch_size: 100
  retention_days: 7

cache:
  type: "sqlite"
  path: ""   # Uses platform default if empty

logging:
  level: "info"    # debug, info, warn, error
  format: "json"   # json or console

package_managers:
  preferred: "auto"   # auto, homebrew, chocolatey
```

All values can be overridden with environment variables:

```bash
export CASTLEOPS_SERVER_URL="http://your-server:5000"
export CASTLEOPS_LOGGING_LEVEL="debug"
export CASTLEOPS_METRICS_COLLECTION_INTERVAL="30s"
```

## Service Management

### macOS

```bash
launchctl start com.castleops.client
launchctl stop com.castleops.client
launchctl list | grep castleops
```

### Windows

```powershell
Start-Service CastleOpsClient
Stop-Service CastleOpsClient
Get-Service CastleOpsClient
```

## Building from Source

Requires Go 1.25.4+ and GCC (for SQLite CGO compilation).

```bash
git clone https://github.com/Tillman32/CastleOps.Client.git
cd CastleOps.Client

make build           # Current platform
make build-darwin    # macOS (amd64 + arm64)
make build-windows   # Windows (requires mingw-w64)
make build-linux     # Linux
make release         # All platforms
make test            # Run tests with race detection
```

## Current Status

- 127 tests total (119 passing, 4 failing in API client tests)
- 51.8% overall test coverage
- Zero race conditions detected
- Production readiness: ~56%

**Remaining work**: Fix 4 failing API tests; expand coverage for agent orchestrator and package manager components.

## Uninstallation

```bash
# macOS
sudo castleops-client -uninstall

# Windows (as Administrator)
.\castleops-client.exe -uninstall
```

## Security Notes

- The auth token is stored in the config file. Set file permissions to `600` on macOS/Linux; use NTFS ACLs on Windows.
- Always set `tls_verify: true` in production.
- The Windows service runs as `NT AUTHORITY\LocalService` (limited privileges).
- The Windows service is configured to auto-restart on failure (3 attempts, 60s delay).
