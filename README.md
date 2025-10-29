# MacroManager - Gaming Automation Tool

Windows Forms application for creating, recording, and executing custom macros, keyboard shortcuts, and mouse actions for video games.

## 🎮 Features

### 📝 Macros (Available)
- **Automatic Recording**: Automatic capture of keyboard and mouse actions
- **Playback**: Execute recorded macros with configurable repetitions
- **Complete Management**: Save, load, export and import macros
- **Visual Editor**: Intuitive interface for editing individual actions
- **Persistence**: Saves macros in JSON format

### ⌨️ Shortcuts (Coming Soon)
- Custom keyboard shortcuts configuration
- Macro assignment to key combinations
- Global hotkey management

### 🖱️ Mouse (Coming Soon)
- Specific mouse actions configuration
- Custom movements and clicks management
- Advanced mouse tools

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

The project is structured with **MVC (Model-View-Controller)** architecture and **dependency injection**:

```
MacroManager/
├── Models/                    # Data models
│   └── MacroConfig.cs         # Macro configuration
├── Services/                  # Business logic
│   ├── IMacroRecorder.cs      # Recording interface
│   ├── IMacroPlayer.cs        # Playback interface
│   ├── ISettingsManager.cs    # Settings interface
│   ├── MacroRecorder.cs       # Recording service
│   ├── MacroPlayer.cs         # Playback service
│   ├── SettingsManager.cs     # Settings management
│   └── UIConfigurationService.cs # UI configuration
├── Commands/                  # Command Pattern
│   ├── ICommand.cs            # Command interface
│   ├── CommandManager.cs      # Command manager
│   ├── CreateMacroCommand.cs  # Create macro command
│   ├── AddActionCommand.cs    # Add action command
│   └── DeleteActionCommand.cs # Delete action command
├── Controller.cs              # Main controller
├── Model.cs                   # Data model
├── View.cs                    # Main view
└── Program.cs                 # Entry point
```

### Design Patterns Implemented

- **MVC (Model-View-Controller)**: Clear separation of responsibilities
- **Dependency Injection**: Services injected in constructor
- **Command Pattern**: For macro operations (create, add, delete)
- **Observer Pattern**: Events for component communication
- **Service Layer**: Encapsulated services with interfaces

## 📦 Dependencies

- **Newtonsoft.Json 13.0.3**: JSON serialization
- **WindowsInput**: Keyboard and mouse input simulation

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

## 🚧 Development Status

- ✅ **Macros**: Fully functional
- 🚧 **Shortcuts**: In development
- 🚧 **Mouse**: In development

## 📄 License

Free for personal use. Do not use for competitive gaming.

---

**MacroManager - Create powerful automation for your gaming sessions! 🎮🚀**
