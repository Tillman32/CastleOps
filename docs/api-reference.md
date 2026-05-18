# API Reference

Base URL: `http://localhost:5000/api/v1`

All endpoints return JSON. Authenticated endpoints require `Authorization: Bearer <token>` obtained during client registration.

---

## Clients

Handles registration and ongoing communication with CastleOps.Client agents.

### Register Client

`POST /api/v1/clients/register`

Registers a new agent. Returns a one-time plaintext token — store it securely in the agent config.

**Request:**
```json
{
  "hostname": "my-macbook",
  "os": "darwin",
  "osVersion": "14.5",
  "architecture": "arm64",
  "agentVersion": "1.0.0"
}
```

**Response:**
```json
{
  "clientId": "<uuid>",
  "token": "<plaintext-token>"
}
```

---

### Heartbeat

`POST /api/v1/clients/{id}/heartbeat` — **Auth required**

Updates the client's `LastSeen` and `Status`. Called by the agent every ~30 seconds.

**Request:**
```json
{
  "uptime": 3600,
  "status": "online"
}
```

**Response:** Server acknowledgement with any updated configuration (heartbeat/metrics intervals).

---

### Upload Metrics

`POST /api/v1/clients/{id}/metrics` — **Auth required**

Batch-uploads system metrics collected by the agent.

**Request:**
```json
{
  "metrics": [
    {
      "timestamp": "2025-01-01T00:00:00Z",
      "cpuPercent": 12.5,
      "memoryUsedBytes": 4294967296,
      "memoryTotalBytes": 17179869184,
      "diskUsedBytes": 107374182400,
      "diskTotalBytes": 536870912000
    }
  ]
}
```

---

### Get Pending Commands

`GET /api/v1/clients/{id}/commands` — **Auth required**

Polls for commands queued for this client (e.g., Peon script executions).

**Response:**
```json
{
  "commands": [
    {
      "id": "<cmd-uuid>",
      "type": "run_peon",
      "payload": {}
    }
  ]
}
```

---

### Submit Command Result

`POST /api/v1/clients/{id}/commands/{cmdId}/result` — **Auth required**

Reports the outcome of a command execution.

**Request:**
```json
{
  "success": true,
  "output": "Peon executed successfully",
  "exitCode": 0
}
```

---

### List Clients

`GET /api/v1/clients`

Returns all registered client agents.

---

### Get Client

`GET /api/v1/clients/{id}`

**Client object fields:**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `hostname` | string | Machine hostname |
| `os` | string | `darwin`, `windows`, or `linux` |
| `osVersion` | string | OS version string |
| `architecture` | string | CPU arch (`amd64`, `arm64`) |
| `agentVersion` | string | CastleOps.Client version |
| `status` | string | `online`, `degraded`, or `offline` |
| `lastSeen` | datetime | Timestamp of last heartbeat |
| `uptime` | long | Uptime in seconds (from last heartbeat) |
| `heartbeatInterval` | int | Configured heartbeat interval (seconds) |
| `metricsInterval` | int | Configured metrics collection interval (seconds) |

---

## Devices

### List Devices

`GET /api/v1/devices`

### Get Device

`GET /api/v1/devices/{id}`

### Register Device

`POST /api/v1/devices`

**Request:**
```json
{
  "name": "My PC"
}
```

---

## Marketplace

### List Marketplace Items

`GET /api/v1/marketplace`

Fetches the current Peon marketplace listing. Data is sourced from `MorphStack/peon-marketplace` on GitHub and cached in memory.

**Response:**
```json
[
  {
    "name": "Ping Device",
    "description": "A simple peon that pings a device to check its availability.",
    "author": "CastleOps",
    "url": "https://github.com/MorphStack/Peon-Ping",
    "tags": ["healthcheck", "monitoring", "ping"],
    "type": "powershell",
    "os": ["windows"]
  }
]
```

---

## Peons

### List Peons

`GET /api/v1/peons`

Lists all Peons installed in this CastleOps instance.

### Get Peon

`GET /api/v1/peons/{id}`

### Install Peon

`POST /api/v1/peons`

Installs a Peon from a GitHub repository URL. The API fetches `peon.yml` from the repository and stores the Peon and its default configuration.

### Delete Peon

`DELETE /api/v1/peons/{id}`

---

## Authentication Errors

All Bearer-authenticated endpoints return these errors on failure:

| HTTP | Code | Meaning |
|------|------|---------|
| 401 | `AUTH_REQUIRED` | No Authorization header provided |
| 401 | `INVALID_AUTH_FORMAT` | Header is not `Bearer <token>` |
| 401 | `TOKEN_REQUIRED` | Token value is empty |
| 401 | `INVALID_TOKEN` | Token does not match stored hash |
