# ?? IMPORTANTE: Corrección de Variable ASPNETCORE_ENVIRONMENT

## ?? Problema Identificado

En el Container App `ca-platheotemplate-stg`, se detectó una **configuración incorrecta** de la variable `ASPNETCORE_ENVIRONMENT`:

### Variables Actuales en Container App

| Variable | Valor Actual | Estado |
|----------|--------------|--------|
| `Environment` | `Stage` | ?? Redundante |
| `ASPNETCORE_ENVIRONMENT` | `Staging` | ? **INCORRECTO** |
| `EnableDetailedTelemetry` | `false` | ? OK |
| `APPINSIGHTS_CONNECTION_STRING` | (secreto) | ? OK |
| `DB_PASSWORD` | (secreto) | ? OK |

---

## ? ¿Por qué es un Problema?

### 1. **Archivo de configuración no coincide**

Tu proyecto tiene:
```
VectorStinger.Api.Service/
??? appsettings.json
??? appsettings.Development.json
??? appsettings.Stage.json          ? Nombre del archivo
??? appsettings.Production.json
```

Pero el Container App tiene:
```
ASPNETCORE_ENVIRONMENT=Staging      ? ASP.NET Core buscará appsettings.Staging.json
```

### 2. **Comportamiento de ASP.NET Core**

ASP.NET Core carga archivos de configuración en este orden:
1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (específico del ambiente)

Con `ASPNETCORE_ENVIRONMENT=Staging`, buscará:
- ? `appsettings.Staging.json` (NO EXISTE)
- ? `appsettings.Stage.json` (EXISTE, pero no se carga)

### 3. **Consecuencias**

- ? No carga la configuración específica de Stage
- ? Usa valores por defecto de `appsettings.json`
- ? Connection string incorrecta
- ? Secrets no se reemplazan correctamente
- ? Configuración de telemetría incorrecta

---

## ? Solución Implementada

### Opción 1: Corrección Automática en Pipeline (RECOMENDADO)

La pipeline ahora:
1. ? Verifica el valor de `ASPNETCORE_ENVIRONMENT`
2. ? Lo corrige automáticamente a `Stage` en cada deployment
3. ? Valida que el cambio se aplicó correctamente

**No requiere acción manual** - Se corregirá en el próximo deployment.

### Opción 2: Corrección Manual Inmediata

Si necesitas corregirlo ahora sin esperar al deployment:

```sh
# Opción A: Actualizar solo la variable
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Stage"

# Opción B: Actualizar varias variables a la vez
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Stage" "Environment=Stage" "EnableDetailedTelemetry=false"
```

---

## ?? Limpieza Recomendada

### Eliminar Variable Redundante

La variable `Environment` no es necesaria si tienes `ASPNETCORE_ENVIRONMENT`:

```sh
# Listar todas las variables
az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --query "properties.template.containers[0].env[].{Name:name, Value:value, SecretRef:secretRef}" \
  -o table

# Actualizar removiendo Environment (mantener solo ASPNETCORE_ENVIRONMENT)
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --remove-env-vars "Environment"
```

---

## ?? Configuración Correcta Final

### Variables de Entorno Recomendadas

| Variable | Valor | Tipo | Descripción |
|----------|-------|------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Stage` | Manual | ? Principal - Define ambiente ASP.NET Core |
| `EnableDetailedTelemetry` | `false` | Manual | Controla telemetría detallada |
| `ASPINSIGHTS_CONNECTION_STRING` | (valor) | Secret | Application Insights connection |
| `DB_PASSWORD` | (valor) | Secret | Password de base de datos |
| `PAYMENT_SECRET_KEY` | (valor opcional) | Secret | Key para payment bridge |

### Secrets Recomendados

```sh
# Verificar secrets actuales
az containerapp secret list \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  -o table

# Secrets esperados:
# - applicationinsights-connecti... (para APPINSIGHTS_CONNECTION_STRING)
# - db-password (para DB_PASSWORD)
# - payment-secret-key (para PAYMENT_SECRET_KEY) - si se usa
```

---

## ?? Verificación Post-Corrección

### 1. Verificar Variable en Container App

```sh
# PowerShell
az containerapp show `
  --name ca-platheotemplate-stg `
  --resource-group "Suscripción Platheo" `
  --query "properties.template.containers[0].env[?name=='ASPNETCORE_ENVIRONMENT']" `
  -o table

