# ?? Azure DevOps Pipeline - Deployment Guide

Esta carpeta contiene las pipelines de CI/CD para desplegar Platheo Templates API a Azure.

---

## ?? Estructura

```
.Deploy/
??? azure-pipelines-stage.yml      # Pipeline para ambiente Stage
??? azure-pipelines-prod.yml       # Pipeline para ambiente Production (futuro)
??? README.md                      # Esta guía
```

---

## ?? Pipeline de Stage

### Descripción

La pipeline `azure-pipelines-stage.yml` automatiza:

1. **Build**: Construye la imagen Docker con configuración de Stage
2. **Push**: Sube la imagen a Azure Container Registry (ACR)
3. **Deploy**: Despliega la imagen en Azure Container Apps (ACA)
4. **Verify**: Verifica que el despliegue fue exitoso

### Configuración

#### Variables Configuradas

| Variable | Valor | Descripción |
|----------|-------|-------------|
| `azureSubscription` | Suscripción Platheo | Service Connection en Azure DevOps |
| `subscriptionId` | 7430bafe-bc29-443c-93d1-a8cb3090136c | ID de suscripción de Azure |
| `resourceGroup` | Suscripción Platheo | Grupo de recursos |
| `acrName` | acrplatheotemplatestg | Nombre del ACR (sin sufijo) |
| `acrLoginServer` | acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io | URL completa del ACR |
| `imageName` | platheo-api | Nombre de la imagen Docker |
| `containerAppName` | ca-platheotemplate-stg | Nombre del Container App |

#### Triggers

- **Branch**: `main`
- **Paths**: Cambios en código fuente o Dockerfile
- **PRs**: No se ejecuta automáticamente en PRs

---

## ?? Configuración Inicial en Azure DevOps

### Paso 1: Crear Service Connection

1. Ve a **Project Settings** ? **Service connections**
2. Click en **New service connection**
3. Selecciona **Azure Resource Manager**
4. Selecciona **Service principal (automatic)**
5. Configura:
   - **Subscription**: Suscripción Platheo (7430bafe-bc29-443c-93d1-a8cb3090136c)
   - **Resource Group**: Suscripción Platheo
   - **Service connection name**: `Suscripción Platheo`
6. Click en **Save**

### Paso 2: Crear Docker Registry Service Connection

?? **IMPORTANTE**: Este paso es necesario antes de ejecutar la pipeline.

#### Opción A: Creación Manual (Recomendado)

1. **Obtener credenciales del ACR**
   ```powershell
   az acr credential show --name acrplatheotemplatestg --resource-group "Suscripción Platheo"
   ```
   
   **Output:**
   ```json
   {
     "passwords": [
       {
         "name": "password",
         "value": "xxxxx..."  ? Copia este valor
       }
     ],
     "username": "acrplatheotemplatestg"
   }
   ```

2. **Ir a Azure DevOps**
   - Ve a: `https://dev.azure.com/platheoinc/Platheo-Templates`
   - Click en **Project Settings** (?? abajo a la izquierda)
   - Selecciona **Service connections**

3. **Crear nueva conexión**
   - Click en **New service connection**
   - Busca y selecciona: **Docker Registry**
   - Click **Next**

4. **Configurar conexión**
   - **Registry type**: Selecciona **Others** (NO "Azure Container Registry")
   - **Docker Registry**: `acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io`
   - **Docker ID**: `acrplatheotemplatestg`
   - **Docker Password**: (Pega el valor de `password` del paso 1)
   - **Service connection name**: `acrplatheotemplatestg` ?? **DEBE SER EXACTAMENTE ESTE NOMBRE**
   - **Description**: `ACR connection for Platheo Templates Stage`
   - ? Marca: **Grant access permission to all pipelines**
   - Click **Save**

5. **Verificar creación**
   - La conexión debe aparecer en la lista con estado ? **Ready**
   - Si aparece con error, revisa las credenciales

#### Opción B: Con Script Helper

```powershell
# Ejecutar script que te guiará paso a paso
.\.Deploy\create-service-connection.ps1
```

Este script:
- ? Obtiene automáticamente las credenciales
- ? Muestra toda la información necesaria
- ? Abre Azure DevOps en el navegador
- ? Te guía en los pasos de creación

#### ?? Troubleshooting

**Error: "service connection could not be found"**
- Verifica que el nombre sea exactamente: `acrplatheotemplatestg` (sin espacios ni mayúsculas diferentes)
- Verifica que esté marcado "Grant access permission to all pipelines"

