# ========================================
# Platheo API - Docker Helper Script (PowerShell)
# ========================================

param(
    [Parameter(Position=0)]
    [string]$Command,
    
    [Parameter(Position=1)]
    [string]$Environment = "dev"
)

$ImageName = "platheo-api"
$ContainerName = "platheo-api"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

function Show-Usage {
    Write-Host "Uso:" -ForegroundColor Green
    Write-Host "  .\docker-helper.ps1 [comando] [ambiente]"
    Write-Host ""
    Write-Host "Comandos:" -ForegroundColor Green
    Write-Host "  build       - Construir imagen Docker"
    Write-Host "  run         - Ejecutar contenedor"
    Write-Host "  stop        - Detener contenedor"
    Write-Host "  logs        - Ver logs del contenedor"
    Write-Host "  restart     - Reiniciar contenedor"
    Write-Host "  clean       - Limpiar contenedores e imágenes"
    Write-Host "  shell       - Abrir shell en el contenedor"
    Write-Host ""
    Write-Host "Ambientes:" -ForegroundColor Green
    Write-Host "  dev         - Development (default)"
    Write-Host "  stage       - Stage"
    Write-Host "  prod        - Production"
    Write-Host ""
    Write-Host "Ejemplos:" -ForegroundColor Green
    Write-Host "  .\docker-helper.ps1 build dev"
    Write-Host "  .\docker-helper.ps1 run stage"
    Write-Host "  .\docker-helper.ps1 logs prod"
    Write-Host ""
    Write-Host "Nota: Este script debe ejecutarse desde VectorStinger.Contaner/" -ForegroundColor Yellow
}

function Get-EnvironmentConfig {
    param([string]$Env)
    
    switch ($Env.ToLower()) {
        "dev" { 
            return @{
                Name = "Development"
                Port = 8034
            }
        }
        "development" {
            return @{
                Name = "Development"
                Port = 8034
            }
        }
        "stage" {
            return @{
                Name = "Stage"
                Port = 8035
            }
        }
        "prod" {
            return @{
                Name = "Production"
                Port = 8080
            }
        }
        "production" {
            return @{
                Name = "Production"
                Port = 8080
            }
        }
        default {
            return @{
                Name = "Development"
                Port = 8034
            }
        }
    }
}

function Build-Image {
    param([string]$Env)
    
    $config = Get-EnvironmentConfig $Env
    $envName = $config.Name
    $tag = $envName.ToLower()
    
    Write-Host "?? Construyendo imagen para ambiente: $envName" -ForegroundColor Green
    Write-Host "?? Context: $RootDir" -ForegroundColor Cyan
    Write-Host "?? Dockerfile: $ScriptDir\Dockerfile" -ForegroundColor Cyan
    
    Push-Location $RootDir
    docker build `
        -f "$ScriptDir\Dockerfile" `
        -t "${ImageName}:${tag}" `
        --build-arg ASPNETCORE_ENVIRONMENT=$envName `
        .
    Pop-Location
    
    Write-Host "? Imagen construida: ${ImageName}:${tag}" -ForegroundColor Green
}

