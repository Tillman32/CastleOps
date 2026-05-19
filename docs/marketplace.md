# Peon Marketplace

The Peon Marketplace is a community registry of reusable Peons hosted at [MorphStack/peon-marketplace](https://github.com/MorphStack/peon-marketplace).

## How It Works

- The registry lives in `config/peon-marketplace.json` in that repository
- A TypeScript validator checks every entry via GitHub Actions CI on each PR
- CastleOps fetches this registry from GitHub at runtime to populate the Marketplace page
- Users browse and install Peons directly from their CastleOps instance

## Registry Format

```json
{
  "peons": [
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
}
```

## Contributing a Peon

### 1. Build Your Peon Repository

Follow the structure in [Peons](./peons.md). You need at minimum:

```
your-peon-repo/
├── peon.yml      # Required
├── README.md
└── script.ps1    # (or .py, .sh, etc.)
```

### 2. Validate Your peon.yml

The marketplace validator checks:

| Rule | Details |
|------|---------|
| `name` and `author` | Required, ≤ 65 characters |
| `giturl` | Must be a reachable GitHub URL (HTTP 2xx/3xx) |
| `peon.yml` | Must exist at repo root and be valid YAML |
| Required fields | `version`, `description`, `author`, `tags`, `giturl`, `type`, `entrypoint` |

Run validation locally:

```bash
git clone https://github.com/MorphStack/peon-marketplace.git
cd peon-marketplace
npm install
npm run validate-url https://github.com/your-username/your-peon-repo
```

### 3. Submit to the Marketplace

1. Fork [MorphStack/peon-marketplace](https://github.com/MorphStack/peon-marketplace)
2. Add your entry to `config/peon-marketplace.json`
3. Open a Pull Request
4. The CI validation workflow runs automatically
5. A maintainer reviews and merges

### Review Criteria

Beyond automated checks, reviewers look for:

- A clear, specific description
- Correct OS targeting
- No hardcoded secrets or credentials
- Idempotent behavior where possible
- A useful README in the Peon repository

## Supported Script Types

| Type | `type` value | Notes |
|------|-------------|-------|
| PowerShell | `powershell` | Primary support; Windows and macOS |
| Python | `python` | Requires Python on the target device |
| Bash | `bash` | macOS and Linux |
| Node.js | `nodejs` | Requires Node.js on the target device |

## Current Registry

| Name | Author | OS | Type |
|------|--------|----|----- |
| Ping Device | CastleOps | Windows | PowerShell |