**Error: "Failed to validate connection"**
- Verifica que el Docker Registry sea: `acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io`
- Verifica que el username sea: `acrplatheotemplatestg`
- Verifica que la password sea correcta (cópiala completa del comando az acr credential show)

**Error: "Admin user not enabled"**
- El admin user ya está habilitado ? (se ve en tu imagen)
- Si estuviera deshabilitado:
  ```powershell
  az acr update --name acrplatheotemplatestg --admin-enabled true
  ```

### Paso 3: Crear Environment

1. Ve a **Pipelines** ? **Environments**
2. Click en **New environment**
3. Configura:
   - **Name**: `Stage`
   - **Description**: Stage environment for Platheo Templates API
   - **Resource**: None
4. Click en **Create**

### Paso 4: Crear Pipeline

1. Ve a **Pipelines** ? **Pipelines**
2. Click en **New pipeline**
3. Selecciona **Azure Repos Git**
4. Selecciona tu repositorio: `Platheo-Templates-API`
5. Selecciona **Existing Azure Pipelines YAML file**
6. Selecciona la ruta: `.Deploy/azure-pipelines-stage.yml`
7. Click en **Continue**
8. Revisa la configuración
9. Click en **Run**

---

## ?? Configuración de Secrets en Container App

### ? **Nueva Solución Implementada**

Se implementó la configuración mediante **variables de entorno usando el formato nativo de ASP.NET Core**.

?? **Ver documentación completa**: [ENV_VARS_SOLUTION.md](ENV_VARS_SOLUTION.md)

### Resumen de Cambios

**Antes (? No funcionaba):**
```json
// appsettings.Stage.json
{
  "DatabaseSettings": {
    "DefaultConnection": "...Password=${DB_PASSWORD};..."  // Placeholder no soportado
  }
}
```

**Ahora (? Funciona):**
```json
// appsettings.Stage.json
{
  "DatabaseSettings": {
    "DefaultConnection": ""  // Valor vacío, se sobrescribe con variable de entorno
  }
}
```

**Variables de entorno en Azure Container Apps:**
```bash
# Formato con doble guión __ (mapea a jerarquía JSON)
ASPNETCORE_ENVIRONMENT=Stage
UseCase__EnableDetailedTelemetry=false
DatabaseSettings__DefaultConnection=secretref:db-connection-string
PaymentBridgeSettings__SecretKey=secretref:payment-secret-key
APPLICATIONINSIGHTS_CONNECTION_STRING=secretref:appinsights-connection-string
```

### Configuración Rápida

**Opción 1: Usar script de PowerShell (RECOMENDADO)**
```powershell
cd .Deploy
.\configure-env-vars.ps1
```

**Opción 2: Configuración manual con Azure CLI**
```bash
# 1. Crear secrets
az containerapp secret set \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --secrets \
    "db-connection-string=Data Source=platheo-stage-srvbd.database.windows.net;Initial Catalog=BD-Platheo-Template-Stage;User ID=UsrStageAdmin;Password=TU_PASSWORD;Encrypt=True;TrustServerCertificate=True" \
    "appinsights-connection-string=InstrumentationKey=xxx" \
    "payment-secret-key=sk_live_xxx"

# 2. Configurar variables de entorno
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --set-env-vars \
    "ASPNETCORE_ENVIRONMENT=Stage" \
    "UseCase__EnableDetailedTelemetry=false" \
    "DatabaseSettings__DefaultConnection=secretref:db-connection-string" \
    "PaymentBridgeSettings__SecretKey=secretref:payment-secret-key" \
    "APPLICATIONINSIGHTS_CONNECTION_STRING=secretref:appinsights-connection-string"
```

### Verificar Configuración

```sh
# Ver variables de entorno
az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --query "properties.template.containers[0].env" \
  -o table

# Ver secrets
az containerapp secret list \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  -o table
```

**Resultado esperado:**
```
Name                                Value    SecretRef
----------------------------------  -------  -------------------------
ASPNETCORE_ENVIRONMENT              Stage    
UseCase__EnableDetailedTelemetry    false    
DatabaseSettings__DefaultConnection          db-connection-string
PaymentBridgeSettings__SecretKey             payment-secret-key
APPLICATIONINSIGHTS_CONNECTION...            appinsights-connect...
```

?? **Para más detalles**: Ver [ENV_VARS_SOLUTION.md](ENV_VARS_SOLUTION.md)

---

## ?? Variables de Entorno - Referencia Rápida

### Formato de Mapeo

| Variable de Entorno | Mapea a (JSON) | Tipo |
|---------------------|----------------|------|
| `UseCase__EnableDetailedTelemetry` | `UseCase:EnableDetailedTelemetry` | Variable |
| `DatabaseSettings__DefaultConnection` | `DatabaseSettings:DefaultConnection` | Secret |
| `PaymentBridgeSettings__SecretKey` | `PaymentBridgeSettings:SecretKey` | Secret |

