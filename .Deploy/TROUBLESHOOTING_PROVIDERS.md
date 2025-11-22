# ?? Troubleshooting: Microsoft.App Provider No Registrado

## ? Error

```
ERROR: Subscription 7430bafe-bc29-443c-93d1-a8cb3090136c is not registered 
for the Microsoft.App resource provider. 

Please run "az provider register -n Microsoft.App --wait" to register your subscription.
```

## ?? Causa

Azure requiere que los **resource providers** estén registrados en la suscripción antes de poder usarlos. El provider `Microsoft.App` es necesario para:
- Azure Container Apps
- Azure Container Apps Environment
- Container App Jobs

---

## ? Solución Automática (Implementada en Pipeline)

La pipeline ahora incluye un paso que:
1. ? Verifica si `Microsoft.App` ya está registrado
2. ? Si no está registrado, lo registra automáticamente
3. ? Espera a que el registro se complete
4. ? Continúa con el deployment

**No requiere acción manual** en futuros deployments.

---

## ?? Solución Manual (Una Sola Vez)

Si prefieres registrarlo manualmente antes de ejecutar la pipeline:

### Opción 1: Azure CLI

```powershell
# Login a Azure
az login

# Seleccionar suscripción
az account set --subscription "7430bafe-bc29-443c-93d1-a8cb3090136c"

# Registrar provider
az provider register --namespace Microsoft.App --wait

# Verificar registro
az provider show --namespace Microsoft.App --query "registrationState" -o tsv
```

**Output esperado:** `Registered`

### Opción 2: Azure Portal

1. Ve a Azure Portal ? **Subscriptions**
2. Selecciona: **Suscripción Platheo**
3. En el menú izquierdo: **Resource providers**
4. Busca: `Microsoft.App`
5. Si está "NotRegistered", click en **Register**
6. Espera a que cambie a **Registered** (~1-2 minutos)

### Opción 3: Azure PowerShell

```powershell
# Connect a Azure
Connect-AzAccount

# Seleccionar suscripción
Select-AzSubscription -SubscriptionId "7430bafe-bc29-443c-93d1-a8cb3090136c"

# Registrar provider
Register-AzResourceProvider -ProviderNamespace "Microsoft.App"

# Verificar
Get-AzResourceProvider -ProviderNamespace "Microsoft.App" | Select-Object RegistrationState
```

---

## ?? Providers Comunes para Azure Container Apps

| Provider | Para qué sirve | Estado esperado |
|----------|----------------|-----------------|
| `Microsoft.App` | Azure Container Apps | ? Registered |
| `Microsoft.ContainerRegistry` | Azure Container Registry (ACR) | ? Registered |
| `Microsoft.OperationalInsights` | Log Analytics (logs de Container Apps) | ? Registered |
| `Microsoft.Insights` | Application Insights | ? Registered |

---

## ?? Verificar Providers Registrados

### Listar todos los providers

```powershell
# Ver todos los providers
az provider list --query "[].{Namespace:namespace, State:registrationState}" -o table

# Solo los registrados
az provider list --query "[?registrationState=='Registered'].namespace" -o tsv

# Solo los de Azure Container Apps y relacionados
az provider list --query "[?contains(namespace, 'App') || contains(namespace, 'Container')].{Namespace:namespace, State:registrationState}" -o table
```

### Verificar provider específico

```powershell
# Microsoft.App
az provider show --namespace Microsoft.App --query "{Namespace:namespace, State:registrationState}" -o table

# Microsoft.ContainerRegistry
az provider show --namespace Microsoft.ContainerRegistry --query "{Namespace:namespace, State:registrationState}" -o table
```

---

## ?? Registrar Múltiples Providers

Si necesitas registrar varios providers a la vez:

```powershell
# Lista de providers necesarios
$providers = @(
    "Microsoft.App",
    "Microsoft.ContainerRegistry",
    "Microsoft.OperationalInsights",
    "Microsoft.Insights"
)

foreach ($provider in $providers) {
    Write-Host "Registrando $provider..." -ForegroundColor Cyan
    az provider register --namespace $provider
}

# Esperar a que todos estén registrados
Write-Host "Esperando a que los providers se registren..." -ForegroundColor Yellow
foreach ($provider in $providers) {
    az provider register --namespace $provider --wait
    Write-Host "? $provider registrado" -ForegroundColor Green
}
```

---

## ?? Tiempo de Registro

| Acción | Tiempo Estimado |
|--------|-----------------|
| Registro sin `--wait` | Inmediato (asíncrono) |
| Registro con `--wait` | 1-3 minutos |
| Verificación posterior | Instantáneo |

---

## ?? En la Pipeline

La pipeline ahora incluye este step:

