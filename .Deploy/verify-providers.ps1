# ========================================
# Verify Azure Providers Script
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId = "7430bafe-bc29-443c-93d1-a8cb3090136c"
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     Verificacion de Azure Providers" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Providers necesarios
$requiredProviders = @(
    @{ Name = "Microsoft.App"; Description = "Azure Container Apps"; Critical = $true },
    @{ Name = "Microsoft.ContainerRegistry"; Description = "Azure Container Registry"; Critical = $true },
    @{ Name = "Microsoft.OperationalInsights"; Description = "Log Analytics"; Critical = $false },
    @{ Name = "Microsoft.Insights"; Description = "Application Insights"; Critical = $false }
)

try {
    Write-Host "1. Verificando Azure CLI..." -ForegroundColor Yellow
    az --version | Out-Null
    Write-Host "   OK Azure CLI disponible" -ForegroundColor Green
    Write-Host ""

    Write-Host "2. Configurando suscripcion..." -ForegroundColor Yellow
    az account set --subscription $SubscriptionId 2>&1 | Out-Null
    Write-Host "   OK Suscripcion configurada" -ForegroundColor Green
    Write-Host ""

    Write-Host "3. Verificando providers..." -ForegroundColor Yellow
    Write-Host ""

    $allRegistered = $true
    $providersToRegister = @()

    foreach ($provider in $requiredProviders) {
        $state = az provider show --namespace $provider.Name --query "registrationState" -o tsv 2>$null
        
        Write-Host "   $($provider.Name)" -ForegroundColor Cyan
        Write-Host "      Descripcion: $($provider.Description)" -ForegroundColor Gray
        
        if ($state -eq "Registered") {
            Write-Host "      Estado: OK Registered" -ForegroundColor Green
        } else {
            Write-Host "      Estado: ERROR Not Registered" -ForegroundColor Red
            $allRegistered = $false
            $providersToRegister += $provider
        }
        Write-Host ""
    }

    if ($allRegistered) {
        Write-Host "OK Todos los providers estan registrados" -ForegroundColor Green
        exit 0
    }

    Write-Host "ATENCION: Algunos providers no estan registrados" -ForegroundColor Yellow
    Write-Host ""
    
    $register = Read-Host "Deseas registrar los providers faltantes? (Y/n)"
    
    if ($register -eq "" -or $register -eq "Y" -or $register -eq "y") {
        Write-Host ""
        foreach ($provider in $providersToRegister) {
            Write-Host "Registrando $($provider.Name)..." -ForegroundColor Yellow
            az provider register --namespace $provider.Name --wait 2>&1 | Out-Null
            Write-Host "OK $($provider.Name) registrado" -ForegroundColor Green
        }
        Write-Host ""
        Write-Host "OK Providers registrados exitosamente" -ForegroundColor Green
    }

} catch {
    Write-Host ""
    Write-Host "ERROR: $_" -ForegroundColor Red
    exit 1
}
