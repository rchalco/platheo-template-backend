# ========================================
# Pre-Deployment Verification Script
# ========================================
# 
# Este script verifica que toda la configuración
# necesaria esté correcta antes de ejecutar la pipeline
#
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Stage"
)

$ErrorActionPreference = "Continue"

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     ?? Pre-Deployment Verification - $Environment       ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Variables de configuración
$subscriptionId = "7430bafe-bc29-443c-93d1-a8cb3090136c"
$resourceGroup = "Suscripción Platheo"
$acrName = "acrplatheotemplatestg"
$containerAppName = "ca-platheotemplate-stg"

$errors = @()
$warnings = @()

# ========================================
# 1. Verificar Azure CLI
# ========================================
Write-Host "1??  Verificando Azure CLI..." -ForegroundColor Yellow

try {
    $azVersion = az --version 2>&1 | Select-String "azure-cli" | Select-Object -First 1
    if ($azVersion) {
        Write-Host "   ? Azure CLI instalado: $azVersion" -ForegroundColor Green
    } else {
        $errors += "Azure CLI no está instalado"
        Write-Host "   ? Azure CLI no encontrado" -ForegroundColor Red
    }
} catch {
    $errors += "Error verificando Azure CLI"
    Write-Host "   ? Error: $_" -ForegroundColor Red
}

# ========================================
# 2. Verificar Login de Azure
# ========================================
Write-Host ""
Write-Host "2??  Verificando sesión de Azure..." -ForegroundColor Yellow

try {
    $account = az account show -o json 2>$null | ConvertFrom-Json
    if ($account) {
        Write-Host "   ? Sesión activa" -ForegroundColor Green
        Write-Host "      Usuario: $($account.user.name)" -ForegroundColor Gray
        Write-Host "      Suscripción: $($account.name)" -ForegroundColor Gray
        
        # Verificar que sea la suscripción correcta
        if ($account.id -ne $subscriptionId) {
            $warnings += "Suscripción activa no es la esperada"
            Write-Host "   ??  Suscripción activa no coincide con la configurada" -ForegroundColor Yellow
            Write-Host "      Esperada: $subscriptionId" -ForegroundColor Gray
            Write-Host "      Actual: $($account.id)" -ForegroundColor Gray
        }
    } else {
        $errors += "No hay sesión activa en Azure"
        Write-Host "   ? No hay sesión activa" -ForegroundColor Red
    }
} catch {
    $errors += "Error verificando sesión de Azure"
    Write-Host "   ? Error: $_" -ForegroundColor Red
}

# ========================================
# 3. Verificar Azure Container Registry
# ========================================
Write-Host ""
Write-Host "3??  Verificando Azure Container Registry..." -ForegroundColor Yellow

try {
    $acr = az acr show --name $acrName --resource-group $resourceGroup -o json 2>$null | ConvertFrom-Json
    if ($acr) {
        Write-Host "   ? ACR encontrado: $($acr.name)" -ForegroundColor Green
        Write-Host "      Login Server: $($acr.loginServer)" -ForegroundColor Gray
        Write-Host "      SKU: $($acr.sku.name)" -ForegroundColor Gray
        Write-Host "      Estado: $($acr.provisioningState)" -ForegroundColor Gray
        
        # Verificar que admin está habilitado
        if ($acr.adminUserEnabled) {
            Write-Host "      Admin: Habilitado ?" -ForegroundColor Gray
        } else {
            $warnings += "Admin user no está habilitado en ACR"
            Write-Host "      Admin: Deshabilitado ??" -ForegroundColor Yellow
        }
    } else {
        $errors += "ACR no encontrado"
        Write-Host "   ? ACR no encontrado" -ForegroundColor Red
    }
    
    # Listar imágenes en ACR
    Write-Host ""
    Write-Host "   ?? Imágenes en ACR:" -ForegroundColor Cyan
    $repos = az acr repository list --name $acrName -o json 2>$null | ConvertFrom-Json
    if ($repos) {
        foreach ($repo in $repos) {
            Write-Host "      - $repo" -ForegroundColor Gray
            $tags = az acr repository show-tags --name $acrName --repository $repo --orderby time_desc --top 5 -o json 2>$null | ConvertFrom-Json
            if ($tags) {
                Write-Host "        Tags: $($tags -join ', ')" -ForegroundColor DarkGray
            }
        }
    } else {
        Write-Host "      (vacío)" -ForegroundColor DarkGray
    }
    
} catch {
    $errors += "Error verificando ACR"
    Write-Host "   ? Error: $_" -ForegroundColor Red
}

