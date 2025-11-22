# ========================================
# Add .Deploy Folder to Solution
# ========================================

$slnPath = "Plahteo-WebTemplate.sln"
$deployFolder = ".Deploy"

Write-Host "Agregando carpeta .Deploy a la solucion..." -ForegroundColor Cyan

# Leer contenido de la solución
$slnContent = Get-Content $slnPath -Raw

# Generar GUID único para la Solution Folder
$folderGuid = [guid]::NewGuid().ToString().ToUpper()

# Buscar todos los archivos en .Deploy
$deployFiles = Get-ChildItem -Path $deployFolder -File -Recurse | ForEach-Object {
    $_.FullName.Replace("$PWD\", "").Replace("\", "\")
}

Write-Host "Archivos encontrados en .Deploy:" -ForegroundColor Yellow
$deployFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }

# Crear entrada de Solution Folder
$folderEntry = @"

Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = ".Deploy", ".Deploy", "{$folderGuid}"
	ProjectSection(SolutionItems) = preProject
"@

foreach ($file in $deployFiles) {
    $folderEntry += "`n`t`t$file = $file"
}

$folderEntry += @"

	EndProjectSection
EndProject
"@

# Buscar la posición después del último Project
$lastProjectIndex = $slnContent.LastIndexOf("EndProject")
if ($lastProjectIndex -eq -1) {
    Write-Host "ERROR: No se encontro ningun proyecto en la solucion" -ForegroundColor Red
    exit 1
}

# Insertar la Solution Folder después del último proyecto
$insertPosition = $lastProjectIndex + "EndProject".Length
$newContent = $slnContent.Insert($insertPosition, $folderEntry)

# Guardar el archivo
$newContent | Set-Content $slnPath -Encoding UTF8

Write-Host ""
Write-Host "OK Carpeta .Deploy agregada a la solucion" -ForegroundColor Green
Write-Host "Archivos incluidos: $($deployFiles.Count)" -ForegroundColor Green

# Reabrir la solución en Visual Studio (opcional)
$reopen = Read-Host "Deseas reabrir la solucion en Visual Studio? (Y/n)"
if ($reopen -eq "" -or $reopen -eq "Y" -or $reopen -eq "y") {
    Start-Process $slnPath
}
