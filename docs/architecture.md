# Architecture

## Overview

CastleOps follows a hub-and-spoke model. A central server (the "castle") manages remote agents (the "clients") installed on each device in the home. Automation scripts called **Peons** run on managed devices and are sourced from a community **Marketplace**.

```
┌──────────────────────────────────────────────────────┐
│                   CastleOps Server                    │
│                                                       │
│  ┌───────────────┐          ┌───────────────────────┐ │
│  │ CastleOps.Api │◄────────►│   CastleOps.Web       │ │
│  │ (ASP.NET)     │          │   (Blazor Server)     │ │
│  │               │          │                       │ │
│  │  SQLite DB    │          │   Dashboard           │ │
│  │  (EF Core)    │          │   Devices             │ │
│  └──────┬────────┘          │   Peons               │ │
│         │                   │   Marketplace         │ │
└─────────┼───────────────────┴───────────────────────┘ │
          │ REST API (HTTP)                              │
          │                                              │
   ┌──────▼──────┐    ┌─────────────┐                   │
   │   Client    │    │   Client    │  ...               │
   │   Agent     │    │   Agent     │                    │
   │  (Go/macOS) │    │  (Go/Win)   │                    │
   └─────────────┘    └─────────────┘                    │
                                                         │
          ┌───────────────────────────────┐              │
          │  MorphStack/peon-marketplace  │              │
          │  (GitHub — JSON registry)     │              │
          └───────────────────────────────┘              │
```

## Components

### CastleOps.Api

The central REST API server. Built with ASP.NET Core, it:

- Manages **client registration** and **Bearer token authentication** for agents
- Stores all data in a local **SQLite** database (via Entity Framework Core)
- Fetches Peon definitions from GitHub via the **GitHubClient**
- Dispatches **commands** to client agents and collects results
- Exposes **OpenAPI** docs in Development mode

**Key services:**

| Service | Responsibility |
|---------|---------------|
| `ClientService` | Agent registration, heartbeat processing, metrics ingestion, command dispatch |
| `DeviceService` | Device inventory management |
| `PeonService` | Peon install/uninstall; reads `peon.yml` from GitHub |
| `MarketplaceService` | Fetches and caches the marketplace registry from GitHub |

### CastleOps.Core

Shared class library referenced by both `Api` and `Web`. Contains:

- **Models**: `Client`, `Device`, `Peon`, `PeonConfig`, `ClientCommand`, `ClientMetric`, `MarketplaceItem`
- **DTOs**: Request/response shapes for API endpoints
- **HTTP Clients**: `GitHubClient` for fetching `peon.yml` and marketplace data

### CastleOps.Web

Blazor Server frontend. Communicates with `CastleOps.Api` over HTTP. Pages:

- **Dashboard** — Overview of managed devices and system health
- **Devices** — List and manage registered devices
- **Peons** — View installed Peons and their per-device configurations
- **Marketplace** — Browse and install community Peons

### CastleOps.Client (separate repo)

A Go-based background service installed on each managed machine. It:

- **Registers** with the server on first run, receiving an auth token
- Sends **heartbeats** every 30 seconds (configurable)
- Collects and uploads **system metrics** (CPU, memory, disk, network) every 60 seconds
- **Polls** the server for pending commands (Peon executions, etc.)
- Supports **offline-first** operation with local SQLite caching
- Integrates with **Homebrew** (macOS) and **Chocolatey** (Windows) for package management

### Peon Marketplace (separate repo)

A TypeScript/Node.js repository at `MorphStack/peon-marketplace` that serves as the Peon registry. It:

- Stores the list of approved Peons in `config/peon-marketplace.json`
- Validates submitted `peon.yml` configurations via GitHub Actions CI
- Is fetched by `MarketplaceService` in the API at runtime

## Data Flow: Agent Lifecycle

```
1. Install castleops-client on a device
2. Agent calls  POST /api/v1/clients/register
   → Server creates Client record, returns { clientId, token }
3. Agent persists clientId + token in local config
4. Every 30s:   POST /api/v1/clients/{id}/heartbeat
   → Server updates Client.LastSeen and Client.Status
5. Every 60s:   POST /api/v1/clients/{id}/metrics
   → Server stores ClientMetric records
6. Every 30s:   GET  /api/v1/clients/{id}/commands
   → Server returns pending ClientCommand records
7. Agent executes command (e.g., runs a Peon script)
8.              POST /api/v1/clients/{id}/commands/{cmdId}/result
   → Server marks command complete, stores output
```

## Data Flow: Peon Execution

```
1. User browses Marketplace in the Web UI
2. Web UI calls  GET  /api/v1/marketplace
   → API fetches registry JSON from GitHub (cached in memory)
3. User selects a Peon and a target device
4. Web UI calls  POST /api/v1/peons  (install)
5. API reads peon.yml from the Peon's GitHub repo
6. API stores Peon + PeonConfig in the database
7. API creates a ClientCommand for the target client
8. Client agent polls commands, downloads and executes the Peon script
9. Results returned via POST /api/v1/clients/{id}/commands/{cmdId}/result
```

## Database Schema

SQLite, single file managed by EF Core.

| Table | Key Fields |
|-------|------------|
| `Clients` | Id, Hostname, OS, OSVersion, Architecture, AgentVersion, Status, LastSeen, TokenHash |
| `ClientMetrics` | Id, ClientId, timestamp, CPU/memory/disk data |
| `ClientCommands` | Id, ClientId, type, payload, status, result |
| `Devices` | Id, name |
| `Peons` | Id, Slug, Name, Url, Type, Author, Entry, DefaultVersion, DefaultEnvironment |
| `PeonConfigs` | Id, PeonId, DeviceId, per-device environment overrides |
| `MarketplaceItems` | Id, Name, Author, Url, Type, OS |

## Authentication

Client agents authenticate using **Bearer tokens**:

1. On registration the server generates a random token, hashes it (`TokenHash`), and returns the plaintext token once.
2. Subsequent agent requests include `Authorization: Bearer <token>`.
3. The server hashes the incoming token and compares it to the stored hash.

No user authentication is implemented yet in the web frontend.

## Deployment

The API and Web are each containerized. `docker-compose.yml` orchestrates both:

- `castleops-api` — port `5000` (configurable via `API_PORT`)
- `castleops-web` — port `8080` (configurable via `WEB_PORT`)
- Shared `castleops-network` bridge network
- Persistent volumes for `data/` and `logs/`
