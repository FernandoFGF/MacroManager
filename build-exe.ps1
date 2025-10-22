# ========================================
# Script Simple de Compilación
# Genera el ejecutable sin instalador
# ========================================

Write-Host ""
Write-Host "?? Compilando Macro Manager..." -ForegroundColor Cyan
Write-Host ""

# Limpiar
Write-Host "? Limpiando..." -ForegroundColor Yellow
dotnet clean -c Release | Out-Null

# Publicar
Write-Host "? Compilando y publicando..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "? ¡Compilación exitosa!" -ForegroundColor Green
    Write-Host ""
    Write-Host "El ejecutable está en:" -ForegroundColor Cyan
    Write-Host "  MacroManager\bin\Release\net8.0-windows\win-x64\publish\MacroManager.exe" -ForegroundColor White
    Write-Host ""
    
    $response = Read-Host "¿Abrir carpeta? (S/N)"
    if ($response -eq "S" -or $response -eq "s") {
        explorer.exe ".\MacroManager\bin\Release\net8.0-windows\win-x64\publish"
    }
} else {
    Write-Host ""
    Write-Host "? Error en la compilación" -ForegroundColor Red
}

Write-Host ""
