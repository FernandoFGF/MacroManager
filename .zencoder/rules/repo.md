---
description: Repository Information Overview
alwaysApply: true
---

# MacroManager Information

## Summary
MacroManager is a Windows Forms application for creating, recording, and playing custom macros for video games. It captures keyboard and mouse actions, allows for configurable playback, and provides complete macro management with save, load, export, and import capabilities.

## Structure
- **MacroManager/**: Main application code with C# source files
  - **Models/**: Data models for macro configuration
  - **Services/**: Business logic for recording, playback, and settings management
- **Output/**: Contains the compiled installer
- **build-exe.ps1**: PowerShell script to build the executable
- **build-installer.ps1**: PowerShell script to build the installer
- **installer.iss**: Inno Setup script for creating the installer

## Language & Runtime
**Language**: C#
**Version**: .NET 8.0
**Build System**: MSBuild (via dotnet CLI)
**Package Manager**: NuGet

## Dependencies
**Main Dependencies**:
- Newtonsoft.Json (13.0.3): JSON serialization/deserialization
- InputSimulatorCore (1.0.5): Keyboard and mouse input simulation

## Build & Installation
```bash
# Restore packages
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Publish standalone application
dotnet publish -c Release -r win-x64 --self-contained true
```

## Main Files
**Entry Point**: MacroManager/Program.cs
**Main Form**: MacroManager/MainForm.cs
**Configuration**: MacroManager/Services/SettingsManager.cs

## Application Structure
- **Models**:
  - MacroConfig.cs: Defines the data structure for macros and actions
- **Services**:
  - MacroRecorder.cs: Handles recording of keyboard and mouse actions
  - MacroPlayer.cs: Handles playback of recorded macros
  - SettingsManager.cs: Manages persistence of macros and settings

## Installer
**Script**: installer.iss (Inno Setup)
**Output**: Output/MacroManager_v1.0.0_Setup.exe
**Requirements**: Inno Setup 5 or 6
**Build Command**:
```bash
# Using PowerShell script
.\build-installer.ps1
```

## System Requirements
- Windows 10 or higher
- .NET 8.0 Runtime (included in installer)
- Administrator privileges (for global input capture)