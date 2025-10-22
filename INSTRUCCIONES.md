# 📖 INSTRUCCIONES COMPLETAS - MACRO MANAGER

## 🎯 Opción 1: COMPILAR SOLO EL EJECUTABLE (Rápido y Fácil)

Si solo quieres probar la aplicación sin crear un instalador:

### Paso 1: Ejecutar el script de compilación

```powershell
.\build-exe.ps1
```

### Paso 2: Localizar el ejecutable

El archivo estará en:
```
MacroManager\bin\Release\net8.0-windows\win-x64\publish\MacroManager.exe
```

### Paso 3: Ejecutar

Simplemente haz doble clic en `MacroManager.exe`

**⚠️ IMPORTANTE**: Debes distribuir TODA la carpeta `publish` con todas las DLLs, no solo el .exe

---

## 🎯 Opción 2: CREAR INSTALADOR PROFESIONAL (Recomendado para Distribución)

Si quieres crear un instalador .exe profesional para compartir:

### Requisitos Previos

1. **Descargar e instalar Inno Setup**
   - Visita: https://jrsoftware.org/isdl.php
   - Descarga la versión más reciente (normalmente "Inno Setup 6")
   - Instala con las opciones por defecto

### Método A: Usar el Script Automático (RECOMENDADO)

```powershell
.\build-installer.ps1
```

Este script:
1. ✓ Limpia compilaciones anteriores
2. ✓ Restaura paquetes NuGet
3. ✓ Compila el proyecto
4. ✓ Publica la aplicación
5. ✓ Crea el instalador automáticamente
6. ✓ Abre la carpeta Output con el instalador final

### Método B: Paso a Paso Manual

#### 1. Compilar la aplicación

```powershell
# Limpiar
dotnet clean -c Release

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build -c Release

# Publicar
dotnet publish -c Release -r win-x64 --self-contained true
```

#### 2. Abrir Inno Setup

- Abre el programa "Inno Setup Compiler"
- Ve a: File → Open
- Selecciona el archivo `installer.iss` de tu proyecto

#### 3. Compilar el Instalador

- Presiona F9 o ve a: Build → Compile
- Espera a que termine (unos segundos)

#### 4. Localizar el Instalador

El instalador final estará en:
```
Output\MacroManager_v1.0.0_Setup.exe
```

---

## 📦 Distribución de Archivos

### Si compilaste solo el ejecutable:
- Comprime TODA la carpeta `publish` en un ZIP
- Incluye un README explicando que deben extraer todo el ZIP

### Si creaste el instalador:
- Solo necesitas distribuir el archivo `MacroManager_v1.0.0_Setup.exe`
- El instalador se encargará de copiar todos los archivos necesarios

---

## 🚀 USO DE LA APLICACIÓN

### Grabar una Macro

1. Abre Macro Manager
2. Clic en **⏺ Grabar**
3. Realiza las acciones (teclado/mouse) que quieres grabar
4. Clic en **⏹ Detener**
5. Escribe un nombre para tu macro
6. Clic en **💾 Guardar**

### Reproducir una Macro

1. Selecciona una macro de la lista
2. Clic en **▶ Reproducir**
3. Elige cuántas veces repetir:
   - `1` = Una vez
   - `0` = Infinito (hasta presionar Parar)
   - Cualquier número = Esa cantidad de veces

### Gestionar Macros

- **📤 Exportar**: Guarda una macro en un archivo `.macro` para compartir
- **📥 Importar**: Carga macros desde archivos externos
- **🗑 Eliminar**: Borra macros que ya no necesites

---

## ⚙️ PERSONALIZACIÓN

### Cambiar el Icono

1. Reemplaza el archivo `MacroManager\app.ico` con tu icono
2. Vuelve a compilar

### Cambiar Información de la Aplicación

Edita `MacroManager\MacroManager.csproj`:

```xml
<Version>1.0.0.0</Version>
<Authors>Tu Nombre</Authors>
<Company>Tu Compañía</Company>
<Product>Tu Producto</Product>
<Description>Tu Descripción</Description>
```

### Cambiar Información del Instalador

Edita `installer.iss` en las primeras líneas:

```pascal
#define MyAppName "Tu Nombre de App"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Tu Nombre"
#define MyAppURL "https://tu-sitio.com"
```

---

## 🔧 SOLUCIÓN DE PROBLEMAS

### Error: "dotnet no se reconoce como comando"

**Solución**: Instala .NET SDK desde https://dotnet.microsoft.com/download

### Error: "No se puede encontrar app.ico"

**Solución**: El proyecto ya incluye un icono. Si falta, comenta la línea en `.csproj`:
```xml
<!-- <ApplicationIcon>app.ico</ApplicationIcon> -->
```

### Error: "Inno Setup no está instalado"

**Solución**: 
1. Descarga desde https://jrsoftware.org/isdl.php
2. Instala con opciones por defecto
3. Ejecuta nuevamente el script

### El ejecutable no inicia

**Posibles causas**:
- Falta el runtime .NET → Usa `--self-contained true` al publicar
- Antivirus bloqueando → Agrega excepción
- Faltan DLLs → Distribuye toda la carpeta publish

### "El juego no detecta las macros"

**Posibles causas**:
- Ejecuta como Administrador (clic derecho → Ejecutar como administrador)
- Algunos juegos con anti-cheat bloquean macros
- Verifica que el juego esté en primer plano al reproducir

---

## 📊 ESTRUCTURA DEL PROYECTO

```
PROJECT/
├── MacroManager/              # Código fuente
│   ├── Models/               # Modelos de datos
│   ├── Services/             # Lógica de negocio
│   ├── Program.cs            # Punto de entrada
│   ├── MainForm.cs           # Formulario principal
│   └── MainForm.Designer.cs  # Diseño del formulario
├── installer.iss             # Script de Inno Setup
├── build-installer.ps1       # Script completo con instalador
├── build-exe.ps1             # Script simple solo ejecutable
└── INSTRUCCIONES.md          # Este archivo
```

---

## 📝 COMANDOS ÚTILES

```powershell
# Ver versión de .NET
dotnet --version

# Limpiar proyecto
dotnet clean

# Restaurar paquetes
dotnet restore

# Compilar en Debug
dotnet build

# Compilar en Release
dotnet build -c Release

# Publicar para Windows 64-bit
dotnet publish -c Release -r win-x64 --self-contained true

# Ejecutar sin compilar
dotnet run

# Ejecutar con hot reload
dotnet watch run
```

---

## ⚠️ ADVERTENCIAS LEGALES

- Esta aplicación requiere permisos de administrador para funcionar correctamente
- Algunos juegos con anti-cheat pueden detectar y banear el uso de macros
- Usa responsablemente y respeta los términos de servicio de cada juego
- No usar en juegos competitivos o ranked
- El desarrollador no se responsabiliza por sanciones en juegos

---

## 🆘 SOPORTE

Si encuentras problemas:

1. Verifica que seguiste todos los pasos correctamente
2. Revisa la sección de Solución de Problemas
3. Comprueba que tienes todas las herramientas instaladas
4. Ejecuta como administrador si hay problemas de permisos

---

## 📄 LICENCIA

Este proyecto es de código abierto y educativo. Úsalo libremente pero bajo tu propia responsabilidad.

---

**Versión del documento**: 1.0  
**Última actualización**: 2024  
**Compatibilidad**: Windows 10/11, .NET 8.0
