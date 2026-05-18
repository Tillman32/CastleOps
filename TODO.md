# TODO

Ordered by priority. See [`docs/roadmap.md`](docs/roadmap.md) for full background.

---

## Blocker: Close the Peon Execution Loop

- [x] **Link Client → Device.** `Device.ClientId` nullable FK added. `ClientService.RegisterClientAsync` auto-links by hostname. Manual link via `POST api/v1/devices/{id}/link-client/{clientId}`.

- [x] **Command dispatch endpoint.** `POST api/v1/devices/{deviceId}/peons/{peonId}/run` queues a `run_peon` `ClientCommand`. The agent picks it up on its next poll.

- [x] **Fix `Entry` on Peon install.** `MarketplaceService.InstallMarketplaceItemAsync` now sets `Entry` from the parsed `peon.yml` via `PeonYamlDto.ResolvedEntry`.

- [x] **Peon execution in the Go agent.** `CommandRunPeon` type and `RunPeonPayload` added to `internal/api/models.go`. `RunPeonExecutor` added to `internal/api/commands.go`. New `internal/agent/peon_executor.go` downloads the entry script from the GitHub raw URL, injects per-device env vars, and runs it with the correct interpreter (powershell/python/bash). `CommandHandler` is now wired into `agent.go` — `startSubsystems` calls `initCommandHandler`, `pollCommands` dispatches via `HandleCommands`, and `Stop` drains in-flight commands before shutdown.

- [ ] **Fix 4 failing API client tests** in `CastleOps.Client`.

---

## UI: Make the Dashboard and Pages Useful

- [x] **Dashboard — remove hardcoded localhost.** `Dashboard.razor` now uses injected `CastleOpsClient`. API base URL is configurable via `wwwroot/appsettings.json`.

- [x] **Dashboard — show all stats.** Dashboard renders Devices, Peons, and Peon Jobs stat cards.

- [x] **Build out `Peons/Index.razor`.** Lists installed Peons in a table (name, type, author, description).

- [x] **Device detail: Run Peon flow.** `Devices/Details.razor` shows hired peons with a Run button and available peons with a Hire button. Run dispatches `POST api/v1/devices/{id}/peons/{peonId}/run`.

- [x] **Build `Devices/Add.razor`.** Displays platform-specific install scripts (Windows PowerShell / macOS bash) with copy-to-clipboard via `IJSRuntime`. Reads `ApiBaseUrl` from `IConfiguration`. Uses `MudTabs` for platform selection.

- [x] **Clean up `Marketplace/Index.razor`.** Removed unused `@using Newtonsoft.Json` and `@using Newtonsoft.Json.Linq` imports. Removed `Console.WriteLine` debug call.

---

## API / Data Hygiene

- [x] **Normalize routes to `api/v1/`.** All four controllers now use `api/v1/`.

- [x] **Replace `Console.WriteLine` with `ILogger`.** Fixed in `DeviceService` and `DevicesController`.

- [x] **Pre-check for duplicate Peon install.** `MarketplaceService` now calls `GetPeonBySlugAsync` before insert.

- [x] **`DeviceDTO.PeonConfigs` leaks the EF model.** Fixed: `DeviceDTO.PeonConfigs` is now `List<PeonConfigDTO>`. `DeviceService.MapToDTO` explicitly projects each `PeonConfig` entity to `PeonConfigDTO`.

---

## Peon Schema

- [ ] **Standardize `peon.yml` spec.** `peon-ping` uses `entry:` inside a `peon:` wrapper; `castle-peon-add-remote-windows-pc` uses a flat schema with `entryPoint:`. The API now handles both via `PeonYamlDto.ResolvedEntry`, but the Peon repos should be updated to use a consistent canonical format.

- [ ] **Add `castle-peon-add-remote-windows-pc` to the marketplace registry** (`config/peon-marketplace.json` in `MorphStack/peon-marketplace`).

- [ ] **Update `peon-ping` to match canonical schema** (add `description`, `author`, `requirements.os`, rename `entry` → `entrypoint`).

---

## Ops / Deployment

- [ ] **Production migration step.** `EnsureCreated()` only runs in Development. Add `dotnet ef database update` to the Docker entrypoint or deployment docs.

- [ ] **Lock down CORS.** `AllowAnyOrigin` needs a proper origin allowlist before the server is network-exposed.

- [ ] **`wwwroot/appsettings.json` for production.** The new `ApiBaseUrl` setting defaults to `http://localhost:5000/`. Document how to override it (environment-specific `appsettings.Production.json` or build-time substitution in Docker).

---

## Medium-term Features

- [ ] User authentication on the Web UI (login page, session management)
- [ ] Role-based access — admin vs. family member
- [ ] Metrics history endpoint — query CPU/memory over time for a device detail graph
- [ ] Persist marketplace items to DB (uncomment `MarketplaceItems` DbSet)
- [ ] Linux systemd service integration in the Go agent
- [ ] Peon execution scheduling (cron-style: run Ping every 5 min)
- [ ] Offline/error notifications when a device goes offline or a Peon fails
- [ ] Command result viewer — surface `ClientCommand.ResultJson` in the UI after a Peon runs

---

## Long-term Vision

- [ ] User/login sync across devices (push accounts or SSH keys to managed machines)
- [ ] File sync between family devices
- [ ] LDAP layer — consider [LLDAP](https://github.com/lldap/lldap) rather than writing from scratch
- [ ] More Peons: software installs, backups, VPN setup, parental controls
- [ ] Hosted marketplace browser
