# ?? Guía de Despliegue a Azure Container Registry (ACR)

Esta guía te ayudará a subir la imagen Docker del API de Platheo Templates a Azure Container Registry.

---

## ?? Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Método 1: Despliegue por CLI (Recomendado)](#método-1-despliegue-por-cli-recomendado)
3. [Método 2: Despliegue Manual con Archivo TAR](#método-2-despliegue-manual-con-archivo-tar)
4. [Verificación del Despliegue](#verificación-del-despliegue)
5. [Configuración de Azure Container Apps/AKS](#configuración-de-azure-container-appsaks)
6. [Troubleshooting](#troubleshooting)

---

## ?? Requisitos Previos

### Software Necesario

- [x] **Docker Desktop** instalado y ejecutándose
- [x] **Azure CLI** instalado ([Descargar](https://aka.ms/installazurecli))
- [x] Imagen Docker construida localmente
- [x] Acceso a un Azure Container Registry

### Verificar Instalaciones

```powershell
# Verificar Docker
docker --version
# Output esperado: Docker version 24.x.x, build xxxxx

# Verificar Azure CLI
az --version
# Output esperado: azure-cli 2.xx.x

# Verificar imagen local
docker images | Select-String "platheo|vectorstinger"
# Output esperado: vectorstingercontainer-api-dev   latest    xxxxx   X hours ago   343MB
```

### Información de tu ACR

Necesitarás conocer:
- **Nombre del ACR**: `<tu-acr-name>` (ejemplo: `platheoacr`)
- **Subscription ID**: ID de tu suscripción de Azure
- **Resource Group**: Grupo de recursos donde está el ACR

---

## ?? Método 1: Despliegue por CLI (Recomendado)

Este es el método más rápido y directo para subir tu imagen a ACR.

### Paso 1: Login a Azure

```powershell
# Login a Azure
az login

# Seleccionar la suscripción correcta (si tienes múltiples)
az account list --output table
az account set --subscription "<tu-subscription-id>"

# Verificar suscripción activa
az account show --output table
```

### Paso 2: Login al Azure Container Registry

```powershell
# Método 1: Login automático con Azure CLI (recomendado)
az acr login --name <tu-acr-name>

# Método 2: Login manual con credenciales
# Primero obtener las credenciales
az acr credential show --name <tu-acr-name>

# Luego usar docker login
docker login <tu-acr-name>.azurecr.io -u <username> -p <password>
```

**Ejemplo real:**
```powershell
az acr login --name platheoacr
```

**Output esperado:**
```
Login Succeeded
```

### Paso 3: Construir la Imagen (si no existe)

```powershell
# Desde la raíz del repositorio
cd D:\Proyectos\Platheo\source\Platheo-Templates-API

# Construir imagen de desarrollo
docker-compose -f VectorStinger.Container/docker-compose.yml build api-dev

# O construir directamente con Docker
docker build -f VectorStinger.Container/Dockerfile -t platheo-api:dev .
```

### Paso 4: Etiquetar la Imagen para ACR

```powershell
# Formato general:
# docker tag <imagen-local> <acr-name>.azurecr.io/<nombre-repo>:<tag>

# Etiquetar como 'latest'
docker tag vectorstingercontainer-api-dev:latest <tu-acr-name>.azurecr.io/platheo-api:latest

# Etiquetar con versión específica
docker tag vectorstingercontainer-api-dev:latest <tu-acr-name>.azurecr.io/platheo-api:v1.0.0

# Etiquetar por ambiente
docker tag vectorstingercontainer-api-dev:latest <tu-acr-name>.azurecr.io/platheo-api:dev
```

**Ejemplo real:**
```powershell
docker tag vectorstingercontainer-api-dev:latest platheoacr.azurecr.io/platheo-api:latest
docker tag vectorstingercontainer-api-dev:latest platheoacr.azurecr.io/platheo-api:v1.0.0
docker tag vectorstingercontainer-api-dev:latest platheoacr.azurecr.io/platheo-api:dev
```

### Paso 5: Verificar Etiquetas

```powershell
# Ver todas las imágenes etiquetadas para ACR
docker images | Select-String "<tu-acr-name>.azurecr.io"
```

**Output esperado:**
```
platheoacr.azurecr.io/platheo-api   latest   be9e240e9294   2 hours ago   343MB
platheoacr.azurecr.io/platheo-api   v1.0.0   be9e240e9294   2 hours ago   343MB
platheoacr.azurecr.io/platheo-api   dev      be9e240e9294   2 hours ago   343MB
```

### Paso 6: Push a Azure Container Registry

```powershell
# Push todas las etiquetas
docker push <tu-acr-name>.azurecr.io/platheo-api:latest
docker push <tu-acr-name>.azurecr.io/platheo-api:v1.0.0
docker push <tu-acr-name>.azurecr.io/platheo-api:dev

# O push de una sola vez (todas las tags del mismo repositorio)
docker push <tu-acr-name>.azurecr.io/platheo-api --all-tags
```

**Ejemplo real:**
```powershell
docker push platheoacr.azurecr.io/platheo-api:latest
docker push platheoacr.azurecr.io/platheo-api:v1.0.0
docker push platheoacr.azurecr.io/platheo-api:dev
```

**Output esperado:**
```
The push refers to repository [platheoacr.azurecr.io/platheo-api]
5f70bf18a086: Pushed
a7a5f6c8e0df: Pushed
c2d3f8b5e1a2: Pushed
...
latest: digest: sha256:abc123def456... size: 2847
```

### ?? Script Automatizado (PowerShell)

Guarda este script como `VectorStinger.Container/deploy-to-acr.ps1`:

```powershell
# ========================================
# Platheo API - Deploy to ACR Script
# ========================================

param(
    [Parameter(Mandatory=$true)]
    [string]$AcrName,
    
  [Parameter(Mandatory=$false)]
    [string]$Version = "latest",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev"
)

$ErrorActionPreference = "Stop"
$ImageName = "platheo-api"
$LocalImage = "vectorstingercontainer-api-$Environment"

Write-Host "?? Iniciando despliegue a Azure Container Registry" -ForegroundColor Green
Write-Host "ACR: $AcrName" -ForegroundColor Cyan
Write-Host "Imagen: $ImageName" -ForegroundColor Cyan
Write-Host "Versión: $Version" -ForegroundColor Cyan
Write-Host "Ambiente: $Environment" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar que Docker está ejecutándose
Write-Host "?? Verificando Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "? Docker está ejecutándose" -ForegroundColor Green
} catch {
    Write-Host "? Error: Docker no está ejecutándose. Inicia Docker Desktop." -ForegroundColor Red
    exit 1
}

# Paso 2: Login a Azure
Write-Host ""
Write-Host "?? Iniciando sesión en Azure..." -ForegroundColor Yellow
try {
    az account show | Out-Null
    Write-Host "? Ya tienes sesión activa en Azure" -ForegroundColor Green
} catch {
    Write-Host "??  No hay sesión activa. Iniciando login..." -ForegroundColor Yellow
  az login
}

# Paso 3: Login a ACR
Write-Host ""
Write-Host "?? Iniciando sesión en ACR..." -ForegroundColor Yellow
az acr login --name $AcrName
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al hacer login en ACR" -ForegroundColor Red
    exit 1
}
Write-Host "? Login exitoso en ACR" -ForegroundColor Green

# Paso 4: Verificar imagen local
Write-Host ""
Write-Host "?? Verificando imagen local..." -ForegroundColor Yellow
$imageExists = docker images --format "{{.Repository}}:{{.Tag}}" | Select-String -Pattern "$LocalImage`:latest"
if (-not $imageExists) {
 Write-Host "??  Imagen local no encontrada. Construyendo..." -ForegroundColor Yellow
    
    # Build de la imagen
    Write-Host "?? Construyendo imagen..." -ForegroundColor Yellow
    docker-compose -f VectorStinger.Container/docker-compose.yml build api-$Environment
    
    if ($LASTEXITCODE -ne 0) {
      Write-Host "? Error al construir la imagen" -ForegroundColor Red
        exit 1
    }
    Write-Host "? Imagen construida exitosamente" -ForegroundColor Green
} else {
    Write-Host "? Imagen local encontrada" -ForegroundColor Green
}

# Paso 5: Etiquetar imagen
Write-Host ""
Write-Host "???  Etiquetando imagen para ACR..." -ForegroundColor Yellow
$acrImage = "$AcrName.azurecr.io/$ImageName"

docker tag "$LocalImage`:latest" "$acrImage`:$Version"
docker tag "$LocalImage`:latest" "$acrImage`:$Environment-latest"
docker tag "$LocalImage`:latest" "$acrImage`:latest"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al etiquetar la imagen" -ForegroundColor Red
    exit 1
}
Write-Host "? Imagen etiquetada correctamente" -ForegroundColor Green

# Mostrar tags creados
Write-Host ""
Write-Host "?? Tags creados:" -ForegroundColor Cyan
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}" | Select-String -Pattern $acrImage

# Paso 6: Push a ACR
Write-Host ""
Write-Host "??  Subiendo imagen a ACR..." -ForegroundColor Yellow
Write-Host "Esto puede tomar varios minutos dependiendo de tu conexión..." -ForegroundColor Gray

docker push "$acrImage`:$Version"
docker push "$acrImage`:$Environment-latest"
docker push "$acrImage`:latest"

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al subir la imagen a ACR" -ForegroundColor Red
    exit 1
}

Write-Host "? Imagen subida exitosamente a ACR" -ForegroundColor Green

# Paso 7: Verificar en ACR
Write-Host ""
Write-Host "?? Verificando imagen en ACR..." -ForegroundColor Yellow
az acr repository show-tags --name $AcrName --repository $ImageName --output table

# Resumen final
Write-Host ""
Write-Host "??????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?          ? DESPLIEGUE COMPLETADO       ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "?? Imagen disponible en:" -ForegroundColor Cyan
Write-Host "   $acrImage`:$Version" -ForegroundColor White
Write-Host "   $acrImage`:$Environment-latest" -ForegroundColor White
Write-Host "   $acrImage`:latest" -ForegroundColor White
Write-Host ""
Write-Host "?? Pull command:" -ForegroundColor Cyan
Write-Host "   docker pull $acrImage`:$Version" -ForegroundColor White
Write-Host ""
Write-Host "?? Siguiente paso: Desplegar en Azure Container Apps o AKS" -ForegroundColor Yellow
```

**Uso del script:**
```powershell
cd D:\Proyectos\Platheo\source\Platheo-Templates-API

# Desplegar versión latest del ambiente dev
.\VectorStinger.Container\deploy-to-acr.ps1 -AcrName "platheoacr" -Version "v1.0.0" -Environment "dev"

# Desplegar a producción
.\VectorStinger.Container\deploy-to-acr.ps1 -AcrName "platheoacr" -Version "v1.0.1" -Environment "prod"
```

---

## ?? Método 2: Despliegue Manual con Archivo TAR

Este método es útil cuando no tienes conexión directa a Azure desde tu máquina de desarrollo.

### Paso 1: Exportar Imagen a Archivo TAR

```powershell
# Navegar al directorio del proyecto
cd D:\Proyectos\Platheo\source\Platheo-Templates-API

# Exportar imagen sin comprimir
docker save vectorstingercontainer-api-dev:latest -o platheo-api-dev.tar

# O exportar con compresión (recomendado - archivo más pequeño)
docker save vectorstingercontainer-api-dev:latest | gzip > platheo-api-dev.tar.gz
```

**Tamaño aproximado:**
- Sin comprimir: ~343 MB
- Con gzip: ~120-150 MB

### Paso 2: Transferir Archivo

Tienes varias opciones para transferir el archivo:

#### Opción A: Azure Storage Blob

```powershell
# Subir a Azure Storage
az storage blob upload `
    --account-name <storage-account> `
    --container-name docker-images `
    --file platheo-api-dev.tar.gz `
    --name platheo-api-dev.tar.gz

# Obtener URL de descarga
az storage blob url `
    --account-name <storage-account> `
    --container-name docker-images `
    --name platheo-api-dev.tar.gz
```

#### Opción B: SCP (Linux/Mac)

```sh
scp platheo-api-dev.tar.gz usuario@servidor-azure:/tmp/
```

#### Opción C: USB/Red Local

Simplemente copia el archivo a un USB o carpeta compartida en red.

### Paso 3: Cargar Imagen en Servidor Destino

Desde el servidor que tiene acceso a ACR:

```sh
# Cargar imagen desde archivo
docker load -i platheo-api-dev.tar.gz
# O sin compresión
docker load -i platheo-api-dev.tar

# Verificar imagen cargada
docker images | grep platheo
```

### Paso 4: Push desde Servidor a ACR

```sh
# Login a ACR
az acr login --name <tu-acr-name>

# Etiquetar
docker tag vectorstingercontainer-api-dev:latest <tu-acr-name>.azurecr.io/platheo-api:latest

# Push
docker push <tu-acr-name>.azurecr.io/platheo-api:latest
```

---

## ? Verificación del Despliegue

### Verificar en Azure CLI

```powershell
# Listar todos los repositorios en el ACR
az acr repository list --name <tu-acr-name> --output table

# Listar tags de la imagen
az acr repository show-tags --name <tu-acr-name> --repository platheo-api --output table

# Ver detalles completos de una imagen
az acr repository show --name <tu-acr-name> --repository platheo-api --output table

# Ver manifest de una imagen específica
az acr repository show-manifests --name <tu-acr-name> --repository platheo-api --output table
```

### Verificar en Azure Portal

1. Ir a [portal.azure.com](https://portal.azure.com)
2. Navegar a tu Container Registry
3. En el menú izquierdo, seleccionar **Repositories**
4. Buscar **platheo-api**
5. Ver los tags disponibles

### Pull de Prueba

```powershell
# Pull de la imagen desde ACR
docker pull <tu-acr-name>.azurecr.io/platheo-api:latest

# Ejecutar para probar
docker run -d -p 8080:8080 `
    -e ASPNETCORE_ENVIRONMENT=Production `
    -e DB_PASSWORD="<password>" `
    <tu-acr-name>.azurecr.io/platheo-api:latest

# Verificar health check
Start-Sleep -Seconds 10
Invoke-RestMethod http://localhost:8080/health
```

---

## ?? Configuración de Azure Container Apps/AKS

### Desplegar en Azure Container Apps

```powershell
# Variables
$RESOURCE_GROUP = "rg-platheo-prod"
$LOCATION = "eastus"
$ACR_NAME = "platheoacr"
$APP_NAME = "platheo-api"
$CONTAINER_APP_ENV = "platheo-env"

# Crear Container Apps Environment (si no existe)
az containerapp env create `
    --name $CONTAINER_APP_ENV `
    --resource-group $RESOURCE_GROUP `
    --location $LOCATION

# Crear Container App
az containerapp create `
    --name $APP_NAME `
    --resource-group $RESOURCE_GROUP `
    --environment $CONTAINER_APP_ENV `
    --image "$ACR_NAME.azurecr.io/platheo-api:latest" `
    --registry-server "$ACR_NAME.azurecr.io" `
    --target-port 8080 `
    --ingress external `
    --env-vars `
        ASPNETCORE_ENVIRONMENT=Production `
        DB_PASSWORD=secretref:db-password `
        PAYMENT_SECRET_KEY=secretref:payment-key `
    --min-replicas 1 `
    --max-replicas 3 `
    --cpu 0.5 `
    --memory 1.0Gi

# Configurar secrets
az containerapp secret set `
    --name $APP_NAME `
    --resource-group $RESOURCE_GROUP `
    --secrets `
      db-password="<tu-db-password>" `
        payment-key="<tu-payment-key>"

# Obtener URL de la aplicación
az containerapp show `
    --name $APP_NAME `
 --resource-group $RESOURCE_GROUP `
    --query properties.configuration.ingress.fqdn `
  --output tsv
```

### Desplegar en AKS (Kubernetes)

Crear archivo `deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: platheo-api
  labels:
    app: platheo-api
spec:
  replicas: 3
  selector:
    matchLabels:
 app: platheo-api
  template:
    metadata:
      labels:
   app: platheo-api
    spec:
      containers:
      - name: api
        image: platheoacr.azurecr.io/platheo-api:latest
   ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: DB_PASSWORD
        valueFrom:
      secretKeyRef:
     name: platheo-secrets
      key: db-password
        - name: PAYMENT_SECRET_KEY
        valueFrom:
            secretKeyRef:
   name: platheo-secrets
          key: payment-key
     resources:
          requests:
          memory: "512Mi"
          cpu: "250m"
        limits:
   memory: "1Gi"
            cpu: "500m"
        livenessProbe:
        httpGet:
    path: /health
       port: 8080
          initialDelaySeconds: 30
 periodSeconds: 10
        readinessProbe:
          httpGet:
        path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: platheo-api-service
spec:
  type: LoadBalancer
  selector:
    app: platheo-api
  ports:
  - port: 80
    targetPort: 8080
```

Aplicar configuración:

```sh
# Conectar a AKS
az aks get-credentials --resource-group <resource-group> --name <aks-name>

# Crear secrets
kubectl create secret generic platheo-secrets \
  --from-literal=db-password='<password>' \
  --from-literal=payment-key='<key>'

# Aplicar deployment
kubectl apply -f deployment.yaml

# Verificar pods
kubectl get pods

# Ver logs
kubectl logs -f deployment/platheo-api

# Obtener IP externa
kubectl get service platheo-api-service
```

---

## ?? Troubleshooting

### Error: "authentication required"

```powershell
# Problema: No has hecho login a ACR
# Solución:
az acr login --name <tu-acr-name>
```

### Error: "unauthorized: authentication required"

```powershell
# Problema: Token expirado
# Solución: Renovar token
az acr login --name <tu-acr-name>
```

### Error: "denied: requested access to the resource is denied"

```powershell
# Problema: No tienes permisos en el ACR
# Solución: Verificar permisos
az role assignment list --assignee <tu-email> --scope /subscriptions/<subscription-id>/resourceGroups/<rg>/providers/Microsoft.ContainerRegistry/registries/<acr-name>

# Asignar rol AcrPush si no lo tienes
az role assignment create \
  --assignee <tu-email> \
  --role AcrPush \
  --scope /subscriptions/<subscription-id>/resourceGroups/<rg>/providers/Microsoft.ContainerRegistry/registries/<acr-name>
```

### Error: "dial tcp: lookup ... no such host"

```powershell
# Problema: Nombre de ACR incorrecto o ACR no existe
# Solución: Verificar nombre del ACR
az acr list --output table
```

### Push muy lento

```powershell
# Problema: Conexión lenta
# Solución: Usar compresión o aumentar timeout
docker push <tu-acr-name>.azurecr.io/platheo-api:latest --disable-content-trust
```

### Imagen muy grande

```powershell
# Ver capas de la imagen
docker history vectorstingercontainer-api-dev:latest

# Optimizar usando .dockerignore (ya implementado)
# Ver VectorStinger.Container/.dockerignore
```

---

## ?? Comandos de Referencia Rápida

### Gestión de Imágenes Locales

```powershell
# Listar imágenes
docker images

# Eliminar imagen
docker rmi <image-id>

# Limpiar imágenes sin usar
docker image prune -a

# Ver espacio usado
docker system df
```

### Gestión de ACR

```powershell
# Listar ACRs
az acr list --output table

# Crear ACR (si no existe)
az acr create --name <acr-name> --resource-group <rg> --sku Basic --location eastus

# Habilitar admin user
az acr update --name <acr-name> --admin-enabled true

# Obtener credenciales
az acr credential show --name <acr-name>

# Eliminar imagen de ACR
az acr repository delete --name <acr-name> --image platheo-api:v1.0.0
```

### Información del Sistema

```powershell
# Ver información de Docker
docker info

# Ver versión de Docker
docker --version

# Ver versión de Azure CLI
az --version

# Ver suscripción activa
az account show
```

---

## ?? Soporte y Recursos

### Documentación Oficial

- [Azure Container Registry Docs](https://docs.microsoft.com/azure/container-registry/)
- [Docker Push](https://docs.docker.com/engine/reference/commandline/push/)
- [Azure Container Apps](https://docs.microsoft.com/azure/container-apps/)
- [AKS Documentation](https://docs.microsoft.com/azure/aks/)

### Enlaces Útiles

- **Portal Azure**: https://portal.azure.com
- **Azure DevOps**: https://dev.azure.com/platheoinc/Platheo-Templates
- **Documentación Local**: `VectorStinger.Container/DOCKER_README.md`

### Contacto

- **Equipo**: Platheo Development Team
- **Repositorio**: [Platheo-Templates-API](https://dev.azure.com/platheoinc/Platheo-Templates/_git/Platheo-Templates-API)

---

**Última actualización**: 11/01/2025 - v1.0

? **Imagen Docker lista para desplegar en Azure** ??
