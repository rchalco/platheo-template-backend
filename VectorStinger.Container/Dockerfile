# Dockerfile para VectorStinger.Api.Service (.NET 9)
# Soporta múltiples ambientes: Development, Stage, Production
# Optimizado para tamaño mínimo de imagen

# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias (layer caching)
COPY ["VectorStinger.Api.Service/VectorStinger.Api.Service.csproj", "VectorStinger.Api.Service/"]
COPY ["VectorStinger.Application/VectorStinger.Application.csproj", "VectorStinger.Application/"]
COPY ["VectorStinger.Core/VectorStinger.Core.csproj", "VectorStinger.Core/"]
COPY ["VectorStinger.Foundation.Abstractions/VectorStinger.Foundation.Abstractions.csproj", "VectorStinger.Foundation.Abstractions/"]
COPY ["VectorStinger.Foundation.Factory/VectorStinger.Foundation.Factory.csproj", "VectorStinger.Foundation.Factory/"]
COPY ["VectorStinger.Foundation.Utilities/VectorStinger.Foundation.Utilities.csproj", "VectorStinger.Foundation.Utilities/"]
COPY ["VectorStinger.Host.ServiceDefaults/VectorStinger.Host.ServiceDefaults.csproj", "VectorStinger.Host.ServiceDefaults/"]
COPY ["VectorStinger.Infrastructure.Bucket/VectorStinger.Infrastructure.Bucket.csproj", "VectorStinger.Infrastructure.Bucket/"]
COPY ["VectorStinger.Infrastructure.DataAccess/VectorStinger.Infrastructure.DataAccess.csproj", "VectorStinger.Infrastructure.DataAccess/"]
COPY ["VectorStinger.Infrastructure.OAuth/VectorStinger.Infrastructure.OAuth.csproj", "VectorStinger.Infrastructure.OAuth/"]
COPY ["VectorStinger.Modules.Security/VectorStinger.Modules.Security.csproj", "VectorStinger.Modules.Security/"]
COPY ["VectorSinger.Modules.WebTemplate/VectorSinger.Modules.WebTemplate.csproj", "VectorSinger.Modules.WebTemplate/"]

# Restaurar dependencias
RUN dotnet restore "VectorStinger.Api.Service/VectorStinger.Api.Service.csproj" \
    --runtime linux-musl-x64

# Copiar todo el código fuente
COPY . .

# Build y publish en un solo paso (reduce layers)
WORKDIR "/src/VectorStinger.Api.Service"
RUN dotnet publish "VectorStinger.Api.Service.csproj" \
    -c Release \
    -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained false \
    --no-restore \
    /p:PublishTrimmed=false \
    /p:PublishSingleFile=false

# ===== Runtime Stage (Alpine para imagen más pequeña) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final

# Metadata
LABEL maintainer="Platheo Team"
LABEL description="Platheo Templates API Service"
LABEL version="1.0"

# Instalar dependencias necesarias en una sola capa
RUN apk add --no-cache \
    curl \
    icu-libs \
    tzdata && \
    rm -rf /var/cache/apk/*

# Configurar timezone y locale
ENV TZ=America/New_York \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

WORKDIR /app

# Crear directorio para imágenes con permisos
RUN mkdir -p /app/images && chmod 755 /app/images

# Copiar solo los archivos publicados (más pequeño que copiar todo el build)
COPY --from=build /app/publish .

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_HTTP_PORTS=8080 \
    DB_PASSWORD="" \
    PAYMENT_SECRET_KEY="" \
    APPINSIGHTS_CONNECTION_STRING=""

# Exponer puerto
EXPOSE 8080

# Health check optimizado
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -sf http://localhost:8080/health || exit 1

# Crear usuario no-root para seguridad
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app

USER appuser

# Punto de entrada
ENTRYPOINT ["dotnet", "VectorStinger.Api.Service.dll"]