# ========================================
# 4. Verificar Azure Container App
# ========================================
Write-Host ""
Write-Host "4??  Verificando Azure Container App..." -ForegroundColor Yellow

try {
    $app = az containerapp show --name $containerAppName --resource-group $resourceGroup -o json 2>$null | ConvertFrom-Json
    if ($app) {
        Write-Host "   ? Container App encontrado: $($app.name)" -ForegroundColor Green
        Write-Host "      FQDN: $($app.properties.configuration.ingress.fqdn)" -ForegroundColor Gray
        Write-Host "      Estado: $($app.properties.provisioningState)" -ForegroundColor Gray
        
        # Imagen actual
        $currentImage = $app.properties.template.containers[0].image
        Write-Host "      Imagen actual: $currentImage" -ForegroundColor Gray
        
        # Variables de entorno
        Write-Host ""
        Write-Host "   ?? Variables de entorno configuradas:" -ForegroundColor Cyan
        $envVars = $app.properties.template.containers[0].env
        foreach ($env in $envVars) {
            if ($env.secretRef) {
                Write-Host "      - $($env.name): (secreto) ? $($env.secretRef)" -ForegroundColor DarkGray
            } else {
                Write-Host "      - $($env.name): $($env.value)" -ForegroundColor DarkGray
            }
        }
        
        # Revisar secrets
        Write-Host ""
        Write-Host "   ?? Secrets configurados:" -ForegroundColor Cyan
        $secrets = az containerapp secret list --name $containerAppName --resource-group $resourceGroup -o json 2>$null | ConvertFrom-Json
        if ($secrets) {
            foreach ($secret in $secrets) {
                Write-Host "      - $($secret.name)" -ForegroundColor DarkGray
            }
        } else {
            $warnings += "No hay secrets configurados en Container App"
            Write-Host "      (ninguno)" -ForegroundColor Yellow
        }
        
        # Revisiones activas
        Write-Host ""
        Write-Host "   ?? Revisiones activas:" -ForegroundColor Cyan
        $revisions = az containerapp revision list --name $containerAppName --resource-group $resourceGroup --query "[?properties.active]" -o json 2>$null | ConvertFrom-Json
        if ($revisions) {
            foreach ($rev in $revisions) {
                $trafficWeight = if ($rev.properties.trafficWeight) { $rev.properties.trafficWeight } else { 0 }
                Write-Host "      - $($rev.name) (Traffic: $trafficWeight%)" -ForegroundColor DarkGray
            }
        }
        
    } else {
        $errors += "Container App no encontrado"
        Write-Host "   ? Container App no encontrado" -ForegroundColor Red
    }
} catch {
    $errors += "Error verificando Container App"
    Write-Host "   ? Error: $_" -ForegroundColor Red
}

# ========================================
# 5. Verificar Dockerfile
# ========================================
Write-Host ""
Write-Host "5??  Verificando Dockerfile..." -ForegroundColor Yellow

