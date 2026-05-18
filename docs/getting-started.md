# Getting Started

## Prerequisites

- Docker and Docker Compose (recommended), **or**
- .NET 9 SDK for running without Docker

## Running with Docker

```bash
# 1. Clone the repository
git clone https://github.com/Tillman32/CastleOps.git
cd CastleOps

# 2. Copy and edit the environment file
cp .env.example .env

# 3. Start all services
docker compose up -d

# 4. Verify services are running
docker compose ps
```

- **API**: http://localhost:5000
- **Web UI**: http://localhost:8080
- **OpenAPI**: Not available in Production mode — set `ASPNETCORE_ENVIRONMENT=Development` in `.env` to enable.

### Stopping

```bash
docker compose down
```

Data persists in `./data/` (SQLite database) and logs in `./logs/` on the host. These directories are bind-mounted into the container at `/app/data` and `/app/logs` respectively.

## Running Without Docker

Requires .NET 9 SDK.

```bash
# Restore dependencies
dotnet restore CastleOps.sln

# Terminal 1 — Run the API
dotnet run --project CastleOps.Api

# Terminal 2 — Run the Web UI
dotnet run --project CastleOps.Web
```

In Development mode the SQLite database is auto-created on first run (`EnsureCreated`). No migration step is needed.

On Windows/macOS the database and logs are stored under `%LOCALAPPDATA%\CastleOps\` or `~/.local/share/CastleOps/`.

## Connecting a Client Agent

With the server running, install the Go client agent on any machine you want to manage. See [Client Agent](./client-agent.md).

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` for OpenAPI and SQLite auto-create |
| `API_PORT` | `5000` | Host port for the API |
| `WEB_PORT` | `8080` | Host port for the Web UI |

## Production Checklist

- [ ] Lock down CORS — the default policy (`AllowAnyOrigin`) is development-only
- [ ] Run EF migrations: `cd CastleOps.Api && dotnet ef database update`
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production` (disables OpenAPI endpoint)
- [ ] Ensure `./data/` is on persistent storage (mounted volume in Docker)
- [ ] Rotate logs — 14-day rolling retention is configured by default
- [ ] Put a reverse proxy (nginx/Caddy) in front for TLS termination
