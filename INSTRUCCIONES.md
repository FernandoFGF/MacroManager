# 📖 COMPLETE INSTRUCTIONS - MACRO MANAGER

## 🎯 Option 1: BUILD EXECUTABLE ONLY (Quick and Easy)

If you just want to test the application without creating an installer:

### Step 1: Run the build script

```powershell
.\build-exe.ps1
```

### Step 2: Locate the executable

The file will be at:
```
MacroManager\bin\Release\net8.0-windows\win-x64\publish\MacroManager.exe
```

### Step 3: Run it

Simply double-click `MacroManager.exe`

**⚠️ IMPORTANT**: You must distribute the ENTIRE `publish` folder with all DLLs, not just the .exe

---

## 🎯 Option 2: CREATE PROFESSIONAL INSTALLER (Recommended for Distribution)

If you want to create a professional .exe installer to share:

### Prerequisites

1. **Download and install Inno Setup**
   - Visit: https://jrsoftware.org/isdl.php
   - Download the latest version (usually "Inno Setup 6")
   - Install with default options

### Method A: Use the Automatic Script (RECOMMENDED)

```powershell
.\build-installer.ps1
```

This script:
1. ✓ Cleans previous builds
2. ✓ Restores NuGet packages
3. ✓ Compiles the project
4. ✓ Publishes the application
5. ✓ Creates the installer automatically

**Result**: `Output\MacroManager_v1.0.0_Setup.exe`

---

## 🎯 Option 3: MANUAL INSTALLATION STEPS

If you prefer more control, here are the steps:

### Step 1: Restore NuGet Packages

```powershell
dotnet restore
```

### Step 2: Build in Release Mode

```powershell
dotnet build -c Release
```

### Step 3: Publish as Self-Contained

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

### Step 4: Create Installer with Inno Setup

- Open `installer.iss` in Inno Setup
- Click "Compile"
- The installer will be created in the `Output` folder

---

## 🚀 QUICK START

### For Immediate Testing

**Option A: Use the interactive menu**
```batch
COMANDOS-RAPIDOS.bat
```

**Option B: Build executable**
```powershell
.\build-exe.ps1
```

**Option C: Build + Installer**
```powershell
.\build-installer.ps1
```

---

## 📦 FILE DISTRIBUTION

### Using Portable Executable:

1. Go to: `MacroManager\bin\Release\net8.0-windows\win-x64\publish\`
2. Compress the ENTIRE folder to ZIP
3. Share the ZIP with other users
4. Users must extract the ENTIRE ZIP before running

### Using Installer:

1. Run `.\build-installer.ps1`
2. Locate: `Output\MacroManager_v1.0.0_Setup.exe`
3. Share only that .exe file
4. Users simply double-click and install

---

## 🛠️ TROUBLESHOOTING

### "dotnet not recognized"

```powershell
# Install .NET SDK:
# https://dotnet.microsoft.com/download
```

### "Error opening app.ico"

```powershell
# It's already included, but if it fails:
# Comment the line in MacroManager.csproj
```

### "Inno Setup not found"

```powershell
# Download and install:
# https://jrsoftware.org/isdl.php
```

### "Executable won't run"

```powershell
# 1. Run as administrator
# 2. Verify you distributed the ENTIRE publish folder
# 3. Temporarily disable antivirus
```

---

## 📋 PROJECT STRUCTURE

```
MacroManager-master/
├── MacroManager/                    # Main application code
│   ├── Program.cs                   # Entry point
│   ├── MainForm.cs                  # Main UI form
│   ├── MacroManager.csproj          # Project configuration
│   ├── Models/
│   │   └── MacroConfig.cs           # Macro data model
│   ├── Services/
│   │   ├── MacroRecorder.cs         # Recording service
│   │   ├── MacroPlayer.cs           # Playback service
│   │   └── SettingsManager.cs       # Persistence service
│   ├── bin/                         # Compiled output
│   │   └── Release/net8.0-windows/win-x64/publish/
│   │       └── MacroManager.exe     # Final executable
│   └── obj/                         # Build artifacts
│
├── Output/                          # Installer output
│   └── MacroManager_v1.0.0_Setup.exe # Installer file
│
├── build-exe.ps1                    # Build executable script
├── build-installer.ps1              # Build installer script
├── installer.iss                    # Inno Setup configuration
├── README.md                        # User documentation
├── INSTRUCCIONES.md                 # Instructions (this file)
└── RESUMEN-PROYECTO.md              # Project summary
```

---

## ✨ SUMMARY

| Method | When to Use | Advantages | Disadvantages |
|--------|-------------|-----------|---------------|
| **Direct Execution** | Development/testing | Fast, no installation | Only for you |
| **Portable Executable** | Share with friends | Easy to distribute | Must extract ZIP |
| **Professional Installer** | Public distribution | Professional, easy install | Requires Inno Setup |

---

## ⚠️ IMPORTANT NOTES

### Permissions
- Requires **Administrator privileges** for global input capture
- Windows 11 may show SmartScreen (normal, click "More info" → "Run anyway")

### Anti-Cheat
- Some games with anti-cheat can **detect and ban** macro usage
- **DO NOT use in competitive games** (CS:GO, Valorant, Fortnite ranked, etc.)
- Use only in casual or single-player games

### Compatibility
- ✅ Windows 10/11
- ✅ x64 Architecture
- ✅ .NET 8.0

---

**Your MacroManager is ready to use! 🎮🚀**

*Created with ❤️ for the gaming community*
