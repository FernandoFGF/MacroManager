# MacroManager for Video Games

Windows Forms application for creating, recording, and playing custom macros for video games.

## 🎮 Features

- **Macro Recording**: Automatic capture of keyboard and mouse actions
- **Playback**: Execute recorded macros with configurable repetitions
- **Complete Management**: Save, load, export and import macros
- **Intuitive Interface**: Clean and easy-to-use design
- **Persistence**: Saves macros in JSON format

## 🚀 How to Use

### Record a Macro

1. Click the **⏺ Record** button
2. Perform the actions you want to record (keyboard and mouse)
3. Click **⏹ Stop** when finished
4. Assign a name to your macro
5. Click **💾 Save**

### Play a Macro

1. Select a macro from the list
2. Click **▶ Play**
3. Choose how many times to repeat (0 = infinite)
4. The macro will execute automatically

### Manage Macros

- **Export**: Save a macro to a .macro file to share
- **Import**: Load macros from external files
- **Delete**: Remove macros you no longer need

## 📋 Requirements

- Windows 10 or higher
- .NET 8.0 Runtime (included in the installer)

## 🛠️ Architecture

The project is structured with object-oriented architecture:

```
MacroManager/
├── Models/              # Data models
│   └── MacroConfig.cs   # Macro configuration
├── Services/            # Business logic
│   ├── MacroRecorder.cs # Action recording
│   ├── MacroPlayer.cs   # Action playback
│   └── SettingsManager.cs # Persistence
└── MainForm.cs          # User interface
```

## 📦 Dependencies

- **Newtonsoft.Json 13.0.3**: JSON serialization
- **InputSimulatorCore 1.0.5**: Keyboard and mouse input simulation

## ⚠️ Important Notes

### Permissions
- Requires **Administrator privileges** to capture global keyboard and mouse events
- On Windows 11, SmartScreen may appear (this is normal, click "More info" → "Run anyway")

### Anti-Cheat Systems
- Some games with anti-cheat may **detect and ban** macro usage
- **DO NOT use in competitive games** (CS:GO, Valorant, Fortnite ranked, etc.)
- Use only in casual or single-player games

### Compatibility
- ✅ Windows 10/11
- ✅ x64 Architecture
- ✅ .NET 8.0

## 📄 License

Free for personal use. Do not use for competitive gaming.

---

**MacroManager - Create powerful automation for your gaming sessions! 🎮🚀**
