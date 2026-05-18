# CastleOps

Self-hosted home device management for families. Think "Active Directory lite" — manage desktops, laptops, and Macs across a home network without AD/LDAP complexity.

**Status**: Work in progress, not yet released.

## Repository Layout

```
CastleOps/
├── CastleOps.Api/        # ASP.NET Core REST API
│   ├── Controllers/      # HTTP endpoints (Clients, Devices, Marketplace, Peons)
│   ├── Services/         # Business logic
│   ├── Infrastructure/   # Database (EF Core + SQLite), in-memory caching
│   └── Migrations/       # EF Core migrations
├── CastleOps.Core/       # Shared library
│   ├── Models/           # Entity models (Client, Device, Peon, etc.)
│   ├── DTOs/             # Request/response shapes
│   ├── HTTP/             # HTTP client helpers (GitHubClient)
│   └── Types/            # Shared enums/types
└── CastleOps.Web/        # Blazor Server frontend
    ├── Pages/            # Razor pages (Dashboard, Devices, Marketplace, Peons)
    ├── Components/       # Shared UI components
    └── Layout/           # App layout
```

## Related Repositories

| Repo | Purpose |
|------|---------|
| [Tillman32/CastleOps.Client](https://github.com/Tillman32/CastleOps.Client) | Go agent installed on managed machines |
| [MorphStack/peon-marketplace](https://github.com/MorphStack/peon-marketplace) | Official Peon registry with CI validation |
| [MorphStack/peon-ping](https://github.com/MorphStack/peon-ping) | Reference Peon: ping a device |
| [MorphStack/castle-peon-add-remote-windows-pc](https://github.com/MorphStack/castle-peon-add-remote-windows-pc) | Peon: enroll a Windows PC via WinRM |
| [MorphStack/castle-peon-add-device](https://github.com/MorphStack/castle-peon-add-device) | Peon: add a device (early stage) |

## Quick Start

```bash
# Docker (recommended)
cp .env.example .env
docker compose up -d
# API → http://localhost:5000
# Web  → http://localhost:8080

# Without Docker (.NET 9 SDK required)
dotnet run --project CastleOps.Api
dotnet run --project CastleOps.Web   # separate terminal
```

## Development Notes

- **Database**: SQLite, auto-created via `EnsureCreated()` in Development mode. File at `%LOCALAPPDATA%/CastleOps/app.db` (Windows) or `/var/log/castleops/app.db` (Linux).
- **CORS**: Currently `AllowAnyOrigin` — tighten before any network-exposed deployment.
- **Logging**: Serilog, rolling daily to `castle-api-log-{date}.txt`, 14-day retention.
- **OpenAPI**: Available at `/openapi` only in Development mode (`ASPNETCORE_ENVIRONMENT=Development`).
- **Auth**: Client agents authenticate via Bearer token (hashed server-side). No user-facing auth implemented yet.

## Key Commands

```bash
# Restore and build
dotnet restore CastleOps.sln
dotnet build CastleOps.sln

# Add EF migration
cd CastleOps.Api
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Docs

Full documentation is in [`/docs`](./docs/README.md).
