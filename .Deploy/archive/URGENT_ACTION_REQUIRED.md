# ?? ACCIÓN REQUERIDA URGENTE - Health Check Falla

## ? **Problema Actual**

Tu deployment está fallando con:
```
Health check retornó HTTP 404
```

**Causa:** Las variables de entorno NO están configuradas en Azure Container Apps.

---

## ? **Solución en 3 Pasos**

### **Paso 1: Configurar Variables en Azure (5 minutos)**

Ejecuta este script:

```powershell
cd .Deploy
.\configure-env-vars.ps1
```

**El script te preguntará:**

```
?? ¿Deseas crear el secret ahora? (y/N)
```
**Responde:** `y`

```
?? Ingresa el password de la base de datos:
```
**Ingresa:** El password real de la BD de Stage (se oculta al escribir)

```
¿Deseas continuar? (y/N)
```
**Responde:** `y`

**El script creará:**
- ? Secret `db-connection-string` con el connection string completo
- ? Variable `DatabaseSettings__DefaultConnection=secretref:db-connection-string`
- ? Variable `ASPNETCORE_ENVIRONMENT=Stage`
- ? Variable `UseCase__EnableDetailedTelemetry=false`

---

### **Paso 2: Hacer Commit y Push**

```bash
git add .
git commit -m "fix: agregar validación de connection string y configurar variables de entorno"
git push origin main
```

**El pipeline se ejecutará automáticamente.**

---

### **Paso 3: Verificar**

Después de que el pipeline termine (2-5 minutos):

```bash
# Obtener URL
APP_URL=$(az containerapp show --name ca-platheotemplate-stg --resource-group Platheo-tempalte --query "properties.configuration.ingress.fqdn" -o tsv)

# Probar health check
curl https://$APP_URL/health
```

**Resultado esperado:** `Healthy` (HTTP 200)

---

## ?? **¿Por Qué Estaba Fallando?**

### **Antes (? Fallaba):**

1. `appsettings.Stage.json` tiene `DefaultConnection: ""`
2. Azure Container Apps NO tenía configurado `DatabaseSettings__DefaultConnection`
3. La app intentaba iniciar sin connection string
4. Posible fallo al registrar UserCases
5. Endpoint `/health` no disponible ? **HTTP 404**

### **Ahora (? Funciona):**

1. `appsettings.Stage.json` tiene `DefaultConnection: ""`
2. Azure Container Apps **SÍ tiene** `DatabaseSettings__DefaultConnection=secretref:db-connection-string`
3. Azure inyecta el valor real del secret al iniciar
4. `Program.cs` valida que existe el connection string
5. Registra UserCases correctamente
6. Endpoint `/health` responde ? **HTTP 200 ?**

---

## ?? **Cambios Realizados en el Código**

### **1. Program.cs - Validación de Connection String**

```csharp
// ? NUEVO: Solo registrar UserCases si el connection string está configurado
if (!string.IsNullOrEmpty(databaseSettings?.DefaultConnection))
{
    var serviceUserCase = builder.Services.RegisterUserCases(userCaseTypes, databaseSettings!);
}
```

**Esto permite que la app inicie incluso sin BD configurada** (útil para debugging).

---

### **2. Program.cs - Health Check Explícito**

```csharp
// ? NUEVO: Health check explícito PRIMERO
app.MapHealthChecks("/health");
app.MapDefaultEndpoints();
```

**Esto asegura que `/health` siempre funciona**, incluso si hay problemas con otros endpoints.

---

### **3. configure-env-vars.ps1 - Crear Secret Interactivamente**

El script ahora:
- ? Detecta si el secret existe
- ? Ofrece crearlo si no existe
- ? Pide el password de forma segura
- ? Configura todo automáticamente

---

## ?? **Si el Script No Funciona**

### **Configuración Manual con Azure CLI:**

```bash
# 1. Crear secret
az containerapp secret set \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --secrets "db-connection-string=Data Source=platheo-stage-srvbd.database.windows.net;Initial Catalog=BD-Platheo-Template-Stage;User ID=UsrStageAdmin;Password=TU_PASSWORD_REAL;Encrypt=True;TrustServerCertificate=True"

# 2. Configurar variables
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --set-env-vars \
    "ASPNETCORE_ENVIRONMENT=Stage" \
    "UseCase__EnableDetailedTelemetry=false" \
    "DatabaseSettings__DefaultConnection=secretref:db-connection-string"
```

---

## ?? **Documentación Adicional**

- **Explicación completa del problema:** [HEALTH_CHECK_FIX.md](.Deploy/HEALTH_CHECK_FIX.md)
- **Guía de variables de entorno:** [ENV_VARS_SOLUTION.md](.Deploy/ENV_VARS_SOLUTION.md)
- **Resumen de cambios:** [CHANGES_SUMMARY.md](.Deploy/CHANGES_SUMMARY.md)

---

## ? **Checklist Final**

- [ ] Ejecuté `configure-env-vars.ps1`
- [ ] Creé el secret `db-connection-string` con el password real
- [ ] Verifiqué que las variables están configuradas
- [ ] Hice commit de los cambios
- [ ] Hice push a main
- [ ] El pipeline se ejecutó exitosamente
- [ ] Health check responde HTTP 200
- [ ] La aplicación funciona correctamente

---

**¡EJECUTA EL SCRIPT AHORA!** ??

```powershell
cd .Deploy
.\configure-env-vars.ps1
```
