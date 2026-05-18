# CastleOps Documentation

CastleOps is a self-hosted home device management platform — like a simplified Active Directory for families. Manage PCs and Macs across your home network, run automation scripts (Peons) on devices, and browse a community marketplace of scripts.

## Contents

| Document | Description |
|----------|-------------|
| [Architecture](./architecture.md) | System components, data flow, and design decisions |
| [Getting Started](./getting-started.md) | Running CastleOps locally and in production |
| [API Reference](./api-reference.md) | Complete REST API endpoint documentation |
| [Client Agent](./client-agent.md) | The Go agent that runs on managed machines |
| [Peons](./peons.md) | Automation scripts: what they are, peon.yml spec, how to write them |
| [Marketplace](./marketplace.md) | Contributing Peons to the community marketplace |

## Project Status

This project is a work in progress and has not been publicly released.

- **Backend API**: Core functionality implemented (clients, devices, peons, marketplace)
- **Web UI**: Blazor WebAssembly frontend with Dashboard, Devices, Marketplace, and Peons pages
- **Client Agent**: Core phases complete; ~56% production-ready (4 failing API tests remain)
- **Marketplace**: Validation CI in place; 1 Peon published
