# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY CastleOps.sln ./
COPY CastleOps.Web/CastleOps.Web.csproj CastleOps.Web/
COPY CastleOps.Core/CastleOps.Core.csproj CastleOps.Core/

# Restore dependencies
RUN dotnet restore CastleOps.Web/CastleOps.Web.csproj

# Copy the rest of the source code
COPY CastleOps.Web/ CastleOps.Web/
COPY CastleOps.Core/ CastleOps.Core/

# Build and publish the application
WORKDIR /src/CastleOps.Web
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - use nginx to serve the static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy the published Blazor WASM app
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx config
COPY CastleOps.Web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
