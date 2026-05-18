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
- **Docker** — `docker-compose.yml` with both services, shared network, volume mounts (`./data` → `/app/data`, `./logs` → `/app/logs`).
- **Web UI scaffolding** — Blazor WebAssembly with MudBlazor; pages exist for Dashboard, Devices (Index/Add/Details), Marketplace (Index/Item), Peons (Index stub).
- **Command dispatch** — `POST /api/v1/devices/{deviceId}/peons/{peonId}/run` creates a `ClientCommand` for the registered agent on that device.
- **Peon execution** — Go agent downloads the entrypoint script, injects env vars, executes with correct interpreter, and posts results back.

### Bugs & Gaps Found

#### Critical — blocks end-to-end flow

~~1. **No Client ↔ Device relationship.**~~ Fixed: `Device.ClientId` nullable FK; auto-linked by hostname on registration.

~~2. **No command dispatch endpoint.**~~ Fixed: `POST /api/v1/devices/{deviceId}/peons/{peonId}/run`.

~~3. **`Entry` field not set on install.**~~ Fixed: `MarketplaceService.InstallMarketplaceItemAsync` sets `Entry` from `PeonYamlDto.ResolvedEntry`.

~~4. **No Peon script execution in the agent.**~~ Fixed: `internal/agent/peon_executor.go` in `CastleOps.Client`.

#### Significant — degraded functionality

~~5. **API versioning inconsistency.**~~ Fixed: all controllers now use `api/v1/`.

~~6. **Dashboard hardcodes `https://localhost:5001`.**~~ Fixed: Dashboard uses injected `CastleOpsClient`.

7. **`MarketplaceItems` table commented out.** `// public DbSet<MarketplaceItem> MarketplaceItems { get; set; }` — marketplace data only lives in memory cache and is re-fetched from GitHub on restart. Not a blocker but means no persistence or audit history.

8. **`PeonService.AssignPeonToDeviceAsync` is never called.** `DevicesController.HirePeonAsync` calls `_deviceService.HirePeonAsync`, which is a different method on `DeviceService`. The correct assignment logic (copies defaults into `PeonConfig`) lives in `PeonService` and isn't connected.

9. **No duplicate-install check.** `MarketplaceService.InstallMarketplaceItemAsync` relies on the database's unique index on `Peon.Slug` throwing an exception to detect re-installation, rather than a pre-check. The exception is caught in the controller as a `409 Conflict` — this works but is fragile.

#### Minor / Polish

10. **Peons/Index.razor is a 195-byte placeholder.** Nothing is rendered.
11. **Production needs manual EF migrations.** `EnsureCreated()` only runs in Development. Production deployments need `dotnet ef database update` — not documented in the Docker setup.
12. **peon.yml schema inconsistency across repos.** `peon-ping` uses `entry:` (no wrapper, no description/author fields). `castle-peon-add-remote-windows-pc` uses a flat schema with `entryPoint:` (camelCase). The marketplace docs specify `entrypoint:` (lowercase) inside a `peon:` wrapper. None of the existing Peons match the canonical spec.

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
- **Peon execution** — `CommandRunPeon` type wired into `CommandHandler`; downloads script from GitHub, injects env vars, executes with correct interpreter, submits result.

### Gaps

1. **4 failing API client tests.** Exact failures not inspected but noted in the README as the primary blocker for production-readiness.
2. **No Linux service integration.** Planned but not started.
3. **Test coverage at 51.8%.** Agent orchestrator and package manager components need coverage.

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

| # | What | Where |
|---|------|-------|
| 1 | Fix 4 failing API client tests | `CastleOps.Client` |
| 2 | Add `castle-peon-add-remote-windows-pc` to marketplace registry | `MorphStack/peon-marketplace` |
| 3 | Standardize `peon.yml` schema across repos | `peon-ping`, `castle-peon-add-remote-windows-pc` |

### Soon — make the UI usable

| # | What | Where |
|---|------|-------|
| 4 | Build out Peons/Index.razor — list installed Peons with their device assignments | `CastleOps.Web/Pages/Peons/` |
| 5 | Command result viewer — surface `ClientCommand.ResultJson` in the UI after a Peon runs | `CastleOps.Web` |
| 6 | Display all stats on Dashboard (Peons, running jobs) | `Dashboard.razor` |

### Medium-term — hardening & family features

| # | What | Notes |
|---|------|-------|
| 7 | User authentication on the Web UI | Sessions, family member accounts; JWT or cookie auth |
| 8 | Role-based access (admin vs. family member) | Admins manage devices; members see status |
| 9 | Add metrics query endpoint — fetch historical CPU/memory for a device | Powers a device detail graph |
| 10 | Persist marketplace items to DB (uncomment `MarketplaceItems` DbSet) | Enables audit history and offline browsing |
| 11 | Production migration strategy — run `dotnet ef database update` in Docker entrypoint | Ops concern |
| 12 | Add Linux systemd service integration to the Go agent | Opens up Linux servers/NAS boxes as managed devices |

### Long-term — the vision

| # | What | Notes |
|---|------|-------|
| 13 | User/login sync across devices | The core "Active Directory" feature — push user accounts or SSH keys to managed machines |
| 14 | File sync | Sync home folder or selected files between family devices |
| 15 | LDAP/AD integration (optional layer) | Could use LLDAP (a lightweight LDAP server) rather than building from scratch |
| 16 | More Peons | Software installs, backup scripts, VPN setup, parental controls, etc. |
| 17 | Peon scheduling | Cron-style: run Ping every 5 minutes, run Backup every night |
| 18 | Notifications | Alert when a device goes offline or a Peon fails |
| 19 | Web-based marketplace browser | Hosted UI for discovering Peons without a CastleOps instance |
