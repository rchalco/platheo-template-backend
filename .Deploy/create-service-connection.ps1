# ========================================
# Create Service Connection for ACR
# ========================================

param(
    [Parameter(Mandatory=$true)]
    [string]$AcrName = "acrplatheotemplatestg",
    
    [Parameter(Mandatory=$true)]
    [string]$AcrLoginServer = "acrplatheotemplatestg-gafvh3d5d4hbb4fc.azurecr.io",
    
    [Parameter(Mandatory=$true)]
    [string]$Organization = "platheoinc",
    
    [Parameter(Mandatory=$true)]
    [string]$Project = "Platheo-Templates",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "Suscripción Platheo"
)

Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?     ?? Create Azure DevOps Service Connection          ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar Azure CLI y extensión
Write-Host "1??  Verificando Azure DevOps CLI..." -ForegroundColor Yellow
try {
    $azDevOpsExt = az extension list --query "[?name=='azure-devops'].version" -o tsv
    if (-not $azDevOpsExt) {
        Write-Host "   ?? Instalando extensión azure-devops..." -ForegroundColor Cyan
        az extension add --name azure-devops
    } else {
        Write-Host "   ? Extensión azure-devops instalada (v$azDevOpsExt)" -ForegroundColor Green
    }
} catch {
    Write-Host "   ? Error verificando extensión: $_" -ForegroundColor Red
    exit 1
}

# Paso 2: Obtener credenciales del ACR
Write-Host ""
Write-Host "2??  Obteniendo credenciales del ACR..." -ForegroundColor Yellow
try {
    $credentials = az acr credential show --name $AcrName --resource-group $ResourceGroup -o json | ConvertFrom-Json
    
    if ($credentials) {
        $username = $credentials.username
        $password = $credentials.passwords[0].value
        
        Write-Host "   ? Credenciales obtenidas" -ForegroundColor Green
        Write-Host "      Username: $username" -ForegroundColor Gray
        Write-Host "      Password: $('*' * $password.Length)" -ForegroundColor Gray
    } else {
        Write-Host "   ? No se pudieron obtener las credenciales" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ? Error obteniendo credenciales: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "   ?? Obtén las credenciales manualmente:" -ForegroundColor Yellow
    Write-Host "   az acr credential show --name $AcrName" -ForegroundColor White
    exit 1
}

# Paso 3: Configurar defaults de Azure DevOps
Write-Host ""
Write-Host "3??  Configurando Azure DevOps CLI..." -ForegroundColor Yellow
az devops configure --defaults organization="https://dev.azure.com/$Organization" project=$Project

# Paso 4: Crear Service Connection
Write-Host ""
Write-Host "4??  Creando Service Connection..." -ForegroundColor Yellow
Write-Host "   ??  NOTA: Este comando requiere crear manualmente en el portal" -ForegroundColor Yellow
Write-Host ""
Write-Host "   ?? Información para crear la conexión manualmente:" -ForegroundColor Cyan
Write-Host "   ???????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host "   Registry Type:    Others (non-Azure)" -ForegroundColor White
Write-Host "   Docker Registry:  $AcrLoginServer" -ForegroundColor White
Write-Host "   Docker ID:        $username" -ForegroundColor White
Write-Host "   Docker Password:  " -NoNewline -ForegroundColor White
Write-Host "$password" -ForegroundColor Yellow
Write-Host "   Connection Name:  $AcrName" -ForegroundColor White
Write-Host "   Description:      ACR connection for Platheo Templates" -ForegroundColor White
Write-Host "   ???????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host ""

# Paso 5: Abrir navegador con Azure DevOps
$serviceConnectionUrl = "https://dev.azure.com/$Organization/$Project/_settings/adminservices?resourceType=endpoint"
Write-Host "5??  Abriendo Azure DevOps en el navegador..." -ForegroundColor Yellow
Write-Host "   URL: $serviceConnectionUrl" -ForegroundColor Cyan

$openBrowser = Read-Host "   ¿Abrir navegador? (Y/n)"
if ($openBrowser -eq "" -or $openBrowser -eq "Y" -or $openBrowser -eq "y") {
    Start-Process $serviceConnectionUrl
    Write-Host "   ? Navegador abierto" -ForegroundColor Green
}

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?              ?? Pasos Siguientes                       ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "1. En Azure DevOps, click 'New service connection'" -ForegroundColor White
Write-Host "2. Selecciona 'Docker Registry'" -ForegroundColor White
Write-Host "3. Selecciona 'Others' como Registry type" -ForegroundColor White
Write-Host "4. Usa las credenciales mostradas arriba" -ForegroundColor White
Write-Host "5. Marca 'Grant access permission to all pipelines'" -ForegroundColor White
Write-Host "6. Click 'Save'" -ForegroundColor White
Write-Host ""
Write-Host "? Después, ejecuta nuevamente la pipeline" -ForegroundColor Green
Write-Host ""
