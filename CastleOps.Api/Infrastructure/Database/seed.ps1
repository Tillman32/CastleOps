# Script to seed database for development using the API
# Usage: .\seed.ps1

# Create Device
Invoke-RestMethod -Uri "http://localhost:5001/api/devices/register" -Method Post -Body (@{
    name = "Joe's Desktop"
    operatingSystem = "Windows 11"
    ipAddress = "192.168.1.100"
} | ConvertTo-Json) -ContentType "application/json"

# Create Peon Marketplace Item
Invoke-RestMethod -Uri "http://localhost:5001/api/peons/marketplace" -Method Post -Body (@{
    name = "Ping"
    gitUrl = "https://github.com/morphstack/peon-ping"
    entryPoint = "ping.ps1"
    scriptType = "PowerShell"
    description = "A classic ping/pong Peon to check connectivity to remote devices."
    author = "MorphStack"
    version = "0.0.1"
    tags = @("healthcheck", "monitoring", "ping")
} | ConvertTo-Json) -ContentType "application/json"