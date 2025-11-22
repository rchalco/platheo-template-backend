# ?? Resumen de Cambios - Solución de Variables de Entorno

## ? **Problema Resuelto**

Se corrigió el problema de placeholders `${VARIABLE}` que no eran soportados por ASP.NET Core.

---

## ?? **Archivos Modificados**

### 1. **VectorStinger.Api.Service/appsettings.Stage.json**

**Antes:**
```json
{
  "UseCase": {
    "EnableDetailedTelemetry": "${EnableDetailedTelemetry}"  // ? Placeholder no funciona
  },
  "DatabaseSettings": {
    "DefaultConnection": "...Password=${DB_PASSWORD};..."  // ? Placeholder no funciona
  }
}
```

**Después:**
```json
{
  "UseCase": {
    "EnableDetailedTelemetry": false  // ? Valor por defecto
  },
  "DatabaseSettings": {
    "DefaultConnection": ""  // ? Se sobrescribe con variable de entorno
  },
  "PaymentBridgeSettings": {
    "SecretKey": ""  // ? Se sobrescribe con variable de entorno
  },
  "APPLICATIONINSIGHTS_CONNECTION_STRING": ""  // ? Se sobrescribe con variable de entorno
}
```

**Cambios:**
- ? Eliminados placeholders `${VARIABLE}`
- ? Valores vacíos para que variables de entorno los sobrescriban
- ? Valor por defecto en `EnableDetailedTelemetry`

---

## ?? **Archivos Creados**

### 2. **.Deploy/configure-env-vars.ps1**

Script de PowerShell para configurar automáticamente las variables de entorno en Azure Container Apps.

**Funcionalidades:**
- ? Verifica Azure CLI y autenticación
- ? Verifica que el Container App existe
- ? Lista secretos configurados
- ? Configura todas las variables de entorno con formato correcto (`__`)
- ? Muestra resumen de configuración

**Uso:**
```powershell
cd .Deploy
.\configure-env-vars.ps1
```

---

### 3. **.Deploy/ENV_VARS_SOLUTION.md**

Documentación completa de la solución implementada.

**Contenido:**
- ? Explicación del problema y solución
- ? Cómo funciona el mapeo de variables de entorno
- ? Instrucciones paso a paso (script, manual CLI, Azure Portal)
- ? Ejemplos de verificación
- ? Referencia de formato de ASP.NET Core
- ? Troubleshooting común

---

### 4. **.Deploy/README.md** (Actualizado)

Se agregó nueva sección "Configuración de Secrets" con:
- ? Resumen de la solución implementada
- ? Comparación antes/después
- ? Instrucciones rápidas
- ? Referencia de mapeo de variables

---

## ?? **Cómo Funciona la Solución**

### **Flujo de Configuración**

```
1. Aplicación lee appsettings.json (base)
   ?
2. Aplicación lee appsettings.Stage.json (específico)
   ?
3. ASP.NET Core lee variables de entorno
   UseCase__EnableDetailedTelemetry=false
   DatabaseSettings__DefaultConnection=secretref:db-connection-string
   ?
4. Variables de entorno SOBRESCRIBEN valores del JSON
   ?
5. Configuración final en memoria ?
```

### **Ejemplo de Mapeo**

**Variable de entorno:**
```bash
DatabaseSettings__DefaultConnection=secretref:db-connection-string
```

**Se mapea a:**
```json
{
  "DatabaseSettings": {
    "DefaultConnection": "Data Source=...;Password=real_password_from_secret;..."
  }
}
```

**Acceso en C#:**
```csharp
var connectionString = builder.Configuration["DatabaseSettings:DefaultConnection"];
// Devuelve el valor real del secret, NO "secretref:..."
```

---

## ?? **Próximos Pasos**

### **1. Configurar Secrets en Azure Container Apps**

**Opción A: Usar script (RECOMENDADO)**
```powershell
cd .Deploy
.\configure-env-vars.ps1
```

**Opción B: Manual con Azure CLI**
```bash
# Crear secrets
az containerapp secret set \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --secrets \
    "db-connection-string=Data Source=platheo-stage-srvbd.database.windows.net;Initial Catalog=BD-Platheo-Template-Stage;User ID=UsrStageAdmin;Password=TU_PASSWORD_REAL;Encrypt=True;TrustServerCertificate=True" \
    "appinsights-connection-string=InstrumentationKey=xxx;IngestionEndpoint=https://xxx" \
    "payment-secret-key=sk_live_xxx"

# Configurar variables
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

---

### **2. Hacer Commit y Push**

```bash
git add .
git commit -m "fix: implementar configuración de variables de entorno con formato nativo de ASP.NET Core

- Eliminar placeholders ${VARIABLE} de appsettings.Stage.json
- Agregar script configure-env-vars.ps1 para configuración automática
- Agregar documentación completa en ENV_VARS_SOLUTION.md
- Actualizar README.md con nueva sección de configuración"

git push origin main
```

---

### **3. Ejecutar Pipeline**

El pipeline se ejecutará automáticamente al hacer push a `main`:

1. ? Construirá la imagen Docker con los nuevos `appsettings.json`
2. ? Subirá la imagen a ACR
3. ? Desplegará en Azure Container Apps
4. ? La aplicación leerá las variables de entorno correctamente

---

### **4. Verificar Despliegue**

```bash
# Obtener URL
APP_URL=$(az containerapp show \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --query "properties.configuration.ingress.fqdn" \
  -o tsv)

# Probar health check
curl https://$APP_URL/health

# Ver logs
az containerapp logs show \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --follow
```

**Buscar en logs:**
```
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Stage
```

---

## ? **Checklist de Verificación**

- [ ] `appsettings.Stage.json` sin placeholders `${VARIABLE}`
- [ ] Script `configure-env-vars.ps1` creado
- [ ] Documentación `ENV_VARS_SOLUTION.md` creada
- [ ] README.md actualizado
- [ ] Secrets creados en Azure Container Apps
- [ ] Variables de entorno configuradas con formato `__`
- [ ] Commit y push a main
- [ ] Pipeline ejecutado exitosamente
- [ ] Health check responde 200 OK
- [ ] Logs muestran "Hosting environment: Stage"
- [ ] Aplicación funciona correctamente

---

## ?? **Comparación Antes vs Después**

| Aspecto | Antes (?) | Después (?) |
|---------|-----------|-------------|
| **Formato** | Placeholders `${VAR}` | Variables de entorno nativas |
| **Soporte** | No soportado por .NET | Nativo de ASP.NET Core |
| **Configuración** | Manual en código | Azure Container Apps |
| **Secrets** | Expuestos en JSON | Referenciados desde Azure |
| **Despliegue** | Requiere modificar código | Una sola vez, persiste |
| **Mantenimiento** | Difícil | Fácil (Azure Portal/CLI) |

---

## ?? **Enlaces Útiles**

- [Documentación Completa](ENV_VARS_SOLUTION.md)
- [Script de Configuración](configure-env-vars.ps1)
- [Pipeline Stage](azure-pipelines-stage.md)
- [ASP.NET Core Configuration Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

---

**Fecha de implementación**: 2025-01-13
**Estado**: ? Completado y listo para desplegar
**Autor**: Platheo DevOps Team
