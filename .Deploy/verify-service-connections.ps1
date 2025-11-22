# ========================================
# Verify Service Connections for Pipeline
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Organization = "platheoinc",
    
    [Parameter(Mandatory=$false)]
    [string]$Project = "Platheo-Templates"
)

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     ?? Verificación de Service Connections             ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Configurar defaults
az devops configure --defaults organization="https://dev.azure.com/$Organization" project=$Project 2>$null

# Service Connections necesarias
$requiredConnections = @(
    @{
        Name = "acrplatheotemplatestg"
        Type = "dockerregistry"
        Description = "Docker Registry para ACR"
    },
    @{
        Name = "Suscripción Platheo"
        Type = "azurerm"
        Description = "Azure Resource Manager para deployment"
    }
)

$allConnectionsExist = $true

Write-Host "?? Verificando service connections..." -ForegroundColor Yellow
Write-Host ""

foreach ($conn in $requiredConnections) {
    Write-Host "?? Verificando: $($conn.Name)" -ForegroundColor Cyan
    
    try {
        $existing = az devops service-endpoint list `
            --query "[?name=='$($conn.Name)'].{Name:name, Type:type, IsReady:isReady}" `
            -o json 2>$null | ConvertFrom-Json
        
        if ($existing) {
            Write-Host "   ? Existe" -ForegroundColor Green
            Write-Host "      Tipo: $($existing.Type)" -ForegroundColor Gray
            Write-Host "      Estado: $(if ($existing.IsReady) { 'Ready' } else { 'Not Ready' })" -ForegroundColor Gray
            
            if (-not $existing.IsReady) {
                Write-Host "      ??  La conexión no está lista" -ForegroundColor Yellow
                $allConnectionsExist = $false
            }
        } else {
            Write-Host "   ? NO EXISTE" -ForegroundColor Red
            Write-Host "      Tipo esperado: $($conn.Type)" -ForegroundColor Gray
            Write-Host "      Descripción: $($conn.Description)" -ForegroundColor Gray
            $allConnectionsExist = $false
        }
    } catch {
        Write-Host "   ? Error verificando: $_" -ForegroundColor Red
        $allConnectionsExist = $false
    }
    
    Write-Host ""
}

# Resumen
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor $(if ($allConnectionsExist) { "Green" } else { "Yellow" })
Write-Host "?                    ?? Resumen                          ?" -ForegroundColor $(if ($allConnectionsExist) { "Green" } else { "Yellow" })
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor $(if ($allConnectionsExist) { "Green" } else { "Yellow" })
Write-Host ""

if ($allConnectionsExist) {
    Write-Host "? Todas las service connections están configuradas" -ForegroundColor Green
    Write-Host "?? Puedes ejecutar la pipeline" -ForegroundColor Green
} else {
    Write-Host "??  Faltan service connections" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "?? Service connections necesarias:" -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($conn in $requiredConnections) {
        $exists = az devops service-endpoint list `
            --query "[?name=='$($conn.Name)'].name" `
            -o tsv 2>$null
        
        $status = if ($exists) { "?" } else { "?" }
        Write-Host "   $status $($conn.Name)" -ForegroundColor White
        Write-Host "      Tipo: $($conn.Type)" -ForegroundColor Gray
        Write-Host "      Descripción: $($conn.Description)" -ForegroundColor Gray
        Write-Host ""
    }
    
    Write-Host "?? Para crear las conexiones faltantes:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Ve a Azure DevOps ? Project Settings ? Service connections" -ForegroundColor White
    Write-Host "2. Click 'New service connection'" -ForegroundColor White
    Write-Host "3. Selecciona el tipo correspondiente" -ForegroundColor White
    Write-Host "4. Configura según la documentación en .Deploy/README.md" -ForegroundColor White
    Write-Host ""
    
    # Abrir Azure DevOps
    $serviceConnectionUrl = "https://dev.azure.com/$Organization/$Project/_settings/adminservices"
    $openBrowser = Read-Host "¿Abrir Azure DevOps en el navegador? (Y/n)"
    if ($openBrowser -eq "" -or $openBrowser -eq "Y" -or $openBrowser -eq "y") {
        Start-Process $serviceConnectionUrl
        Write-Host "? Navegador abierto" -ForegroundColor Green
    }
}

Write-Host ""
