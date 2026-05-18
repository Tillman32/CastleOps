# Roadmap & Current State Review

This document captures what is actually working, what is stubbed or broken, and what is needed next — across all CastleOps repositories.

---

## Existing Plans

No top-level `plan.md` exists in this repo. The client agent's roadmap is embedded in [CastleOps.Client/README.md](https://github.com/Tillman32/CastleOps.Client/blob/main/README.md#roadmap) and tracks phases 1–5 as complete, phase 7 (production readiness) as in-progress.

---

## CastleOps (Main Server)

### What Is Working

- **ClientService** — complete: registration with token hashing, heartbeat processing, metrics ingestion, command polling, command result storage. Uses constant-time token comparison (timing attack safe).
- **DeviceService** — basic CRUD, plus `HirePeonAsync` and `ConfigurePeonAsync`.
- **PeonService** — CRUD, `AssignPeonToDeviceAsync` (copies default env into a per-device `PeonConfig`).
- **MarketplaceService** — fetches `config/peon-marketplace.json` from GitHub, parses Peons, memory-caches results, reads `peon.yml` from each Peon's repo on install.
- **DatabaseContext** — proper EF Core config: composite PK on `PeonConfigs`, JSON serialization of `Dictionary<string,string>` fields, compound indexes on `ClientCommands` and `ClientMetrics`.
- **Docker** — `docker-compose.yml` with both services, shared network, volume mounts.
- **Web UI scaffolding** — Blazor Server with MudBlazor; pages exist for Dashboard, Devices (Index/Add/Details), Marketplace (Index/Item), Peons (Index stub).

### Bugs & Gaps Found

#### Critical — blocks end-to-end flow

1. **No Client ↔ Device relationship.** `Client` (the Go agent) and `Device` are separate, unrelated records. There is no FK or join between them, so it's impossible to know which agent maps to which device, or to dispatch a command to "the agent on device X".

2. **No command dispatch endpoint.** `ClientCommand` records are consumed by agents polling `/api/v1/clients/{id}/commands`, but there is no API endpoint to *create* a command. The path from "user clicks Run Peon on a device" through the Web UI → API → agent is not wired up.

3. **`Entry` field not set on install.** In `MarketplaceService.InstallMarketplaceItemAsync`, `peonDto.Entry` is commented out. Installed Peons have no entrypoint, so agents wouldn't know what script to run.

#### Significant — degraded functionality

4. **API versioning inconsistency.** `ClientsController` uses `api/v1/clients`. `DevicesController`, `PeonsController`, and `MarketplaceController` use the implicit `api/[controller]` pattern (`api/Devices`, `api/Peons`, `api/Marketplace`). These should all be on `api/v1/`.

5. **Dashboard hardcodes `https://localhost:5001`.** `Dashboard.razor` constructs an `HttpClient` directly and calls a hardcoded URL. This breaks in Docker or any non-default setup. Should use the injected `CastleOpsClient` (as the Marketplace page does).

6. **`MarketplaceItems` table commented out.** `// public DbSet<MarketplaceItem> MarketplaceItems { get; set; }` — marketplace data only lives in memory cache and is re-fetched from GitHub on restart. Not a blocker but means no persistence or audit history.

7. **`DevicesController` uses `Console.WriteLine` for errors** instead of the injected `ILogger`. Errors won't appear in structured logs.

8. **`PeonService.AssignPeonToDeviceAsync` is never called.** `DevicesController.HirePeonAsync` calls `_deviceService.HirePeonAsync`, which is a different method on `DeviceService`. The correct assignment logic (copies defaults into `PeonConfig`) lives in `PeonService` and isn't connected.

9. **No duplicate-install check.** `MarketplaceService.InstallMarketplaceItemAsync` relies on the database's unique index on `Peon.Slug` throwing an exception to detect re-installation, rather than a pre-check. The exception is caught in the controller as a `409 Conflict` — this works but is fragile.

#### Minor / Polish

10. **Peons/Index.razor is a 195-byte placeholder.** Nothing is rendered.
11. **Dashboard.razor shows only device count.** `TotalPeons` and `TotalPeonJobs` are calculated but never displayed in the template.
12. **Production needs manual EF migrations.** `EnsureCreated()` only runs in Development. Production deployments need `dotnet ef database update` — not documented in the Docker setup.
13. **peon.yml schema inconsistency across repos.** `peon-ping` uses `entry:` (no wrapper, no description/author fields). `castle-peon-add-remote-windows-pc` uses a flat schema with `entryPoint:` (camelCase). The marketplace docs specify `entrypoint:` (lowercase) inside a `peon:` wrapper. None of the existing Peons match the canonical spec.

---

## CastleOps.Client (Go Agent)