$dockerfilePath = "VectorStinger.Container/Dockerfile"
if (Test-Path $dockerfilePath) {
    Write-Host "   ? Dockerfile encontrado: $dockerfilePath" -ForegroundColor Green
    
    # Verificar que el Dockerfile tiene el ARG correcto
    $dockerfileContent = Get-Content $dockerfilePath -Raw
    if ($dockerfileContent -match "ASPNETCORE_ENVIRONMENT") {
        Write-Host "      ? ARG ASPNETCORE_ENVIRONMENT configurado" -ForegroundColor Green
    } else {
        $warnings += "ARG ASPNETCORE_ENVIRONMENT no encontrado en Dockerfile"
        Write-Host "      ??  ARG ASPNETCORE_ENVIRONMENT no encontrado" -ForegroundColor Yellow
    }
} else {
    $errors += "Dockerfile no encontrado"
    Write-Host "   ? Dockerfile no encontrado: $dockerfilePath" -ForegroundColor Red
}

# ========================================
# 6. Verificar appsettings.Stage.json
# ========================================
Write-Host ""
Write-Host "6??  Verificando appsettings.Stage.json..." -ForegroundColor Yellow

$appsettingsPath = "VectorStinger.Api.Service/appsettings.Stage.json"
if (Test-Path $appsettingsPath) {
    Write-Host "   ? appsettings.Stage.json encontrado" -ForegroundColor Green
    
    # Verificar contenido
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    
    # Verificar placeholders
    $placeholders = @()
    $content = Get-Content $appsettingsPath -Raw
    if ($content -match '\$\{(\w+)\}') {
        $Matches[1..($Matches.Count-1)] | ForEach-Object {
            $placeholders += $_
        }
        Write-Host "      Placeholders encontrados:" -ForegroundColor Cyan
        foreach ($ph in $placeholders) {
            Write-Host "      - `${$ph}" -ForegroundColor DarkGray
        }
    }
} else {
    $warnings += "appsettings.Stage.json no encontrado"
    Write-Host "   ??  appsettings.Stage.json no encontrado: $appsettingsPath" -ForegroundColor Yellow
}

# ========================================
# 7. Verificar Pipeline YAML
# ========================================
Write-Host ""
Write-Host "7??  Verificando pipeline YAML..." -ForegroundColor Yellow

$pipelinePath = ".Deploy/azure-pipelines-stage.yml"
if (Test-Path $pipelinePath) {
    Write-Host "   ? Pipeline encontrado: $pipelinePath" -ForegroundColor Green
    
    # Verificar variables en el pipeline
    $pipelineContent = Get-Content $pipelinePath -Raw
    $requiredVars = @(
        'azureSubscription',
        'acrName',
        'imageName',
        'containerAppName'
    )
    
    foreach ($var in $requiredVars) {
        if ($pipelineContent -match "$var:") {
            Write-Host "      ? Variable $var configurada" -ForegroundColor Green
        } else {
            $warnings += "Variable $var no encontrada en pipeline"
            Write-Host "      ??  Variable $var no encontrada" -ForegroundColor Yellow
        }
    }
} else {
    $errors += "Pipeline YAML no encontrado"
    Write-Host "   ? Pipeline no encontrado: $pipelinePath" -ForegroundColor Red
}

# ========================================
# Resumen Final
# ========================================
Write-Host ""
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?                  ?? Resumen Final                      ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

if ($errors.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "? Todo está configurado correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Puedes ejecutar la pipeline con confianza" -ForegroundColor Green
    exit 0
} else {
    if ($errors.Count -gt 0) {
        Write-Host "? Errores encontrados ($($errors.Count)):" -ForegroundColor Red
        foreach ($error in $errors) {
            Write-Host "   - $error" -ForegroundColor Red
        }
        Write-Host ""
    }
    
    if ($warnings.Count -gt 0) {
        Write-Host "??  Advertencias encontradas ($($warnings.Count)):" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "   - $warning" -ForegroundColor Yellow
        }
        Write-Host ""
    }
    
    if ($errors.Count -gt 0) {
        Write-Host "? Corrige los errores antes de ejecutar la pipeline" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "??  Puedes continuar, pero revisa las advertencias" -ForegroundColor Yellow
        exit 0
    }
}
