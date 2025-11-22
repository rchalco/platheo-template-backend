# ?? Troubleshooting: Resource Group Name con Espacios

## ? Problema

```
ERROR: unrecognized arguments: Platheo

Command:
az containerapp show --name ca-platheotemplate-stg --resource-group Suscripción Platheo
```

## ?? Causa

El Resource Group `Suscripción Platheo` tiene:
- ? **Espacio** entre palabras
- ? **Acento** en "Suscripción"

Bash interpreta esto como dos argumentos separados:
1. `--resource-group Suscripción`
2. `Platheo` (argumento no reconocido)

---

## ? Soluciones

### Opción 1: Verificar el Nombre Real del Resource Group

```powershell
# Listar todos los resource groups
az group list --query "[].name" -o table

# Buscar el resource group que contiene el Container App
az containerapp show --name ca-platheotemplate-stg --query "resourceGroup" -o tsv
```

**Posibles nombres reales:**
- `Platheo-template` ?
- `platheo-template` ?
- `Suscripción Platheo` ?? (con espacio - requiere comillas)
- `SuscripcionPlatheo` ?

### Opción 2: Usar Comillas en el Comando (Si el nombre tiene espacios)

Si el Resource Group realmente se llama `Suscripción Platheo`:

```yaml
inlineScript: |
  az containerapp show \
    --name $(containerAppName) \
    --resource-group "$(resourceGroup)" \  # Comillas dobles
    --query "properties.configuration.ingress.fqdn" \
    -o tsv
```

### Opción 3: Cambiar el Valor de la Variable (Recomendado)

En la pipeline, cambiar a:

```yaml
variables:
  resourceGroup: 'Platheo-template'  # Sin espacios ni acentos
```

---

## ?? Cómo Verificar el Nombre Real

### Método 1: Azure Portal

1. Ve a Azure Portal
2. Busca `ca-platheotemplate-stg`
3. En la página del Container App, verás el Resource Group en la parte superior

### Método 2: Azure CLI

```powershell
# Método 1: Buscar el Container App
az resource list \
  --resource-type "Microsoft.App/containerApps" \
  --name "ca-platheotemplate-stg" \
  --query "[0].resourceGroup" \
  -o tsv

# Método 2: Listar todos los RG y buscar el correcto
az group list --query "[?contains(name, 'platheo') || contains(name, 'Platheo')].name" -o table
```

### Método 3: Azure DevOps (desde la pipeline fallida)

Revisar los logs anteriores donde se creó el Container App:

```bash
# En logs de creación, buscar:
"resourceGroup": "nombre-real-del-rg"
```

---

## ?? Actualización de la Pipeline

### Caso 1: Resource Group sin espacios

```yaml
variables:
  resourceGroup: 'Platheo-template'  # ? Nombre correcto
```

### Caso 2: Resource Group con espacios (no recomendado)

```yaml
variables:
  resourceGroup: 'Suscripción Platheo'  # ?? Con espacio

# Y en los comandos bash, usar comillas:
inlineScript: |
  APP_URL=$(az containerapp show \
    --name "$(containerAppName)" \
    --resource-group "$(resourceGroup)" \  # ? Comillas dobles
    --query "properties.configuration.ingress.fqdn" \
    -o tsv)
```

---

## ? Recomendaciones

### 1. Nombres sin Espacios

```
? Platheo-template
? platheo-template
? rg-platheo-template-stg
? Suscripción Platheo (espacio + acento)
```

### 2. Convención de Nombres

```
[prefijo]-[proyecto]-[recurso]-[ambiente]

Ejemplos:
- rg-platheo-template-stage
- rg-platheo-template-prod
```

### 3. Si ya existe con espacios

Si el Resource Group ya existe y tiene espacios:
- **Opción A**: Usar comillas en todos los comandos bash ??
- **Opción B**: Renombrar el RG (requiere mover recursos) ??
- **Opción C**: Crear nuevo RG y migrar Container App ??

---

## ?? Verificación

### Script de Verificación

```powershell
# Guardar como .Deploy/verify-resource-group.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$ContainerAppName
)

Write-Host "?? Buscando Resource Group del Container App..." -ForegroundColor Cyan

# Método 1: Buscar por Container App
$rg = az resource list `
    --resource-type "Microsoft.App/containerApps" `
    --name $ContainerAppName `
    --query "[0].resourceGroup" `
    -o tsv

if ($rg) {
    Write-Host "? Resource Group encontrado: $rg" -ForegroundColor Green
    
    # Verificar si tiene espacios
    if ($rg -match '\s') {
        Write-Host "??  WARNING: El Resource Group contiene espacios" -ForegroundColor Yellow
        Write-Host "   Recomendación: Usar comillas en comandos bash" -ForegroundColor Yellow
    } else {
        Write-Host "? El Resource Group NO tiene espacios" -ForegroundColor Green
    }
    
    # Mostrar información del Container App
    Write-Host ""
    Write-Host "?? Información del Container App:" -ForegroundColor Cyan
    az containerapp show `
        --name $ContainerAppName `
        --resource-group $rg `
        --query "{Name:name, ResourceGroup:resourceGroup, Location:location, FQDN:properties.configuration.ingress.fqdn}" `
        -o table
        
} else {
    Write-Host "? Container App no encontrado: $ContainerAppName" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Sugerencia: Listar Container Apps disponibles:" -ForegroundColor Yellow
    az containerapp list --query "[].{Name:name, ResourceGroup:resourceGroup}" -o table
}
```

**Uso:**
```powershell
.\.Deploy\verify-resource-group.ps1 -ContainerAppName "ca-platheotemplate-stg"
```

---

## ?? Checklist de Corrección

- [ ] Verificar nombre real del Resource Group
- [ ] Actualizar variable `resourceGroup` en pipeline
- [ ] Si tiene espacios, agregar comillas en comandos bash
- [ ] Commit y push de cambios
- [ ] Re-ejecutar pipeline
- [ ] Verificar que el deployment funciona

---

## ?? Comandos Útiles

### Listar Resource Groups

```powershell
# Todos los RG
az group list --query "[].name" -o table

# Solo los de Platheo
az group list --query "[?contains(name, 'platheo') || contains(name, 'Platheo')].{Name:name, Location:location}" -o table
```

### Buscar Container App

```powershell
# Por nombre
az containerapp list --query "[?name=='ca-platheotemplate-stg'].{Name:name, RG:resourceGroup, FQDN:properties.configuration.ingress.fqdn}" -o table

# Todos en la suscripción
az containerapp list --query "[].{Name:name, RG:resourceGroup}" -o table
```

### Obtener Resource Group del Container App

```powershell
# Método directo
az resource show \
  --ids "/subscriptions/7430bafe-bc29-443c-93d1-a8cb3090136c/resourceGroups/*/providers/Microsoft.App/containerApps/ca-platheotemplate-stg" \
  --query "resourceGroup" \
  -o tsv
```

---

**Última actualización**: 11/01/2025 - v1.0
**Problema**: Resource Group con espacios en nombre
**Solución**: Verificar nombre real y usar sin espacios en variables
