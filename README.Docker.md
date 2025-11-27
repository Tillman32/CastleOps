# CastleOps Docker Setup

This project includes Docker support for both the API and Web (Blazor WebAssembly) components.

## Prerequisites

- Docker
- Docker Compose

## Quick Start

### Build and run all services:

```bash
docker-compose up --build
```

### Run in detached mode:

```bash
docker-compose up -d
```

### Stop services:

```bash
docker-compose down
```

## Services

### API Service
- **URL**: http://localhost:5000
- **Container**: castleops-api
- **Framework**: ASP.NET Core 9.0
- **Database**: SQLite (persisted in `./data` volume)
- **Logs**: Available in `./logs` volume

### Web Service
- **URL**: http://localhost:8080
- **Container**: castleops-web
- **Framework**: Blazor WebAssembly
- **Server**: Nginx

## Individual Service Commands

### Build only API:
```bash
docker-compose build castleops-api
```

### Build only Web:
```bash
docker-compose build castleops-web
```

### Run only API:
```bash
docker-compose up castleops-api
```

### Run only Web:
```bash
docker-compose up castleops-web
```

## Logs

View logs for all services:
```bash
docker-compose logs -f
```

View logs for a specific service:
```bash
docker-compose logs -f castleops-api
docker-compose logs -f castleops-web
```

## Volumes

- `./data` - SQLite database files
- `./logs` - Application logs

## Environment Variables

You can override environment variables in `docker-compose.yml` or create a `.env` file:

```env
ASPNETCORE_ENVIRONMENT=Production
```

## Production Deployment

For production, consider:
1. Using a proper database (PostgreSQL, SQL Server)
2. Setting up proper SSL/TLS certificates
3. Configuring environment-specific settings
4. Using Docker secrets for sensitive data
5. Setting up health checks and monitoring

## Troubleshooting

### Rebuild from scratch:
```bash
docker-compose down -v
docker-compose build --no-cache
docker-compose up
```

### Check container status:
```bash
docker-compose ps
```

### Access container shell:
```bash
docker exec -it castleops-api /bin/bash
docker exec -it castleops-web /bin/sh
```
