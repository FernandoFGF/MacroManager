# Macro Manager para Videojuegos

Aplicación de Windows Forms para crear, grabar y reproducir macros personalizadas para videojuegos.

## 🎮 Características

- **Grabación de Macros**: Captura automática de acciones de teclado y mouse
- **Reproducción**: Ejecuta las macros grabadas con repeticiones configurables
- **Gestión Completa**: Guarda, carga, exporta e importa macros
- **Interfaz Intuitiva**: Diseño limpio y fácil de usar
- **Persistencia**: Guarda las macros en formato JSON

## 🚀 Cómo Usar

### Grabar una Macro

1. Haz clic en el botón **⏺ Grabar**
2. Realiza las acciones que quieres grabar (teclado y mouse)
3. Haz clic en **⏹ Detener** cuando termines
4. Asigna un nombre a tu macro
5. Haz clic en **💾 Guardar**

### Reproducir una Macro

1. Selecciona una macro de la lista
2. Haz clic en **▶ Reproducir**
3. Elige cuántas veces repetir (0 = infinito)
4. La macro se ejecutará automáticamente

### Gestionar Macros

- **Exportar**: Guarda una macro en un archivo .macro para compartir
- **Importar**: Carga macros desde archivos externos
- **Eliminar**: Borra macros que ya no necesitas

## 📋 Requisitos

- Windows 10 o superior
- .NET 8.0 Runtime (incluido en el instalador)

## 🛠️ Arquitectura

El proyecto está estructurado con arquitectura orientada a objetos:

```
MacroManager/
├── Models/              # Modelos de datos
│   └── MacroConfig.cs   # Configuración de macros
├── Services/            # Lógica de negocio
│   ├── MacroRecorder.cs # Grabación de acciones
│   ├── MacroPlayer.cs   # Reproducción de macros
│   └── SettingsManager.cs # Persistencia de datos
├── MainForm.cs          # Formulario principal
├── MainForm.Designer.cs # Diseño del formulario
└── Program.cs           # Punto de entrada
```

## 🔧 Compilación

### Desde la terminal:

```powershell
# Restaurar paquetes
dotnet restore

# Compilar en modo Debug
dotnet build

# Compilar en modo Release
dotnet build -c Release

# Publicar aplicación autónoma
dotnet publish -c Release -r win-x64 --self-contained true
```

### Desde Visual Studio Code:

1. Abre la carpeta del proyecto
2. Presiona `Ctrl+Shift+B` para compilar
3. O ejecuta desde el menú: Terminal > Run Build Task

## 📦 Crear Instalador

1. Compila el proyecto en modo Release
2. Localiza los archivos en `bin/Release/net8.0-windows/publish/`
3. Abre Inno Setup
4. Carga el script `installer.iss`
5. Compila para generar `MacroManager_Setup.exe`

## ⚠️ Advertencias

- Esta aplicación requiere permisos de administrador para capturar eventos globales
- Algunos juegos con anti-cheat pueden detectar y bloquear macros
- Usa responsablemente y respeta los términos de servicio de los juegos
- El uso de macros puede estar prohibido en juegos competitivos

## 📄 Licencia

Este proyecto es de código abierto. Úsalo bajo tu propia responsabilidad.

## 👨‍💻 Desarrollador

Creado como proyecto educativo para aprender C# y Windows Forms.

---

**Versión**: 1.0.0  
**Fecha**: 2024
