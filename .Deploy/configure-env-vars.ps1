# ========================================
# Script: Configurar Variables de Entorno en Azure Container Apps
# ========================================
# 
# Este script configura todas las variables de entorno necesarias
# en Azure Container Apps usando el formato nativo de ASP.NET Core
# (doble guión __ para jerarquías JSON)
#
# USO:
#   .\configure-env-vars.ps1
#
# PREREQUISITOS:
#   - Azure CLI instalado y configurado
#   - Permisos en la suscripción y resource group
#   - Los secrets ya deben estar creados en Container App
#
# ========================================

param(
    [string]$ContainerAppName = "ca-platheotemplate-stg",
    [string]$ResourceGroup = "Platheo-tempalte",
    [string]$Environment = "Stage",
    
    # Connection string parameters (solo para crear el secret)
    [string]$DbServer = "platheo-stage-srvbd.database.windows.net",
    [string]$DbName = "BD-Platheo-Template-Stage",
    [string]$DbUser = "UsrStageAdmin",
    [string]$DbPassword = ""  # Se pedirá interactivamente si no se proporciona
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configurando Variables de Entorno" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que Azure CLI está instalado
Write-Host "🔍 Verificando Azure CLI..." -ForegroundColor Yellow
$azVersion = az version --query '\"azure-cli\"' -o tsv 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Azure CLI no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host "   Instalar desde: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Azure CLI versión: $azVersion" -ForegroundColor Green
Write-Host ""

# Verificar que el usuario está autenticado
Write-Host "🔍 Verificando autenticación..." -ForegroundColor Yellow
$accountInfo = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ No estás autenticado en Azure" -ForegroundColor Red
    Write-Host "   Ejecuta: az login" -ForegroundColor Yellow
    exit 1
}
$subscriptionName = az account show --query "name" -o tsv
Write-Host "✅ Autenticado en: $subscriptionName" -ForegroundColor Green
Write-Host ""

# Verificar que el Container App existe
Write-Host "🔍 Verificando Container App..." -ForegroundColor Yellow
$appExists = az containerapp show --name $ContainerAppName --resource-group $ResourceGroup --query "name" -o tsv 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Container App '$ContainerAppName' no encontrado en resource group '$ResourceGroup'" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Container App encontrado: $appExists" -ForegroundColor Green
Write-Host ""

# Verificar secretos existentes
Write-Host "🔍 Verificando secretos configurados..." -ForegroundColor Yellow
$secrets = az containerapp secret list --name $ContainerAppName --resource-group $ResourceGroup -o json | ConvertFrom-Json

Write-Host ""
Write-Host "📋 Secretos actuales:" -ForegroundColor Cyan
if ($secrets.Count -eq 0) {
    Write-Host "   ⚠️  No hay secretos configurados" -ForegroundColor Yellow
} else {
    foreach ($secret in $secrets) {
        Write-Host "   ✅ $($secret.name)" -ForegroundColor Green
    }
}
Write-Host ""

# Verificar si el secret db-connection-string existe
$dbSecretExists = $secrets | Where-Object { $_.name -eq "db-connection-string" }

if (-not $dbSecretExists) {
    Write-Host "⚠️  Secret 'db-connection-string' NO existe" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "🔧 ¿Deseas crear el secret ahora? (y/N)" -ForegroundColor Yellow
    $createSecret = Read-Host
    
    if ($createSecret -eq "y" -or $createSecret -eq "Y") {
        Write-Host ""
        Write-Host "📝 Configuración del Connection String:" -ForegroundColor Cyan
        Write-Host "   Server: $DbServer" -ForegroundColor Gray
        Write-Host "   Database: $DbName" -ForegroundColor Gray
        Write-Host "   User: $DbUser" -ForegroundColor Gray
        Write-Host ""
        
        if ([string]::IsNullOrEmpty($DbPassword)) {
            Write-Host "🔑 Ingresa el password de la base de datos:" -ForegroundColor Yellow
            $securePassword = Read-Host -AsSecureString
            $DbPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
            )
        }
        
        $connectionString = "Data Source=$DbServer;Initial Catalog=$DbName;User ID=$DbUser;Password=$DbPassword;Encrypt=True;TrustServerCertificate=True;Command Timeout=0"
        
        Write-Host ""
        Write-Host "🔧 Creando secret 'db-connection-string'..." -ForegroundColor Yellow
        
        az containerapp secret set `
            --name $ContainerAppName `
            --resource-group $ResourceGroup `
            --secrets "db-connection-string=$connectionString"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Secret creado exitosamente" -ForegroundColor Green
        } else {
            Write-Host "❌ Error al crear secret" -ForegroundColor Red
            exit 1
        }
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "⚠️  IMPORTANTE: Sin el secret 'db-connection-string', la aplicación NO funcionará correctamente" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "   Para crear el secret manualmente:" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "   az containerapp secret set \" -ForegroundColor Cyan
        Write-Host "     --name $ContainerAppName \" -ForegroundColor Cyan
        Write-Host "     --resource-group $ResourceGroup \" -ForegroundColor Cyan
        Write-Host "     --secrets `"db-connection-string=Data Source=...;Password=TU_PASSWORD;...`"" -ForegroundColor Cyan
        Write-Host ""
        
        $continue = Read-Host "¿Deseas continuar de todos modos? (y/N)"
        if ($continue -ne "y" -and $continue -ne "Y") {
            exit 0
        }
    }
}

