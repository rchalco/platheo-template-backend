# 🚀 Quick Start Guide

## Desarrollo Local (Sin Docker) - Método Actual

### Visual Studio
1. Abrir solución `Platheo-Templates-API.sln`
2. Presionar `F5` o `Ctrl+F5`
3. El API estará en `http://localhost:8034`

### CLI
```bash
# Desde la raíz del repositorio
dotnet run --project VectorStinger.Api.Service
```

**✅ Con esto se ejecutara el proyecto API.**

---

## Docker (Opcional)

### Prerequisitos
- Docker Desktop instalado

### ⚠️ Nota Importante
Los archivos de Docker están en la carpeta `VectorStinger.Container/`. 

**Asegúrate de ejecutar los comandos desde la raíz del repositorio**, no desde dentro de la carpeta `VectorStinger.Container`.

---

### Opción 1: Docker Compose (Recomendado)

#### Development
```bash
# Desde la raíz del repositorio
docker-compose -f VectorStinger.Container/docker-compose.yml up -d api-dev
```

#### Stage
```bash
docker-compose -f VectorStinger.Container/docker-compose.yml --profile stage up -d api-stage
```

#### Production
```bash
docker-compose -f VectorStinger.Container/docker-compose.yml --profile production up -d api-prod
```

#### Ver logs
```bash
docker-compose -f VectorStinger.Container/docker-compose.yml logs -f api-dev
```

#### Detener
```bash
docker-compose -f VectorStinger.Container/docker-compose.yml down
```

---

### Opción 2: Scripts Helper

Los scripts están en `VectorStinger.Container/`. Ejecútalos desde allí o desde la raíz especificando la ruta.

#### Windows (PowerShell)
```powershell
# Desde la carpeta VectorStinger.Container
cd VectorStinger.Container

# Build
.\docker-helper.ps1 build dev

# Run
.\docker-helper.ps1 run dev

# Logs
.\docker-helper.ps1 logs dev

# Stop
.\docker-helper.ps1 stop dev

# Volver a la raíz
cd ..
```

#### Linux/Mac (Bash)
```bash
# Desde la carpeta VectorStinger.Container
cd VectorStinger.Container

# Dar permisos de ejecución (solo la primera vez)
chmod +x docker-helper.sh

# Build
./docker-helper.sh build dev

# Run
./docker-helper.sh run dev

# Logs
./docker-helper.sh logs dev

# Stop
./docker-helper.sh stop dev

# Volver a la raíz
cd ..
```

---

### Opción 3: Comandos Docker Directos

```bash
# Desde la raíz del repositorio

# Build
docker build -f VectorStinger.Container/Dockerfile -t platheo-api:dev \
  --build-arg ASPNETCORE_ENVIRONMENT=Development .

# Run
docker run -d --name platheo-api-dev -p 8034:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e DB_PASSWORD="tu_password" \
  -v $(pwd)/images:/app/images \
  platheo-api:dev

# Logs
docker logs -f platheo-api-dev

# Stop
docker stop platheo-api-dev

# Remove
docker rm platheo-api-dev
```

---

## 📁 Estructura de Archivos Docker

```
Platheo-Templates-API/
├── VectorStinger.Api.Service/          # Proyecto principal del API
├── VectorStinger.Application/          # Capa de aplicación
├── VectorStinger.Core/                 # Capa de dominio
├── VectorStinger.Container/             # ⭐ Archivos Docker aquí
│   ├── Dockerfile                      # Definición de imagen
│   ├── docker-compose.yml              # Orquestación
│   ├── docker-helper.ps1               # Script Windows
│   ├── docker-helper.sh                # Script Linux/Mac
│   ├── .dockerignore                   # Exclusiones
│   ├── .env.example                    # Variables de entorno
│   ├── DOCKER_README.md                # Documentación detallada
│   └── QUICKSTART.md                   # Esta guía
└── images/                             # Volumen para imágenes
```

---

## 🌐 URLs por Ambiente