**Regla**: Doble guión bajo `__` en variable de entorno = Dos puntos `:` en JSON

---

## ?? Monitoreo del Deployment

### Ver Logs de la Pipeline

1. Ve a **Pipelines** ? **Pipelines**
2. Selecciona la ejecución de la pipeline
3. Click en cada stage/job para ver logs detallados

### Ver Logs del Container App

```sh
# Ver logs en tiempo real
az containerapp logs show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --follow

# Ver logs de una revisión específica
az containerapp revision logs show \
  --name ca-platheotemplate-stg--build-123 \
  --resource-group "Suscripción Platheo"
```

### Health Check

```sh
# Obtener URL del Container App
APP_URL=$(az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --query "properties.configuration.ingress.fqdn" \
  -o tsv)

# Probar health check
curl https://$APP_URL/health

# Probar Swagger
curl https://$APP_URL/swagger
```

---

## ?? Workflow de Deployment

### Deployment Automático (Trigger en main)

```
1. Developer hace push a main
   ?
2. Pipeline se activa automáticamente
   ?
3. Build stage:
   - Construye imagen Docker
   - Tag: stage-{BuildId} y stage-latest
   - Push a ACR
   ?
4. Deploy stage:
   - Obtiene configuración actual de ACA
   - Actualiza imagen del contenedor
   - Mantiene variables de entorno existentes
   - Crea nueva revisión
   ?
5. Verify:
   - Verifica health check
   - Lista revisiones activas
   - Muestra URL de acceso
```

### Deployment Manual

```sh
# Desde Azure DevOps
1. Ve a Pipelines ? Pipelines
2. Selecciona la pipeline "Deploy to Stage"
3. Click en "Run pipeline"
4. Selecciona branch: main
5. Click en "Run"
```

---

## ?? Stages de la Pipeline

### Stage 1: Build & Push Docker Image

**Pasos:**
1. Checkout del código
2. Login a ACR
3. Build de imagen Docker
4. Tag de imagen (build-id y latest)
5. Push a ACR
6. Verificación de imagen en ACR

**Duración aproximada**: 3-5 minutos

### Stage 2: Deploy to Azure Container Apps

**Pasos:**
1. Obtener configuración actual de ACA
2. Actualizar Container App con nueva imagen
3. Verificar despliegue (health check)
4. Listar revisiones activas
5. Mostrar resumen del deployment

**Duración aproximada**: 2-3 minutos

---

## ?? Troubleshooting

### Error: Service connection not found

**Problema**: La service connection no existe o tiene nombre diferente.

**Solución**:
1. Verifica el nombre en Project Settings ? Service connections
2. Actualiza la variable `azureSubscription` en la pipeline

### Error: Image not found in ACR

**Problema**: La imagen no se subió correctamente a ACR.

**Solución**:
```sh
# Verificar imágenes en ACR
az acr repository list --name acrplatheotemplatestg

# Ver tags de una imagen
az acr repository show-tags --name acrplatheotemplatestg --repository platheo-api
```

### Error: Container App update failed

**Problema**: El Container App no se pudo actualizar.

**Solución**:
```sh
# Ver estado del Container App
az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --query "properties.provisioningState"

# Ver logs de la última revisión
az containerapp logs show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --follow
```

### Error: Health check failed

**Problema**: La aplicación no responde en el endpoint /health.

**Solución**:
1. Ver logs del container:
```sh
az containerapp logs show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --follow
```

2. Verificar variables de entorno:
```sh
az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --query "properties.template.containers[0].env"
```

3. Verificar secrets:
```sh
az containerapp secret list \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo"
```

---

## ?? Próximos Pasos

1. **Configurar Pipeline de Production**
   - Crear `azure-pipelines-prod.yml`
   - Configurar approvals en environment Production

2. **Implementar Blue-Green Deployment**
   - Usar traffic splitting en Container Apps
   - Gradual rollout de nuevas versiones

3. **Agregar Tests Automatizados**
   - Integration tests antes del deploy
   - Smoke tests después del deploy

4. **Configurar Alertas**
   - Application Insights alerts
   - Azure Monitor alerts

---

## ?? Soporte

- **Documentación**: `.Deploy/README.md`
- **Pipeline Issues**: Azure DevOps ? Pipelines ? {pipeline} ? Issues
- **Azure Issues**: Azure Portal ? Container Apps ? {app} ? Logs

---

**Última actualización**: 11/01/2025 - v1.0
