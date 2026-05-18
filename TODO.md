# TODO

Ordered by priority. The first five items block every end-to-end demo.
See [`docs/roadmap.md`](docs/roadmap.md) for the full background and reasoning.

---

## Blocker: Close the Peon Execution Loop

Right now a user cannot trigger a Peon and see it run on a device. Three things are missing:

- [ ] **Link Client → Device.** Add a `ClientId` (nullable FK) to the `Device` model so the server knows which registered agent lives on which device. Add an EF migration. Update `DeviceService` so that when an agent registers, it can be associated with a device (or auto-creates one).

- [ ] **Command dispatch endpoint.** Add `POST /api/v1/devices/{deviceId}/peons/{peonId}/run` (or similar) that creates a `ClientCommand` record targeting the agent on that device. The agent already polls for commands — this is the missing server side.

- [ ] **Fix `Entry` on Peon install.** In `MarketplaceService.InstallMarketplaceItemAsync` the `peonDto.Entry` assignment is commented out. Installed Peons have no entrypoint, so agents can’t know what script to run. Uncomment and wire it from the parsed `peon.yml`.

- [ ] **Peon execution in the Go agent.** The agent polls commands and returns results, but never executes a Peon script. Add logic in `CastleOps.Client` to: download the entrypoint from the Peon’s GitHub repo, inject environment variables from the command payload, run the script (PowerShell / Python / Bash based on `type`), capture exit code and output, and POST the result back.

- [ ] **Fix 4 failing API client tests** in `CastleOps.Client`. These are the only thing blocking the agent from being considered stable.

---

## UI: Make the Dashboard and Pages Useful

- [ ] **Dashboard — remove hardcoded localhost.** `Dashboard.razor` constructs a raw `HttpClient` pointing at `https://localhost:5001`. Replace with the injected `CastleOpsClient` (same pattern as `Marketplace/Index.razor`).

- [ ] **Dashboard — show all stats.** `TotalPeons` and `TotalPeonJobs` are computed but never rendered. Add cards for them alongside the device count.

- [ ] **Build out `Peons/Index.razor`.** Currently a placeholder (195 bytes). Should list installed Peons with their per-device assignments and a "Run" action.

- [ ] **Device detail: Run Peon flow.** `Devices/Details.razor` needs a way to pick a Peon, trigger it (calls the dispatch endpoint above), and show the command result.

---

## API Hygiene

- [ ] **Normalize routes to `api/v1/`.** `ClientsController` uses `api/v1/clients`. `DevicesController`, `PeonsController`, and `MarketplaceController` use the implicit `api/[controller]` pattern. Standardize everything under `api/v1/`.

- [ ] **Replace `Console.WriteLine` with `ILogger` in `DevicesController`.** Three catch blocks use `Console.WriteLine` instead of the injected logger, so errors won’t appear in structured logs.

- [ ] **Pre-check for duplicate Peon install.** `MarketplaceService.InstallMarketplaceItemAsync` relies on the unique DB constraint throwing to detect re-installs. Add an explicit slug lookup before attempting insert.

---

## Peon Schema

- [ ] **Standardize `peon.yml` spec.** `peon-ping` uses `entry:` with a `peon:` wrapper and no description/author. `castle-peon-add-remote-windows-pc` uses a flat schema with `entryPoint:` (camelCase). The marketplace docs specify `entrypoint:` (lowercase). Pick one and enforce it in the marketplace validator.

- [ ] **Add `castle-peon-add-remote-windows-pc` to the marketplace registry** (`config/peon-marketplace.json` in `MorphStack/peon-marketplace`). It’s the most complete Peon but isn’t listed.

- [ ] **Update `peon-ping` to match canonical schema** (add `description`, `author`, `requirements.os`, rename `entry` → `entrypoint`).

---

## Ops / Deployment

- [ ] **Production migration step.** `EnsureCreated()` only runs in Development. Add `dotnet ef database update` to the Docker entrypoint or document it clearly in the deployment guide so a production first-run doesn’t silently have no schema.

- [ ] **Lock down CORS.** `AllowAnyOrigin` is fine for local dev but needs a proper origin allowlist before the server is exposed on a home network.

---

## Medium-term Features

- [ ] User authentication on the Web UI (login page, session management)
- [ ] Role-based access — admin vs. family member
- [ ] Metrics history endpoint — query CPU/memory over time for a device detail graph
- [ ] Persist marketplace items to DB (uncomment `MarketplaceItems` DbSet) so the list survives restarts without a GitHub round-trip
- [ ] Linux systemd service integration in the Go agent
- [ ] Peon execution scheduling (cron-style: run Ping every 5 min)
- [ ] Offline/error notifications when a device goes offline or a Peon fails

---

## Long-term Vision

- [ ] User/login sync across devices (push accounts or SSH keys to managed machines — the core AD-like feature)
- [ ] File sync between family devices
- [ ] LDAP layer — consider building on [LLDAP](https://github.com/lldap/lldap) rather than writing from scratch
- [ ] More Peons: software installs, backups, VPN setup, parental controls
- [ ] Hosted marketplace browser (discover Peons without a running CastleOps instance)