| Ambiente | Puerto | URL |
|----------|--------|-----|
| Development | 8034 | http://localhost:8034 |
| Stage | 8035 | http://localhost:8035 |
| Production | 8080 | http://localhost:8080 |

---

## 📊 Swagger UI

- **Development**: http://localhost:8034/swagger
- **Stage**: http://localhost:8035/swagger (si está habilitado)
- **Production**: No disponible

---

## ❤️ Health Check

```bash
# Development
curl http://localhost:8034/health

# Stage
curl http://localhost:8035/health

# Production
curl http://localhost:8080/health
```

---

## 📖 Documentación Completa

Ver `VectorStinger.Container/DOCKER_README.md` para documentación detallada sobre:
- Configuración de variables de entorno
- Despliegue en diferentes ambientes
- Troubleshooting avanzado
- Integración con CI/CD
- Despliegue en Azure

**📦 Para desplegar a Azure Container Registry:**
Ver `VectorStinger.Container/ACR_DEPLOYMENT.md` para instrucciones completas sobre:
- Despliegue por CLI (automatizado)
- Despliegue manual con archivos TAR
- Configuración de Azure Container Apps
- Configuración de AKS (Kubernetes)
- Script automatizado de deployment

---

## 🔧 Troubleshooting

### Puerto en uso
```bash
# Windows
netstat -ano | findstr :8034

# Linux/Mac
lsof -i :8034
```

### Ver logs del contenedor
```bash
docker logs -f platheo-api-dev
```

### Rebuild completo
```bash
# Con docker-compose
docker-compose -f VectorStinger.Container/docker-compose.yml down
docker-compose -f VectorStinger.Container/docker-compose.yml up -d --build

# Manualmente
docker stop platheo-api-dev
docker rm platheo-api-dev
docker build -f VectorStinger.Container/Dockerfile -t platheo-api:dev .
docker run -d --name platheo-api-dev -p 8034:8080 platheo-api:dev
```

### Limpiar todo Docker
```bash
# Detener todos los contenedores de Platheo
docker stop $(docker ps -a -q --filter name=platheo-api)

# Eliminar contenedores
docker rm $(docker ps -a -q --filter name=platheo-api)

# Eliminar imágenes
docker rmi $(docker images -q platheo-api)

# Limpiar sistema completo (usar con precaución)
docker system prune -a
```

### Problemas de permisos en volúmenes (Linux/Mac)
```bash
# Crear carpeta de imágenes con permisos correctos
mkdir -p images
chmod 777 images
```

### Variables de entorno no se cargan
```bash
# Verificar que el archivo .env existe
ls -la VectorStinger.Container/.env

# Copiar desde el ejemplo si no existe
cp VectorStinger.Container/.env.example VectorStinger.Container/.env

# Editar con tus credenciales
nano VectorStinger.Container/.env
```

---

## 💡 Tips Útiles

### Alias para facilitar el uso (opcional)

#### PowerShell (Windows)
```powershell
# Agregar al perfil de PowerShell ($PROFILE)
function Docker-Platheo-Dev { docker-compose -f VectorStinger.Container/docker-compose.yml up -d api-dev }
function Docker-Platheo-Logs { docker-compose -f VectorStinger.Container/docker-compose.yml logs -f api-dev }
function Docker-Platheo-Stop { docker-compose -f VectorStinger.Container/docker-compose.yml down }

# Uso
Docker-Platheo-Dev
Docker-Platheo-Logs
Docker-Platheo-Stop
```

#### Bash (Linux/Mac)
```bash
# Agregar al ~/.bashrc o ~/.zshrc
alias platheo-dev='docker-compose -f VectorStinger.Container/docker-compose.yml up -d api-dev'
alias platheo-logs='docker-compose -f VectorStinger.Container/docker-compose.yml logs -f api-dev'
alias platheo-stop='docker-compose -f VectorStinger.Container/docker-compose.yml down'

# Recargar configuración
source ~/.bashrc  # o source ~/.zshrc

# Uso
platheo-dev
platheo-logs
platheo-stop
```

