# ========================================
# Verify Resource Group Name Script
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ContainerAppName = "ca-platheotemplate-stg"
)

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     ?? Verificación de Resource Group                  ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Buscar Container App
Write-Host "1??  Buscando Container App: $ContainerAppName" -ForegroundColor Yellow
Write-Host ""

try {
    $rgOutput = az resource list `
        --resource-type "Microsoft.App/containerApps" `
        --name $ContainerAppName `
        --query "[0].resourceGroup" `
        -o tsv 2>&1

    if ($LASTEXITCODE -ne 0 -or -not $rgOutput) {
        Write-Host "? Container App no encontrado: $ContainerAppName" -ForegroundColor Red
        Write-Host ""
        Write-Host "?? Container Apps disponibles:" -ForegroundColor Yellow
        az containerapp list --query "[].{Name:name, ResourceGroup:resourceGroup, Location:location}" -o table
        exit 1
    }

    $resourceGroup = $rgOutput.Trim()
    
    Write-Host "? Container App encontrado" -ForegroundColor Green
    Write-Host "   Resource Group: $resourceGroup" -ForegroundColor White
    Write-Host ""

    # Paso 2: Verificar si tiene espacios o caracteres especiales
    Write-Host "2??  Analizando nombre del Resource Group..." -ForegroundColor Yellow
    Write-Host ""

    $hasSpace = $resourceGroup -match '\s'
    $hasAccent = $resourceGroup -match '[áéíóúñÁÉÍÓÚÑ]'
    $hasSpecialChars = $resourceGroup -match '[^a-zA-Z0-9\-_]'

    if ($hasSpace) {
        Write-Host "??  WARNING: El Resource Group contiene ESPACIOS" -ForegroundColor Yellow
        Write-Host "   Nombre: '$resourceGroup'" -ForegroundColor White
        Write-Host "   Problema: Bash interpreta espacios como separadores de argumentos" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   ? Solución recomendada:" -ForegroundColor Green
        Write-Host "   1. Verificar si existe otro RG sin espacios" -ForegroundColor White
        Write-Host "   2. O usar comillas en comandos bash: --resource-group `"$resourceGroup`"" -ForegroundColor White
        Write-Host ""
    }

    if ($hasAccent) {
        Write-Host "??  WARNING: El Resource Group contiene ACENTOS" -ForegroundColor Yellow
        Write-Host "   Nombre: '$resourceGroup'" -ForegroundColor White
        Write-Host "   Problema: Puede causar errores de encoding en scripts" -ForegroundColor Gray
        Write-Host ""
    }

    if ($hasSpecialChars -and -not $hasSpace -and -not $hasAccent) {
        Write-Host "??  WARNING: El Resource Group contiene caracteres especiales" -ForegroundColor Yellow
        Write-Host "   Nombre: '$resourceGroup'" -ForegroundColor White
        Write-Host ""
    }

    if (-not $hasSpace -and -not $hasAccent -and -not $hasSpecialChars) {
        Write-Host "? Nombre de Resource Group válido (sin espacios ni caracteres especiales)" -ForegroundColor Green
        Write-Host "   Nombre: '$resourceGroup'" -ForegroundColor White
        Write-Host ""
    }

    # Paso 3: Mostrar información del Container App
    Write-Host "3??  Información del Container App:" -ForegroundColor Yellow
    Write-Host ""

    $appInfo = az containerapp show `
        --name $ContainerAppName `
        --resource-group $resourceGroup `
        --query "{Name:name, ResourceGroup:resourceGroup, Location:location, State:properties.provisioningState, FQDN:properties.configuration.ingress.fqdn}" `
        -o json | ConvertFrom-Json

    Write-Host "   Nombre: $($appInfo.Name)" -ForegroundColor White
    Write-Host "   Resource Group: $($appInfo.ResourceGroup)" -ForegroundColor White
    Write-Host "   Location: $($appInfo.Location)" -ForegroundColor White
    Write-Host "   Estado: $($appInfo.State)" -ForegroundColor White
    Write-Host "   URL: https://$($appInfo.FQDN)" -ForegroundColor White
    Write-Host ""

    # Paso 4: Listar todos los RG que contengan 'platheo'
    Write-Host "4??  Resource Groups disponibles con 'platheo':" -ForegroundColor Yellow
    Write-Host ""

    $platheoRGs = az group list --query "[?contains(toLower(name), 'platheo')].{Name:name, Location:location}" -o json | ConvertFrom-Json

    if ($platheoRGs) {
        foreach ($rg in $platheoRGs) {
            $status = if ($rg.Name -eq $resourceGroup) { "? ACTUAL" } else { "" }
            Write-Host "   - $($rg.Name) ($($rg.Location)) $status" -ForegroundColor $(if ($status) { "Green" } else { "White" })
        }
    } else {
        Write-Host "   (ninguno encontrado)" -ForegroundColor Gray
    }

    Write-Host ""

    # Paso 5: Recomendaciones
    Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host "?               ?? Recomendaciones                       ?" -ForegroundColor Cyan
    Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
    Write-Host ""

    if ($hasSpace -or $hasAccent) {
        Write-Host "??  El Resource Group actual tiene problemas potenciales" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Opción 1: Actualizar pipeline con comillas" -ForegroundColor White
        Write-Host "  En azure-pipelines-stage.yml:" -ForegroundColor Gray
        Write-Host "    variables:" -ForegroundColor Gray
        Write-Host "      resourceGroup: '$resourceGroup'" -ForegroundColor Gray
        Write-Host ""
        Write-Host "  Y en comandos bash usar comillas dobles:" -ForegroundColor Gray
        Write-Host "    az containerapp show --name `$(containerAppName) --resource-group `"`$(resourceGroup)`"" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Opción 2: Usar un RG sin espacios (si existe)" -ForegroundColor White
        if ($platheoRGs.Count -gt 1) {
            $alternativeRGs = $platheoRGs | Where-Object { $_.Name -ne $resourceGroup -and $_.Name -notmatch '\s' }
            if ($alternativeRGs) {
                Write-Host "  Alternativas disponibles:" -ForegroundColor Gray
                foreach ($altRG in $alternativeRGs) {
                    Write-Host "    - $($altRG.Name)" -ForegroundColor Gray
                }
            }
        }
        Write-Host ""
    } else {
        Write-Host "? El Resource Group es compatible con la pipeline" -ForegroundColor Green
        Write-Host ""
        Write-Host "Actualizar en azure-pipelines-stage.yml:" -ForegroundColor White
        Write-Host "  variables:" -ForegroundColor Gray
        Write-Host "    resourceGroup: '$resourceGroup'" -ForegroundColor Gray
        Write-Host ""
    }

    # Paso 6: Generar valor para la pipeline
    Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "?          ? Valor para la Pipeline                     ?" -ForegroundColor Green
    Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    Write-Host "Copia este valor en .Deploy/azure-pipelines-stage.yml:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "variables:" -ForegroundColor White
    Write-Host "  resourceGroup: '$resourceGroup'" -ForegroundColor Yellow
    Write-Host ""

} catch {
    Write-Host "? Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Verifica que estés autenticado en Azure:" -ForegroundColor Yellow
    Write-Host "   az login" -ForegroundColor White
    Write-Host ""
    exit 1
}