# Resultado esperado:
# Name                      Value
# ------------------------  -----
# ASPNETCORE_ENVIRONMENT    Stage
```

### 2. Verificar Logs de la Aplicación

```sh
# Ver logs recientes
az containerapp logs show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --follow

# Buscar línea que confirme el ambiente:
# "Hosting environment: Stage"
```

### 3. Probar Endpoints

```sh
# Obtener URL
APP_URL=$(az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --query "properties.configuration.ingress.fqdn" \
  -o tsv)

# Probar health check
curl https://$APP_URL/health
# Resultado esperado: "Healthy"

# Probar que Swagger NO esté disponible (solo en Development)
curl -I https://$APP_URL/swagger
# Resultado esperado: HTTP 404 (porque Stage != Development)
```

---

## ?? Cambios en el Código

### Program.cs - Uso de ASPNETCORE_ENVIRONMENT

El archivo `Program.cs` usa `ASPNETCORE_ENVIRONMENT` para:

```csharp
// Línea ~103
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
```

**Comportamiento por Ambiente:**

| Ambiente | `IsDevelopment()` | Swagger | OpenAPI | Redirect to /swagger |
|----------|-------------------|---------|---------|----------------------|
| `Development` | ? true | ? Habilitado | ? Habilitado | ? Habilitado |
| `Stage` | ? false | ? Deshabilitado | ? Deshabilitado | ? Deshabilitado |
| `Staging` | ? false | ? Deshabilitado | ? Deshabilitado | ? Deshabilitado |
| `Production` | ? false | ? Deshabilitado | ? Deshabilitado | ? Deshabilitado |

### appsettings.Stage.json - Placeholders

Los placeholders en `appsettings.Stage.json` se reemplazan con variables de entorno:

```json
{
  "UseCase": {
    "EnableDetailedTelemetry": "${EnableDetailedTelemetry}"  // ? Variable de entorno
  },
  "DatabaseSettings": {
    "DefaultConnection": "...Password=${DB_PASSWORD};..."    // ? Variable de entorno
  },
  "PaymentBridgeSettings": {
    "SecretKey": "${PAYMENT_SECRET_KEY}"                     // ? Variable de entorno
  },
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "${APPINSIGHTS_CONNECTION_STRING}"  // ? Variable de entorno
}
```

**?? IMPORTANTE:** ASP.NET Core **NO** reemplaza los placeholders `${VAR}` automáticamente.

---

## ?? Alternativa: Usar Variables de Entorno Directamente

Si prefieres que ASP.NET Core lea directamente las variables sin placeholders:

### Opción 1: Configurar Variables con Prefijo (RECOMENDADO)

```sh
# En Container App, usar nombres con sección
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --set-env-vars \
    "ASPNETCORE_ENVIRONMENT=Stage" \
    "UseCase__EnableDetailedTelemetry=false" \
    "DatabaseSettings__DefaultConnection=Data Source=..." \
    "PaymentBridgeSettings__SecretKey=secretref:payment-secret-key"
```

### Opción 2: Mantener Placeholders y Usar Script de Reemplazo

Crear script de inicialización que reemplace placeholders antes de iniciar la app.

---

## ? Checklist de Verificación

- [ ] `ASPNETCORE_ENVIRONMENT` = `Stage` (no `Staging`)
- [ ] Variable `Environment` eliminada (redundante)
- [ ] `appsettings.Stage.json` existe en el proyecto
- [ ] Secrets configurados en Container App
- [ ] Health check responde correctamente
- [ ] Logs muestran "Hosting environment: Stage"
- [ ] Swagger NO está accesible (correcto para Stage)
- [ ] Connection string de BD correcta (Stage)
- [ ] Application Insights conectado

---

## ?? Soporte

Si después de la corrección sigues teniendo problemas:

1. **Ver logs del contenedor:**
```sh
az containerapp logs show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --follow
```

2. **Ver variables de entorno activas:**
```sh
az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group "Suscripción Platheo" \
  --query "properties.template.containers[0].env" \
  -o table
```

3. **Ejecutar script de verificación:**
```powershell
.\.Deploy\verify-config.ps1
```

---

**Última actualización**: 11/01/2025 - v1.0
**Estado**: ?? ACCIÓN REQUERIDA - Corregir ASPNETCORE_ENVIRONMENT
