# ========================================
# Script de Compilación y Creación de Instalador
# Macro Manager v1.0
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MACRO MANAGER - BUILD & INSTALLER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si .NET está instalado
Write-Host "[1/6] Verificando .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "? .NET SDK encontrado: v$dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "? Error: .NET SDK no está instalado" -ForegroundColor Red
    Write-Host "Descárgalo desde: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Limpiar compilaciones anteriores
Write-Host ""
Write-Host "[2/6] Limpiando compilaciones anteriores..." -ForegroundColor Yellow
dotnet clean -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al limpiar proyecto" -ForegroundColor Red
    exit 1
}
Write-Host "? Proyecto limpio" -ForegroundColor Green

# Restaurar paquetes NuGet
Write-Host ""
Write-Host "[3/6] Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al restaurar paquetes" -ForegroundColor Red
    exit 1
}
Write-Host "? Paquetes restaurados" -ForegroundColor Green

# Compilar en modo Release
Write-Host ""
Write-Host "[4/6] Compilando proyecto en modo Release..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al compilar proyecto" -ForegroundColor Red
    exit 1
}
Write-Host "? Proyecto compilado exitosamente" -ForegroundColor Green

# Publicar aplicación autónoma
Write-Host ""
Write-Host "[5/6] Publicando aplicación autónoma..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al publicar aplicación" -ForegroundColor Red
    exit 1
}
Write-Host "? Aplicación publicada correctamente" -ForegroundColor Green

# Verificar si Inno Setup está instalado
Write-Host ""
Write-Host "[6/6] Creando instalador con Inno Setup..." -ForegroundColor Yellow

# Buscar Inno Setup en ubicaciones comunes
$innoSetupPaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 5\ISCC.exe",
    "C:\Program Files\Inno Setup 5\ISCC.exe"
)

$isccPath = $null
foreach ($path in $innoSetupPaths) {
    if (Test-Path $path) {
        $isccPath = $path
        break
    }
}

if ($isccPath) {
    Write-Host "? Inno Setup encontrado en: $isccPath" -ForegroundColor Green
    
    # Compilar el instalador
    & $isccPath "installer.iss"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Instalador creado exitosamente" -ForegroundColor Green
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "  ¡COMPILACIÓN COMPLETADA!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Archivos generados:" -ForegroundColor Yellow
        Write-Host "  • Ejecutable: MacroManager\bin\Release\net8.0-windows\win-x64\publish\MacroManager.exe" -ForegroundColor White
        Write-Host "  • Instalador: Output\MacroManager_v1.0.0_Setup.exe" -ForegroundColor White
        Write-Host ""
        
        # Preguntar si abrir la carpeta de salida
        $response = Read-Host "¿Deseas abrir la carpeta Output? (S/N)"
        if ($response -eq "S" -or $response -eq "s") {
            if (Test-Path ".\Output") {
                explorer.exe ".\Output"
            }
        }
    } else {
        Write-Host "? Error al crear instalador" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "? Inno Setup no está instalado" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Opciones:" -ForegroundColor Cyan
    Write-Host "  1. Descargar Inno Setup desde: https://jrsoftware.org/isdl.php" -ForegroundColor White
    Write-Host "  2. Instalar y ejecutar nuevamente este script" -ForegroundColor White
    Write-Host ""
    Write-Host "La aplicación ha sido compilada correctamente en:" -ForegroundColor Green
    Write-Host "  MacroManager\bin\Release\net8.0-windows\win-x64\publish\MacroManager.exe" -ForegroundColor White
    Write-Host ""

    # Preguntar si abrir la carpeta de publicación
    $response = Read-Host "¿Deseas abrir la carpeta de publicación? (S/N)"
    if ($response -eq "S" -or $response -eq "s") {
        $publishPath = ".\MacroManager\bin\Release\net8.0-windows\win-x64\publish"
        if (Test-Path $publishPath) {
            explorer.exe $publishPath
        }
    }
}

Write-Host ""
Write-Host "Presiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
