# ========================================
# Script: Listar Casos de Uso Registrados
# ========================================
# 
# Este script lista todos los casos de uso que serán
# registrados automáticamente por VectorStinger.Zmq.Service
#
# USO:
#   .\list-usecases.ps1
#
# ========================================

param(
    [string]$ProjectPath = "VectorStinger.Application",
    [switch]$Detailed = $false,
    [switch]$Json = $false
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Casos de Uso en ZeroMQ Service" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Buscar todos los archivos *UseCase.cs
$useCaseFiles = Get-ChildItem -Path $ProjectPath -Recurse -Filter "*UseCase.cs" | 
    Where-Object { 
        $_.Name -ne "BaseUseCase.cs" -and 
        $_.Name -ne "IUseCase.cs" -and
        $_.DirectoryName -like "*\UserCase\*"
    } |
    Sort-Object FullName

if ($useCaseFiles.Count -eq 0) {
    Write-Host "? No se encontraron casos de uso en: $ProjectPath" -ForegroundColor Red
    exit 1
}

# Agrupar por módulo
$useCasesByModule = $useCaseFiles | Group-Object { 
    if ($_.FullName -match "UserCase\\([^\\]+)\\") {
        $matches[1]
    } else {
        "Unknown"
    }
}

$allUseCases = @()

foreach ($module in $useCasesByModule | Sort-Object Name) {
    Write-Host "?? Módulo: $($module.Name)" -ForegroundColor Yellow
    Write-Host "   Casos de uso: $($module.Count)" -ForegroundColor Gray
    Write-Host ""
    
    $count = 1
    foreach ($file in $module.Group | Sort-Object Name) {
        $useCaseName = $file.BaseName
        $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
        
        # Leer el archivo para obtener el namespace
        $content = Get-Content $file.FullName -Raw
        $namespace = ""
        if ($content -match "namespace\s+([^\s;{]+)") {
            $namespace = $matches[1]
        }
        
        # Buscar archivos relacionados
        $directory = $file.DirectoryName
        $baseName = $useCaseName.Replace("UseCase", "")
        
        $inputFile = Get-ChildItem -Path $directory -Filter "*Input.cs" -ErrorAction SilentlyContinue | Select-Object -First 1
        $outputFile = Get-ChildItem -Path $directory -Filter "*Output.cs" -ErrorAction SilentlyContinue | Select-Object -First 1
        $validationFile = Get-ChildItem -Path $directory -Filter "*Validation.cs" -ErrorAction SilentlyContinue | Select-Object -First 1
        
        Write-Host "   $count. " -NoNewline -ForegroundColor White
        Write-Host "$useCaseName" -ForegroundColor Green
        
        if ($Detailed) {
            Write-Host "      ?? Path: $relativePath" -ForegroundColor DarkGray
            if ($namespace) {
                Write-Host "      ?? Namespace: $namespace" -ForegroundColor DarkGray
            }
            if ($inputFile) {
                Write-Host "      ??  Input: $($inputFile.Name)" -ForegroundColor DarkGray
            }
            if ($outputFile) {
                Write-Host "      ??  Output: $($outputFile.Name)" -ForegroundColor DarkGray
            }
            if ($validationFile) {
                Write-Host "      ??  Validation: $($validationFile.Name)" -ForegroundColor DarkGray
            }
        }
        
        $useCaseInfo = [PSCustomObject]@{
            Name = $useCaseName
            Module = $module.Name
            Namespace = $namespace
            Path = $relativePath
            InputFile = if ($inputFile) { $inputFile.Name } else { $null }
            OutputFile = if ($outputFile) { $outputFile.Name } else { $null }
            ValidationFile = if ($validationFile) { $validationFile.Name } else { $null }
        }
        
        $allUseCases += $useCaseInfo
        
        Write-Host ""
        $count++
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? Total: $($useCaseFiles.Count) casos de uso registrados" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Resumen por módulo
Write-Host "?? Resumen por Módulo:" -ForegroundColor Yellow
Write-Host ""
foreach ($module in $useCasesByModule | Sort-Object Name) {
    Write-Host "   • $($module.Name): " -NoNewline -ForegroundColor White
    Write-Host "$($module.Count) casos de uso" -ForegroundColor Cyan
}
Write-Host ""

# Exportar a JSON si se solicita
if ($Json) {
    $jsonPath = "VectorStinger.Zmq.Service\usecases.json"
    $allUseCases | ConvertTo-Json -Depth 3 | Out-File $jsonPath -Encoding UTF8
    Write-Host "?? Exportado a JSON: $jsonPath" -ForegroundColor Green
    Write-Host ""
}

# Información del servicio ZeroMQ
Write-Host "?? Configuración del Servicio ZeroMQ:" -ForegroundColor Yellow
Write-Host ""

$appsettingsPath = "VectorStinger.Zmq.Service\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    
    if ($appsettings.ZmqSettings) {
        Write-Host "   • Bind Address: " -NoNewline -ForegroundColor White
        Write-Host "$($appsettings.ZmqSettings.BindAddress)" -ForegroundColor Cyan
        
        Write-Host "   • Worker Threads: " -NoNewline -ForegroundColor White
        Write-Host "$($appsettings.ZmqSettings.WorkerThreads)" -ForegroundColor Cyan
    } else {
        Write-Host "   • Bind Address: " -NoNewline -ForegroundColor White
        Write-Host "tcp://*:5555 (default)" -ForegroundColor Cyan
        
        Write-Host "   • Worker Threads: " -NoNewline -ForegroundColor White
        Write-Host "4 (default)" -ForegroundColor Cyan
    }
} else {
    Write-Host "   ??  No se encontró appsettings.json" -ForegroundColor Yellow
    Write-Host "   • Usando configuración por defecto" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ejemplo de uso
Write-Host "?? Ejemplo de Uso desde Cliente:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   using NetMQ;" -ForegroundColor Gray
Write-Host "   using NetMQ.Sockets;" -ForegroundColor Gray
Write-Host "   using MessagePack;" -ForegroundColor Gray
Write-Host "" -ForegroundColor Gray
Write-Host "   using var client = new DealerSocket();" -ForegroundColor Gray
Write-Host "   client.Connect(`"tcp://localhost:5555`");" -ForegroundColor Gray
Write-Host "" -ForegroundColor Gray
Write-Host "   var input = new ValidateTokenInput { ... };" -ForegroundColor Gray
Write-Host "   var data = MessagePackSerializer.Serialize(input);" -ForegroundColor Gray
Write-Host "" -ForegroundColor Gray
Write-Host "   client.SendMoreFrame(string.Empty);" -ForegroundColor Gray
Write-Host "   client.SendMoreFrame(`"ValidateTokenUseCase`");" -ForegroundColor Gray
Write-Host "   client.SendFrame(data);" -ForegroundColor Gray
Write-Host "" -ForegroundColor Gray
Write-Host "   var response = client.ReceiveFrameBytes();" -ForegroundColor Gray
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? Script completado exitosamente" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