function Run-Container {
    param([string]$Env)
    
    $config = Get-EnvironmentConfig $Env
    $envName = $config.Name
    $port = $config.Port
    $tag = $envName.ToLower()
    $containerName = "${ContainerName}-${tag}"
    
    Write-Host "?? Ejecutando contenedor para ambiente: $envName" -ForegroundColor Green
    
    # Detener contenedor existente si existe
    docker stop $containerName 2>$null
    docker rm $containerName 2>$null
    
    # Leer variables de entorno desde .env si existe
    $envFile = Join-Path $ScriptDir ".env"
    $dbPassword = $env:DB_PASSWORD
    $paymentKey = $env:PAYMENT_SECRET_KEY
    $appInsights = $env:APPINSIGHTS_CONNECTION_STRING
    
    if (Test-Path $envFile) {
        Write-Host "?? Cargando variables desde .env" -ForegroundColor Cyan
        Get-Content $envFile | ForEach-Object {
            if ($_ -match '^([^=#]+)=(.*)$') {
                $key = $matches[1].Trim()
                $value = $matches[2].Trim()
                Set-Item -Path "env:$key" -Value $value
            }
        }
        $dbPassword = $env:DB_PASSWORD
        $paymentKey = $env:PAYMENT_SECRET_KEY
        $appInsights = $env:APPINSIGHTS_CONNECTION_STRING
    }
    
    $imagesPath = Join-Path $RootDir "images"
    if (-not (Test-Path $imagesPath)) {
        Write-Host "?? Creando carpeta de imágenes: $imagesPath" -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $imagesPath -Force | Out-Null
    }
    
    docker run -d `
        --name $containerName `
        -p "${port}:8080" `
        -e ASPNETCORE_ENVIRONMENT=$envName `
        -e DB_PASSWORD="$dbPassword" `
        -e PAYMENT_SECRET_KEY="$paymentKey" `
        -e APPINSIGHTS_CONNECTION_STRING="$appInsights" `
        -v "${imagesPath}:/app/images" `
        "${ImageName}:${tag}"
    
    Write-Host "? Contenedor ejecutándose en puerto: $port" -ForegroundColor Green
    Write-Host "?? Ver logs: docker logs -f $containerName" -ForegroundColor Yellow
    Write-Host "?? URL: http://localhost:$port" -ForegroundColor Yellow
    Write-Host "??  Health: http://localhost:$port/health" -ForegroundColor Yellow
}

function Stop-Container {
    param([string]$Env)
    
    $config = Get-EnvironmentConfig $Env
    $tag = $config.Name.ToLower()
    $containerName = "${ContainerName}-${tag}"
    
    Write-Host "?? Deteniendo contenedor: $containerName" -ForegroundColor Yellow
    docker stop $containerName
    Write-Host "? Contenedor detenido" -ForegroundColor Green
}

function Show-Logs {
    param([string]$Env)
    
    $config = Get-EnvironmentConfig $Env
    $tag = $config.Name.ToLower()
    $containerName = "${ContainerName}-${tag}"
    
    Write-Host "?? Mostrando logs de: $containerName" -ForegroundColor Green
    docker logs -f $containerName
}

function Restart-Container {
    param([string]$Env)
    
    $config = Get-EnvironmentConfig $Env
    $tag = $config.Name.ToLower()
    $containerName = "${ContainerName}-${tag}"
    
    Write-Host "?? Reiniciando contenedor: $containerName" -ForegroundColor Yellow
    docker restart $containerName
    Write-Host "? Contenedor reiniciado" -ForegroundColor Green
}

function Clean-All {
    Write-Host "?? Limpiando contenedores e imágenes..." -ForegroundColor Red
    $confirmation = Read-Host "¿Estás seguro? (y/n)"
    
    if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
        docker ps -a --filter "name=$ContainerName" -q | ForEach-Object { docker stop $_ 2>$null; docker rm $_ 2>$null }
        docker images -q $ImageName | ForEach-Object { docker rmi $_ 2>$null }
        Write-Host "? Limpieza completada" -ForegroundColor Green
    }
}

function Open-Shell {
    param([string]$Env)
    
    $config = Get-EnvironmentConfig $Env
    $tag = $config.Name.ToLower()
    $containerName = "${ContainerName}-${tag}"
    
    Write-Host "?? Abriendo shell en: $containerName" -ForegroundColor Green
    docker exec -it $containerName /bin/bash
}

# Main
if ([string]::IsNullOrEmpty($Command)) {
    Show-Usage
    exit 0
}

switch ($Command.ToLower()) {
    "build" {
        Build-Image $Environment
    }
    "run" {
        Run-Container $Environment
    }
    "stop" {
        Stop-Container $Environment
    }
    "logs" {
        Show-Logs $Environment
    }
    "restart" {
        Restart-Container $Environment
    }
    "clean" {
        Clean-All
    }
    "shell" {
        Open-Shell $Environment
    }
    default {
        Show-Usage
    }
}
