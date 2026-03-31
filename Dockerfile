# =============================================================================
# Stage 1: BASE RUNTIME IMAGE
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# aspnet:10.0 is Debian-based — use groupadd/useradd (not Alpine's addgroup/adduser)
RUN groupadd --system --gid 1001 appgroup \
    && useradd --system --uid 1001 --gid appgroup --no-create-home appuser

WORKDIR /app
EXPOSE 8080

# =============================================================================
# Stage 2: BUILD — restore + compile
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# -----------------------------------------------------------------------------
# Copy ONLY .csproj files first for layer cache optimization.
# Docker caches the restore layer until a .csproj changes.
# Source-only changes skip restore entirely — saves 30-60s per build.
# -----------------------------------------------------------------------------

# SharedKernel
COPY src/BuildingBlocks/TicketingSystem.SharedKernel/TicketingSystem.SharedKernel.csproj \
     src/BuildingBlocks/TicketingSystem.SharedKernel/

# Host
COPY src/Host/TicketingSystem.Api/TicketingSystem.Api.csproj \
     src/Host/TicketingSystem.Api/

# Access Module
COPY src/Modules/Access/TicketingSystem.Modules.Access.Domain/TicketingSystem.Modules.Access.Domain.csproj \
     src/Modules/Access/TicketingSystem.Modules.Access.Domain/
COPY src/Modules/Access/TicketingSystem.Modules.Access.Application/TicketingSystem.Modules.Access.Application.csproj \
     src/Modules/Access/TicketingSystem.Modules.Access.Application/
COPY src/Modules/Access/TicketingSystem.Modules.Access.Infrastructure/TicketingSystem.Modules.Access.Infrastructure.csproj \
     src/Modules/Access/TicketingSystem.Modules.Access.Infrastructure/
COPY src/Modules/Access/TicketingSystem.Modules.Access.Api/TicketingSystem.Modules.Access.Api.csproj \
     src/Modules/Access/TicketingSystem.Modules.Access.Api/

# Catalog Module
COPY src/Modules/Catalog/TicketingSystem.Modules.Catalog.Domain/TicketingSystem.Modules.Catalog.Domain.csproj \
     src/Modules/Catalog/TicketingSystem.Modules.Catalog.Domain/
COPY src/Modules/Catalog/TicketingSystem.Modules.Catalog.Application/TicketingSystem.Modules.Catalog.Application.csproj \
     src/Modules/Catalog/TicketingSystem.Modules.Catalog.Application/
COPY src/Modules/Catalog/TicketingSystem.Modules.Catalog.Infrastructure/TicketingSystem.Modules.Catalog.Infrastructure.csproj \
     src/Modules/Catalog/TicketingSystem.Modules.Catalog.Infrastructure/
COPY src/Modules/Catalog/TicketingSystem.Modules.Catalog.Api/TicketingSystem.Modules.Catalog.Api.csproj \
     src/Modules/Catalog/TicketingSystem.Modules.Catalog.Api/

# Finance Module
COPY src/Modules/Finance/TicketingSystem.Modules.Finance.Domain/TicketingSystem.Modules.Finance.Domain.csproj \
     src/Modules/Finance/TicketingSystem.Modules.Finance.Domain/
COPY src/Modules/Finance/TicketingSystem.Modules.Finance.Application/TicketingSystem.Modules.Finance.Application.csproj \
     src/Modules/Finance/TicketingSystem.Modules.Finance.Application/
COPY src/Modules/Finance/TicketingSystem.Modules.Finance.Infrastructure/TicketingSystem.Modules.Finance.Infrastructure.csproj \
     src/Modules/Finance/TicketingSystem.Modules.Finance.Infrastructure/
COPY src/Modules/Finance/TicketingSystem.Modules.Finance.Api/TicketingSystem.Modules.Finance.Api.csproj \
     src/Modules/Finance/TicketingSystem.Modules.Finance.Api/

# Fulfillment Module
COPY src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Domain/TicketingSystem.Modules.Fulfillment.Domain.csproj \
     src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Domain/
COPY src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Application/TicketingSystem.Modules.Fulfillment.Application.csproj \
     src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Application/
COPY src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Infrastructure/TicketingSystem.Modules.Fulfillment.Infrastructure.csproj \
     src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Infrastructure/
COPY src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Api/TicketingSystem.Modules.Fulfillment.Api.csproj \
     src/Modules/Fulfillment/TicketingSystem.Modules.Fulfillment.Api/

# Identity Module
COPY src/Modules/Identity/TicketingSystem.Modules.Identity.Domain/TicketingSystem.Modules.Identity.Domain.csproj \
     src/Modules/Identity/TicketingSystem.Modules.Identity.Domain/
COPY src/Modules/Identity/TicketingSystem.Modules.Identity.Application/TicketingSystem.Modules.Identity.Application.csproj \
     src/Modules/Identity/TicketingSystem.Modules.Identity.Application/
COPY src/Modules/Identity/TicketingSystem.Modules.Identity.Infrastructure/TicketingSystem.Modules.Identity.Infrastructure.csproj \
     src/Modules/Identity/TicketingSystem.Modules.Identity.Infrastructure/
COPY src/Modules/Identity/TicketingSystem.Modules.Identity.Api/TicketingSystem.Modules.Identity.Api.csproj \
     src/Modules/Identity/TicketingSystem.Modules.Identity.Api/

# Sales Module
COPY src/Modules/Sales/TicketingSystem.Modules.Sales.Domain/TicketingSystem.Modules.Sales.Domain.csproj \
     src/Modules/Sales/TicketingSystem.Modules.Sales.Domain/
COPY src/Modules/Sales/TicketingSystem.Modules.Sales.Application/TicketingSystem.Modules.Sales.Application.csproj \
     src/Modules/Sales/TicketingSystem.Modules.Sales.Application/
COPY src/Modules/Sales/TicketingSystem.Modules.Sales.Infrastructure/TicketingSystem.Modules.Sales.Infrastructure.csproj \
     src/Modules/Sales/TicketingSystem.Modules.Sales.Infrastructure/
COPY src/Modules/Sales/TicketingSystem.Modules.Sales.Api/TicketingSystem.Modules.Sales.Api.csproj \
     src/Modules/Sales/TicketingSystem.Modules.Sales.Api/

# NuGet.config — clears Windows fallback package paths that break Linux builds
COPY NuGet.config .

# Restore — cached until any .csproj or NuGet.config changes
RUN dotnet restore src/Host/TicketingSystem.Api/TicketingSystem.Api.csproj

# Copy all source (tests/ excluded via .dockerignore)
COPY src/ src/

# =============================================================================
# Stage 3: RELEASER — publish to output folder
# NOTE: Named "releaser" not "publish" — "publish" is ambiguous to BuildKit
# and causes it to attempt pulling docker.io/library/publish:latest
# =============================================================================
FROM build AS releaser

RUN dotnet publish src/Host/TicketingSystem.Api/TicketingSystem.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# =============================================================================
# Stage 4: FINAL RUNTIME IMAGE
# =============================================================================
FROM base AS final

WORKDIR /app

# Copy only the published output — no SDK, no source code, no build tools
COPY --from=releaser /app/publish .

# Switch to non-root user
USER appuser

# Health check — used by Docker Compose and Kubernetes to gate traffic
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1

# Secrets injected at runtime via docker-compose env vars — never baked in
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "TicketingSystem.Api.dll"]