# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY CastleOps.sln ./
COPY CastleOps.Api/CastleOps.Api.csproj CastleOps.Api/
COPY CastleOps.Core/CastleOps.Core.csproj CastleOps.Core/

# Restore dependencies
RUN dotnet restore CastleOps.Api/CastleOps.Api.csproj

# Copy the rest of the source code
COPY CastleOps.Api/ CastleOps.Api/
COPY CastleOps.Core/ CastleOps.Core/

# Build the application
WORKDIR /src/CastleOps.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CastleOps.Api.dll"]
