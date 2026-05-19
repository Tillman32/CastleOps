# Peons

A **Peon** is a lightweight automation script that performs a specific task on a managed device. Think of them like PowerShell DSC configurations or Ansible playbooks, but simpler — a single script with a `peon.yml` manifest.

## What Peons Can Do

- Check device availability (ping)
- Enroll a remote Windows PC via WinRM
- Install or configure software
- Collect custom diagnostics
- Run any script executable on the target OS

## peon.yml Specification

Every Peon repository must include a `peon.yml` (or `peon.yaml`) at the root.

```yaml
peon:
  version: "1.0.0"              # Semantic version
  description: "What this does" # Clear description
  author: "YourName"            # Author or org
  tags: ["tag1", "tag2"]        # Categorization tags
  giturl: "https://github.com/org/peon-repo"  # Repo URL
  type: "powershell"            # powershell | python | bash | nodejs
  entrypoint: "./script.ps1"    # Relative path to main script
  environment:
    ENV_VAR_ONE: ""             # Empty default = required
    ENV_VAR_TWO: "default"      # Has a default
  requirements:
    os: ["windows"]             # windows | darwin | linux
    dependencies: []            # External tools required
```

### Extended Input Schema

Some Peons use an `inputs` array for richer UI integration (form fields in the Web UI):

```yaml
inputs:
  - id: REMOTE_ADMIN_USERNAME
    type: textBox
    description: "The name of the remote admin user"
    default: "CastleOpsWinRm"
    required: true
  - id: REMOTE_ADMIN_PASSWORD
    type: password
    description: "The password of the remote admin user"
    required: true
```

Supported input types: `textBox`, `password`

## Available Peons

### peon-ping

**Repo**: [MorphStack/peon-ping](https://github.com/MorphStack/peon-ping)  
**OS**: Windows  
**Type**: PowerShell  

Pings a device to check its availability.

```yaml
environment:
  PING_DEVICE: "localhost"
  PING_COUNT: "4"
```

### castle-peon-add-remote-windows-pc

**Repo**: [MorphStack/castle-peon-add-remote-windows-pc](https://github.com/MorphStack/castle-peon-add-remote-windows-pc)  
**OS**: Windows  
**Type**: PowerShell  

Enrolls a remote Windows PC into CastleOps via WinRM. Creates a local admin account, enables WinRM, and configures firewall rules for remote management.

**Required inputs:**
- `REMOTE_ADMIN_USERNAME` — admin account to create (default: `CastleOpsWinRm`)
- `REMOTE_ADMIN_PASSWORD` — password for that account

## How Peons Are Executed

1. User installs a Peon in CastleOps (API fetches `peon.yml` from GitHub)
2. Server creates a `ClientCommand` targeting a specific device
3. Client agent polls for commands, downloads and runs the entrypoint script
4. Environment variables from `PeonConfig` are injected at runtime
5. Exit code and output are reported back via the command result endpoint

## Writing Your Own Peon

1. Create a new GitHub repository
2. Add your script (PowerShell, Python, Bash, etc.)
3. Add `peon.yml` at the root following the spec above
4. Test it manually on a target device
5. Optionally submit to the [Marketplace](./marketplace.md)

### Example: Minimal PowerShell Peon

`peon.yml`:
```yaml
peon:
  version: "1.0.0"
  description: "Lists running processes sorted by CPU usage"
  author: "YourName"
  tags: ["diagnostics", "processes"]
  giturl: "https://github.com/yourname/peon-processes"
  type: "powershell"
  entrypoint: "./run.ps1"
  requirements:
    os: ["windows"]
    dependencies: []
```

`run.ps1`:
```powershell
Get-Process | Select-Object Name, CPU, WorkingSet | Sort-Object CPU -Descending
```

### Tips

- Read all inputs from environment variables (set via `environment` in `peon.yml` and overridden per-device in `PeonConfig`)
- Aim for idempotency — the script should be safe to run multiple times
- Exit with code `0` on success, non-zero on failure — the agent uses the exit code to report success/failure
- Keep the script focused on a single task