### Acceder al contenedor
```bash
# Bash
docker exec -it platheo-api-dev /bin/bash

# Ver estructura de archivos
docker exec platheo-api-dev ls -la /app

# Ver configuración activa
docker exec platheo-api-dev cat /app/appsettings.json
```

### Verificar variables de entorno dentro del contenedor
```bash
docker exec platheo-api-dev env | grep ASPNETCORE
```

---

## 🎯 Workflow Recomendado

### Desarrollo Diario (Sin Docker)
```bash
# 1. Abrir Visual Studio
# 2. Presionar F5
# 3. Desarrollar normalmente
# 4. Commit y push cuando termines
```

### Testing con Docker (Ocasional)
```bash
# 1. Desde la raíz del repositorio
cd VectorStinger.Container

# 2. Ejecutar con script helper
.\docker-helper.ps1 run dev  # Windows
./docker-helper.sh run dev   # Linux/Mac

# 3. Probar el API
curl http://localhost:8034/health

# 4. Ver logs si necesitas
docker logs -f platheo-api-dev

# 5. Detener cuando termines
.\docker-helper.ps1 stop dev  # Windows
./docker-helper.sh stop dev   # Linux/Mac
```

### Deploy a Stage/Production
```bash
# 1. Configurar variables de entorno
cp VectorStinger.Container/.env.example VectorStinger.Container/.env
nano VectorStinger.Container/.env

# 2. Build y deploy
docker-compose -f VectorStinger.Container/docker-compose.yml --profile stage up -d

# 3. Verificar
curl http://localhost:8035/health
```

---

## 📦 Despliegue Manual a ACR - Stage

Para subir la imagen Docker a Azure Container Registry manualmente en ambiente **Stage**:

### Prerequisites
```powershell
# Verificar que Docker está ejecutándose
docker ps

# Verificar Azure CLI instalado
az --version
```

### Paso 1: Login a Azure Container Registry
```powershell
az login --use-device-code
az acr login --name acrplatheotemplatestg
```

### Paso 2: Build de la Imagen
```powershell
# Desde la raíz del repositorio
docker build -f VectorStinger.Container/Dockerfile -t platheo-api:stage --build-arg ASPNETCORE_ENVIRONMENT=Stage .
```

### Paso 3: Etiquetar para ACR
```powershell
docker tag platheo-api:stage acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io/platheo-api:stage
```

### Paso 4: Push a ACR
```powershell
docker push acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io/platheo-api:stage
```

### Paso 5: Verificar Despliegue
```powershell
az acr repository show-tags --name acrplatheotemplatestg --repository platheo-api
```

**Resultado esperado:**
```
[
  "stage"
]
```

### 🚀 Comando Todo-en-Uno (PowerShell)
```powershell
# Ejecutar todos los pasos en secuencia
az acr login --name acrplatheotemplatestg-gafvh3d5d4hbb4fc
docker build -f VectorStinger.Container/Dockerfile -t platheo-api:stage --build-arg ASPNETCORE_ENVIRONMENT=Stage .
docker tag platheo-api:stage acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io/platheo-api:stage
docker push acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io/platheo-api:stage
az acr repository show-tags --name acrplatheotemplatestg-gafvh3d5d4hbb4fc --repository platheo-api
```

### 🔗 Documentación Completa
Para despliegue automatizado y más opciones, ver: [`ACR_DEPLOYMENT.md`](./ACR_DEPLOYMENT.md)

---

## 📞 Soporte

- **Documentación completa**: `VectorStinger.Container/DOCKER_README.md`
- **Issues**: [Azure DevOps](https://dev.azure.com/platheoinc/Platheo-Templates/_git/Platheo-Templates-API)
- **Equipo**: Platheo Development Team

---

**Última actualización**: 11/01/2025 - v1.1 (Archivos movidos a VectorStinger.Container)