Write-Host ""

# Preparar variables de entorno
Write-Host "🔧 Preparando variables de entorno..." -ForegroundColor Yellow
Write-Host ""

# IMPORTANTE: Usar formato de ASP.NET Core con doble guión bajo __
# Este formato mappa directamente a la jerarquía del JSON:
#   UseCase__EnableDetailedTelemetry → UseCase:EnableDetailedTelemetry
#   DatabaseSettings__DefaultConnection → DatabaseSettings:DefaultConnection

$envVars = @(
    # Variable principal de ambiente
    "ASPNETCORE_ENVIRONMENT=$Environment",
    
    # UseCase settings
    "UseCase__EnableDetailedTelemetry=false",
    
    # Database settings - Referencia al secret
    "DatabaseSettings__DefaultConnection=secretref:db-connection-string"
    
    # Payment Bridge settings (opcional - solo si el secret existe)
phtabletonedesktop    # "PaymentBridgeSettings__SecretKey=secretref:payment-secret-key"
    
    # Application Insights (opcional - solo si el secret existe)
    # "APPLICATIONINSIGHTS_CONNECTION_STRING=secretref:appinsights-connection-string"
)

Write-Host "📋 Variables a configurar:" -ForegroundColor Cyan
foreach ($var in $envVars) {
    $varName = $var.Split("=")[0]
    $varValue = $var.Split("=")[1]
    
    # Ocultar valores de secretref
    if ($varValue -like "secretref:*") {
        Write-Host "   $varName = $varValue" -ForegroundColor Gray
    } else {
        Write-Host "   $varName = $varValue" -ForegroundColor White
    }
}
Write-Host ""

# Confirmación
Write-Host "⚠️  IMPORTANTE:" -ForegroundColor Yellow
Write-Host "   Este script actualizará las variables de entorno del Container App" -ForegroundColor Yellow
Write-Host "   Container App: $ContainerAppName" -ForegroundColor Yellow
Write-Host "   Resource Group: $ResourceGroup" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "¿Deseas continuar? (y/N)"
if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-Host "❌ Operación cancelada por el usuario" -ForegroundColor Red
    exit 0
}
Write-Host ""

# Actualizar Container App
Write-Host "🚀 Actualizando Container App..." -ForegroundColor Yellow
Write-Host ""

# Construir comando con todas las variables
$envVarsString = ($envVars | ForEach-Object { "`"$_`"" }) -join " "

$command = "az containerapp update " +
           "--name $ContainerAppName " +
           "--resource-group $ResourceGroup " +
           "--set-env-vars $envVarsString"

Write-Host "Ejecutando comando:" -ForegroundColor Gray
Write-Host $command -ForegroundColor DarkGray
Write-Host ""

# Ejecutar comando
Invoke-Expression $command

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Error al actualizar Container App" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Variables de entorno actualizadas exitosamente" -ForegroundColor Green
Write-Host ""

# Verificar variables configuradas
Write-Host "🔍 Verificando variables configuradas..." -ForegroundColor Yellow
Write-Host ""

$currentEnvVars = az containerapp show `
    --name $ContainerAppName `
    --resource-group $ResourceGroup `
    --query "properties.template.containers[0].env" `
    -o json | ConvertFrom-Json

Write-Host "📋 Variables de entorno actuales:" -ForegroundColor Cyan
Write-Host ""

$envVarsTable = @()
foreach ($envVar in $currentEnvVars) {
    $value = if ($envVar.secretRef) { "secretref:$($envVar.secretRef)" } else { $envVar.value }
    $envVarsTable += [PSCustomObject]@{
        Name = $envVar.name
        Value = $value
    }
}

$envVarsTable | Sort-Object Name | Format-Table -AutoSize

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ✅ Configuración Completada" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Próximos pasos:" -ForegroundColor Yellow
Write-Host "   1. ✅ Variables de entorno configuradas" -ForegroundColor Green
Write-Host "   2. Hacer commit y push para desplegar:" -ForegroundColor White
Write-Host ""
Write-Host "      git add ." -ForegroundColor Cyan
Write-Host "      git commit -m `"fix: configurar variables de entorno`"" -ForegroundColor Cyan
Write-Host "      git push origin main" -ForegroundColor Cyan
Write-Host ""
Write-Host "   3. El pipeline se ejecutará automáticamente" -ForegroundColor White
Write-Host "   4. Verificar despliegue:" -ForegroundColor White
Write-Host ""
$appUrl = az containerapp show `
    --name $ContainerAppName `
    --resource-group $ResourceGroup `
    --query "properties.configuration.ingress.fqdn" `
    -o tsv

Write-Host "      🔗 URL: https://$appUrl" -ForegroundColor Cyan
Write-Host "      🔗 Health: https://$appUrl/health" -ForegroundColor Cyan
Write-Host ""