### What Is Working

All core phases are implemented:
- Agent orchestrator (heartbeat loop, metrics loop, command poll loop)
- API client with retry/backoff
- SQLite cache with offline queue
- System metrics (CPU, memory, disk, network via gopsutil)
- Package manager abstraction (Homebrew, Chocolatey)
- macOS launchd and Windows SCM service integration
- Install/uninstall scripts for both platforms

### Gaps

1. **4 failing API client tests.** Exact failures not inspected but noted in the README as the primary blocker for production-readiness.
2. **No Peon script execution.** The agent polls for commands and returns results, but there is no code to download and execute a Peon script from a GitHub URL. This is the other half of the critical gap listed above — the server can't dispatch and the agent can't execute.
3. **No Linux service integration.** Planned but not started.
4. **Test coverage at 51.8%.** Agent orchestrator and package manager components need coverage.

---

## Peon Marketplace

### What Is Working

- TypeScript validator with GitHub Actions CI.
- Registry JSON with 1 published Peon (Ping Device).
- `npm run validate-url <github-url>` for local testing.

### Gaps

1. **Only 1 Peon published.** The marketplace has minimal content.
2. **`peon-ping` uses a different schema** (`entry:` inside a `peon:` wrapper, no `description`, `author`, or `requirements` fields). The validator should enforce the full canonical schema.
3. **`castle-peon-add-remote-windows-pc` is not in the marketplace registry** despite being the most feature-complete Peon.
4. **No web-based browser UI** (listed on the peon-marketplace roadmap).

---

## Prioritized Next Steps

### Now — unblock the core loop

These are needed before any end-to-end demo is possible:

| # | What | Where |
|---|------|-------|
| 1 | Add `ClientId` FK to `Device` (or create a `DeviceClient` join table) so agents map to devices | `CastleOps.Core/Models/Device.cs`, migration |
| 2 | Add `POST /api/v1/devices/{deviceId}/commands` endpoint to dispatch a `ClientCommand` to the agent on that device | new controller action + `DeviceService` or `ClientService` |
| 3 | Fix `Entry` field in `MarketplaceService.InstallMarketplaceItemAsync` | `MarketplaceService.cs:~60` |
| 4 | Implement Peon script execution in the Go agent (download entrypoint from GitHub, inject env vars, run, return output) | `CastleOps.Client/internal/agent` |
| 5 | Fix 4 failing API client tests | `CastleOps.Client` |

### Soon — make the UI usable

| # | What | Where |
|---|------|-------|
| 6 | Fix Dashboard.razor hardcoded localhost; use `CastleOpsClient` injectable | `CastleOps.Web/Pages/Dashboard.razor` |
| 7 | Build out Peons/Index.razor — list installed Peons with their device assignments | `CastleOps.Web/Pages/Peons/` |
| 8 | Add "Run Peon" button flow in the Devices/Details page — creates a command, shows result | `CastleOps.Web/Pages/Devices/Details.razor` |
| 9 | Display all stats on Dashboard (Peons, running jobs) | `Dashboard.razor` |
| 10 | Normalize all API routes to `api/v1/` | All controllers |

### Medium-term — hardening & family features

| # | What | Notes |
|---|------|-------|
| 11 | User authentication on the Web UI | Sessions, family member accounts; JWT or cookie auth |
| 12 | Role-based access (admin vs. family member) | Admins manage devices; members see status |
| 13 | Replace `Console.WriteLine` with `ILogger` in DevicesController | Minor but important for structured logging |
| 14 | Add metrics query endpoint — fetch historical CPU/memory for a device | Powers a device detail graph |
| 15 | Persist marketplace items to DB (uncomment `MarketplaceItems` DbSet) | Enables audit history and offline browsing |
| 16 | Production migration strategy — run `dotnet ef database update` in Docker entrypoint | Ops concern |
| 17 | Add Linux systemd service integration to the Go agent | Opens up Linux servers/NAS boxes as managed devices |

### Long-term — the vision

| # | What | Notes |
|---|------|-------|
| 18 | User/login sync across devices | The core "Active Directory" feature — push user accounts or SSH keys to managed machines |
| 19 | File sync | Sync home folder or selected files between family devices |
| 20 | LDAP/AD integration (optional layer) | Could use LLDAP (a lightweight LDAP server) rather than building from scratch |
| 21 | More Peons | Software installs, backup scripts, VPN setup, parental controls, etc. |
| 22 | Peon scheduling | Cron-style: run Ping every 5 minutes, run Backup every night |
| 23 | Notifications | Alert when a device goes offline or a Peon fails |
| 24 | Web-based marketplace browser | Hosted UI for discovering Peons without a CastleOps instance |