```yaml
- task: AzureCLI@2
  displayName: 'Register Microsoft.App Provider'
  inputs:
    azureSubscription: '$(azureSubscription)'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      echo "?? Verificando registro de Microsoft.App provider..."
      
      # Verificar si ya está registrado
      PROVIDER_STATE=$(az provider show --namespace Microsoft.App --query "registrationState" -o tsv 2>/dev/null || echo "NotRegistered")
      
      if [ "$PROVIDER_STATE" == "Registered" ]; then
        echo "? Microsoft.App provider ya está registrado"
      else
        echo "??  Microsoft.App provider no está registrado"
        echo "?? Registrando provider..."
        az provider register --namespace Microsoft.App --wait
        echo "? Microsoft.App provider registrado exitosamente"
      fi
```

**Ventajas:**
- ? Registro automático la primera vez
- ? No impacta futuros deployments (verifica primero)
- ? No requiere intervención manual

---

## ?? Script de Verificación

Guardar como `.Deploy/verify-providers.ps1`:

```powershell
# ========================================
# Verify Azure Providers Script
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId = "7430bafe-bc29-443c-93d1-a8cb3090136c"
)

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     ?? Verificación de Azure Providers                 ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Providers necesarios
$requiredProviders = @(
    @{ Name = "Microsoft.App"; Description = "Azure Container Apps" },
    @{ Name = "Microsoft.ContainerRegistry"; Description = "Azure Container Registry" },
    @{ Name = "Microsoft.OperationalInsights"; Description = "Log Analytics" },
    @{ Name = "Microsoft.Insights"; Description = "Application Insights" }
)

# Seleccionar suscripción
az account set --subscription $SubscriptionId

Write-Host "?? Verificando providers necesarios..." -ForegroundColor Yellow
Write-Host ""

$allRegistered = $true

foreach ($provider in $requiredProviders) {
    $state = az provider show --namespace $provider.Name --query "registrationState" -o tsv 2>$null
    
    $status = if ($state -eq "Registered") { "?" } else { "?" }
    $color = if ($state -eq "Registered") { "Green" } else { "Red" }
    
    Write-Host "  $status $($provider.Name)" -ForegroundColor $color
    Write-Host "     $($provider.Description)" -ForegroundColor Gray
    Write-Host "     Estado: $state" -ForegroundColor Gray
    Write-Host ""
    
    if ($state -ne "Registered") {
        $allRegistered = $false
    }
}

if ($allRegistered) {
    Write-Host "? Todos los providers están registrados" -ForegroundColor Green
} else {
    Write-Host "??  Algunos providers no están registrados" -ForegroundColor Yellow
    Write-Host ""
    $register = Read-Host "¿Deseas registrar los providers faltantes? (Y/n)"
    
    if ($register -eq "" -or $register -eq "Y" -or $register -eq "y") {
        Write-Host ""
        Write-Host "?? Registrando providers..." -ForegroundColor Cyan
        
        foreach ($provider in $requiredProviders) {
            $state = az provider show --namespace $provider.Name --query "registrationState" -o tsv 2>$null
            
            if ($state -ne "Registered") {
                Write-Host "  Registrando $($provider.Name)..." -ForegroundColor Yellow
                az provider register --namespace $provider.Name --wait
                Write-Host "  ? $($provider.Name) registrado" -ForegroundColor Green
            }
        }
        
        Write-Host ""
        Write-Host "? Todos los providers registrados exitosamente" -ForegroundColor Green
    }
}

Write-Host ""
```

**Uso:**
```powershell
.\.Deploy\verify-providers.ps1
```

---

## ?? Troubleshooting

### Error: "Insufficient permissions"

**Causa:** No tienes permisos para registrar providers.

**Solución:**
- Necesitas rol de **Owner** o **Contributor** en la suscripción
- O solicita al administrador que registre el provider

### Error: "Provider registration failed"

**Causa:** Error temporal de Azure.

**Solución:**
1. Espera unos minutos
2. Intenta nuevamente:
```powershell
az provider register --namespace Microsoft.App --wait
```

### Provider en estado "Registering"

**Causa:** El registro está en proceso.

**Solución:**
1. Espera 1-3 minutos
2. Verifica el estado:
```powershell
az provider show --namespace Microsoft.App --query "registrationState" -o tsv
```

---

## ?? Referencias

- [Azure Resource Providers Documentation](https://docs.microsoft.com/azure/azure-resource-manager/management/resource-providers-and-types)
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [Register Resource Provider CLI](https://docs.microsoft.com/cli/azure/provider)

---

**Última actualización**: 11/01/2025 - v1.0
**Problema**: Microsoft.App provider no registrado
**Solución**: Registro automático en pipeline
