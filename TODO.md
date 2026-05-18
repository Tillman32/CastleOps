# TODO

Ordered by priority. See [`docs/roadmap.md`](docs/roadmap.md) for full background.

---

## Blocker: Close the Peon Execution Loop

- [x] **Link Client → Device.** `Device.ClientId` nullable FK added. `ClientService.RegisterClientAsync` auto-links by hostname. Manual link via `POST api/v1/devices/{id}/link-client/{clientId}`.

- [x] **Command dispatch endpoint.** `POST api/v1/devices/{deviceId}/peons/{peonId}/run` queues a `run_peon` `ClientCommand`. The agent picks it up on its next poll.

- [x] **Fix `Entry` on Peon install.** `MarketplaceService.InstallMarketplaceItemAsync` now sets `Entry` from the parsed `peon.yml` via `PeonYamlDto.ResolvedEntry`.

- [ ] **Peon execution in the Go agent.** The agent polls commands and returns results but does not yet download and execute a Peon script. Needed in `CastleOps.Client/internal/agent` — download entrypoint from GitHub, inject env vars, run script, POST result back.

- [ ] **Fix 4 failing API client tests** in `CastleOps.Client`.

---

## UI: Make the Dashboard and Pages Useful

- [x] **Dashboard — remove hardcoded localhost.** `Dashboard.razor` now uses injected `CastleOpsClient`. API base URL is configurable via `wwwroot/appsettings.json`.

- [x] **Dashboard — show all stats.** Dashboard renders Devices, Peons, and Peon Jobs stat cards.

- [x] **Build out `Peons/Index.razor`.** Lists installed Peons in a table (name, type, author, description).

- [x] **Device detail: Run Peon flow.** `Devices/Details.razor` shows hired peons with a Run button and available peons with a Hire button. Run dispatches `POST api/v1/devices/{id}/peons/{peonId}/run`.

- [ ] **Build `Devices/Add.razor`.** Page body is entirely commented out. Should display a platform-specific install script (from `MorphStack/castle-peon-add-remote-windows-pc`) that the user copies and runs to register a new device.

- [ ] **Clean up `Marketplace/Index.razor`.** Has unused `@using Newtonsoft.Json` and `@using Newtonsoft.Json.Linq` imports. Also logs raw result to `Console.WriteLine` — remove both.

---

## API / Data Hygiene

- [x] **Normalize routes to `api/v1/`.** All four controllers now use `api/v1/`.

- [x] **Replace `Console.WriteLine` with `ILogger`.** Fixed in `DeviceService` and `DevicesController`.

- [x] **Pre-check for duplicate Peon install.** `MarketplaceService` now calls `GetPeonBySlugAsync` before insert.

- [ ] **`DeviceDTO.PeonConfigs` leaks the EF model.** `DeviceDTO.PeonConfigs` is typed `List<PeonConfig>` (the entity class). `DeviceService.MapToDTO` assigns it directly. This exposes EF navigation properties and ties the API contract to the ORM layer. Change to `List<PeonConfigDTO>` and map explicitly in `MapToDTO`.

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
