# ?? Docker Configuration - Platheo Templates API

## ?? Tabla de Contenidos
- [Desarrollo Local](#desarrollo-local)
- [Docker Build & Run](#docker-build--run)
- [Ambientes](#ambientes)
- [Variables de Entorno](#variables-de-entorno)
- [Troubleshooting](#troubleshooting)

---

## ??? Desarrollo Local

### Ejecutar sin Docker (Modo Actual)
```bash
# En Visual Studio: Presiona F5 o Ctrl+F5
# O desde la terminal:
dotnet run --project VectorStinger.Api.Service
```

La API estará disponible en:
- HTTP: `http://localhost:8034`
- Swagger: `http://localhost:8034/swagger`

**No necesitas Docker para desarrollo local. Todo funciona igual que antes.**

---

## ?? Docker Build & Run

### Prerequisitos (Solo si usas Docker)
- Docker Desktop instalado
- Docker Compose (incluido en Docker Desktop)

### Build de la Imagen

#### Development
```bash
docker build -t platheo-api:dev --build-arg ASPNETCORE_ENVIRONMENT=Development .
```

#### Stage
```bash
docker build -t platheo-api:stage --build-arg ASPNETCORE_ENVIRONMENT=Stage .
```

#### Production
```bash
docker build -t platheo-api:prod --build-arg ASPNETCORE_ENVIRONMENT=Production .
```

### Ejecutar Contenedor Manualmente

#### Development
```bash
docker run -d \
  --name platheo-api-dev \
  -p 8034:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e DB_PASSWORD="Iodfa09//)=00" \
  -e PAYMENT_SECRET_KEY="XztyudJDljlloppo" \
  -v $(pwd)/images:/app/images \
  platheo-api:dev
```

#### Stage
```bash
docker run -d \
  --name platheo-api-stage \
  -p 8035:8080 \
  -e ASPNETCORE_ENVIRONMENT=Stage \
  -e DB_PASSWORD="your_stage_password" \
  -e PAYMENT_SECRET_KEY="your_stage_key" \
  -v $(pwd)/images:/app/images \
  platheo-api:stage
```

#### Production
```bash
docker run -d \
  --name platheo-api-prod \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DB_PASSWORD="your_prod_password" \
  -e PAYMENT_SECRET_KEY="your_prod_key" \
  -v $(pwd)/images:/app/images \
  platheo-api:prod
```

---

## ?? Docker Compose

### Configurar Variables de Entorno
```bash
# 1. Copiar el archivo de ejemplo
cp .env.example .env

# 2. Editar .env con tus credenciales
nano .env  # o usa tu editor favorito
```

### Ejecutar con Docker Compose

#### Development (por defecto)
```bash
docker-compose up -d api-dev
```

#### Stage
```bash
docker-compose --profile stage up -d api-stage
```

#### Production
```bash
docker-compose --profile production up -d api-prod
```

### Comandos Útiles

```bash
# Ver logs
docker-compose logs -f api-dev

# Detener contenedores
docker-compose down

# Reconstruir y ejecutar
docker-compose up -d --build

# Ver estado
docker-compose ps

# Entrar al contenedor
docker exec -it platheo-api-dev /bin/bash
```

---

## ?? Ambientes

### Development
- **Puerto**: 8034
- **Logging**: Detallado
- **Telemetry**: Habilitado
- **Base de Datos**: Development
- **Archivo Config**: `appsettings.Development.json`

### Stage
- **Puerto**: 8035
- **Logging**: Información
- **Telemetry**: Habilitado
- **Base de Datos**: Stage
- **Archivo Config**: `appsettings.Stage.json`

### Production
- **Puerto**: 8080
- **Logging**: Warnings solo
- **Telemetry**: Deshabilitado
- **Base de Datos**: Production
- **Archivo Config**: `appsettings.Production.json`

---

## ?? Variables de Entorno

### Variables Requeridas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de ejecución | `Development`, `Stage`, `Production` |
| `DB_PASSWORD` | Password de la base de datos | `********` |
| `PAYMENT_SECRET_KEY` | Secret key del payment bridge | `********` |
| `APPINSIGHTS_CONNECTION_STRING` | Connection string de Application Insights | `InstrumentationKey=...` |

### Variables Opcionales

| Variable | Descripción | Default |
|----------|-------------|---------|
| `ASPNETCORE_URLS` | URL de escucha | `http://+:8080` |
| `ASPNETCORE_HTTP_PORTS` | Puerto HTTP | `8080` |

---

## ??? Troubleshooting

### El contenedor no arranca
```bash
# Ver logs detallados
docker logs platheo-api-dev

# Verificar que las variables de entorno están configuradas
docker exec platheo-api-dev env | grep ASPNETCORE
```

### Error de conexión a base de datos
```bash
# Verificar que DB_PASSWORD está configurado
docker exec platheo-api-dev env | grep DB_PASSWORD

# Verificar conectividad desde el contenedor
docker exec platheo-api-dev ping platheo-srvbd00.database.windows.net
```

### Puerto en uso
```bash
# En Windows
netstat -ano | findstr :8034

# En Linux/Mac
lsof -i :8034

# Cambiar puerto en docker-compose.yml o usar:
docker run -p 8035:8080 ...
```

### Rebuild completo
```bash
# Limpiar todo y reconstruir
docker-compose down -v
docker system prune -a
docker-compose up -d --build
```

### Health Check Fallando
```bash
# Verificar health endpoint manualmente
docker exec platheo-api-dev curl http://localhost:8080/health

# Ver logs de health checks
docker inspect --format='{{json .State.Health}}' platheo-api-dev | jq
```

---

## ?? Notas Importantes

1. **No es necesario usar Docker para desarrollo local**. Puedes seguir trabajando con Visual Studio/Rider como siempre.

2. **Certificados SSL**: Los archivos `.pfx` no se incluyen en la imagen por defecto (por seguridad). Si necesitas HTTPS en el contenedor, descomenta la línea en el Dockerfile.

3. **Volúmenes**: La carpeta `images/` se monta como volumen para persistir las imágenes subidas.

4. **Secrets**: Nunca commitear el archivo `.env` o credenciales reales. Usa Azure Key Vault o secretos de Kubernetes en producción.

5. **Multi-stage Build**: El Dockerfile usa multi-stage build para optimizar el tamaño de la imagen final (~200MB).

---

## ?? Referencias

- [ASP.NET Core en Docker](https://learn.microsoft.com/aspnet/core/host-and-deploy/docker/)
- [Docker Compose](https://docs.docker.com/compose/)
- [.NET 9 Container Images](https://hub.docker.com/_/microsoft-dotnet-aspnet)
