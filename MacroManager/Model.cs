using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;
using MacroManager.Services;
using System.Runtime.InteropServices;

namespace MacroManager
{
    /// <summary>
    /// Data model for the MacroManager application
    /// Contains all data logic, configuration and application state
    /// </summary>
    public class Model
    {
        // Services - now injected
        private readonly IMacroRecorder _recorder;
        private readonly IMacroPlayer _player;
        private readonly ISettingsManager _settingsManager;
        private readonly UIConfigurationService _uiConfig;

        // Data
        private List<MacroConfig> _loadedMacros;
        private MacroConfig _currentMacro;
        private List<MacroConfig> _loadedShortcuts;
        private MacroConfig _currentShortcut;
        private IntPtr _targetWindowHandle = IntPtr.Zero;
        private uint _targetProcessId = 0;
        private string _targetWindowTitle = "Desktop (global)";

        // Events
        public event EventHandler<MacroAction> ActionRecorded;
        public event EventHandler PlaybackStarted;
        public event EventHandler PlaybackStopped;
        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler ShortcutRecordingStarted;
        public event EventHandler ShortcutRecordingStopped;
        public event EventHandler MacrosChanged;
        public event EventHandler CurrentMacroChanged;
        public event EventHandler ShortcutsChanged;
        public event EventHandler CurrentShortcutChanged;
        public event EventHandler PlaybackPaused;
        public event EventHandler PlaybackResumed;
        public event EventHandler<bool> ConditionalArmedChanged;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public Model(IMacroRecorder recorder, IMacroPlayer player, ISettingsManager settingsManager, UIConfigurationService uiConfig)
        {
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _uiConfig = uiConfig ?? throw new ArgumentNullException(nameof(uiConfig));

            InitializeServices();
            LoadMacros();
            LoadShortcuts();
        }

        #region Properties

        public List<MacroConfig> LoadedMacros => _loadedMacros;
        public MacroConfig CurrentMacro => _currentMacro;
        public List<MacroConfig> LoadedShortcuts => _loadedShortcuts;
        public MacroConfig CurrentShortcut => _currentShortcut;
        public bool IsRecording => _recorder?.IsRecording ?? false;
        public bool IsPlaying => _player?.IsPlaying ?? false;
        public bool IsPaused => _player?.IsPaused ?? false;
        public IntPtr TargetWindowHandle { get => _targetWindowHandle; set => _targetWindowHandle = value; }
        public uint TargetProcessId { get => _targetProcessId; set => _targetProcessId = value; }
        public string TargetWindowTitle { get => _targetWindowTitle; set => _targetWindowTitle = value; }

        // UI Configuration Properties - delegated to service
        public int MinWindowWidth => _uiConfig.MinWindowWidth;
        public int MinWindowHeight => _uiConfig.MinWindowHeight;
        public int DefaultWindowWidth => _uiConfig.DefaultWindowWidth;
        public int DefaultWindowHeight => _uiConfig.DefaultWindowHeight;
        public double TreeViewPercentage => _uiConfig.TreeViewPercentage;
        public double EditorPercentage => _uiConfig.EditorPercentage;
        public int PlaybackPanelHeight => _uiConfig.PlaybackPanelHeight;
        public int MinimumTreeViewWidth => _uiConfig.MinimumTreeViewWidth;
        public int MinimumEditorWidth => _uiConfig.MinimumEditorWidth;

        // Theme Properties - delegated to service
        public Color PanelBackColor => _uiConfig.PanelBackColor;
        public Color PanelForeColor => _uiConfig.PanelForeColor;
        public Color AccentColor => _uiConfig.AccentColor;
        public Color CardBackColor => _uiConfig.CardBackColor;
        public Color BorderColor => _uiConfig.BorderColor;

        // Font Properties - delegated to service
        public string FontFamilyName => _uiConfig.FontFamilyName;
        
        /// <summary>
        /// Creates a font with the specified size and style
        /// </summary>
        public Font CreateFont(float size, FontStyle style = FontStyle.Regular)
        {
            return _uiConfig.CreateFont(size, style);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize services
        /// </summary>
        private void InitializeServices()
        {
            _recorder.ActionRecorded += OnActionRecorded;
            _player.PlaybackStarted += OnPlaybackStarted;
            _player.PlaybackStopped += OnPlaybackStopped;
            
            // Configurar función para cargar macros en MacroPlayer (para acciones tipo Macro)
            _player.LoadMacroFunc = (macroId) => _settingsManager.LoadMacro(macroId);
        }


        /// <summary>
        /// Load all macros and display UI
        /// </summary>
        private void LoadMacros()
        {
            _loadedMacros = _settingsManager.LoadAllMacros();
        }

        /// <summary>
        /// Load all shortcuts and display UI
        /// </summary>
        private void LoadShortcuts()
        {
            _loadedShortcuts = _settingsManager.LoadAllShortcuts();
        }

        /// <summary>
        /// Refresh loaded macros from disk
        /// </summary>
        public void RefreshLoadedMacros()
        {
            _loadedMacros = _settingsManager.LoadAllMacros();
            MacrosChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Macro Management

        /// <summary>
        /// Load a macro for editing
        /// </summary>
        public void LoadMacro(MacroConfig macro)
        {
            _currentMacro = macro;
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Create a new macro
        /// </summary>
        public void CreateNewMacro()
        {
            _currentMacro = new MacroConfig { Name = $"Macro_{DateTime.Now:yyyyMMdd_HHmmss}" };
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Start recording a new macro
        /// </summary>
        public void StartRecording()
        {
            // Asegurar que no hay shortcut activo cuando empezamos a grabar un macro
            _currentShortcut = null;
            _currentMacro = new MacroConfig { Name = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}" };
            _recorder.StartRecording();
            RecordingStarted?.Invoke(this, EventArgs.Empty);
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void StopRecording()
        {
            _recorder.StopRecording();
            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Save current macro
        /// </summary>
        public bool SaveCurrentMacro()
        {
            if (_currentMacro == null)
                return false;

            if (!ValidateMacro(_currentMacro))
                return false;

            _currentMacro.LastModified = DateTime.Now;
            _currentMacro.LastUsed = DateTime.Now;
            bool success = _settingsManager.SaveMacro(_currentMacro);
            
            if (success)
            {
                // Refresh the list from disk to ensure consistency
                RefreshLoadedMacros();
            }
            
            return success;
        }

        /// <summary>
        /// Save a specific macro
        /// </summary>
        public bool SaveMacro(MacroConfig macro)
        {
            if (macro == null)
                return false;

            if (!ValidateMacro(macro))
                return false;

            macro.LastModified = DateTime.Now;
            macro.LastUsed = DateTime.Now;
            bool success = _settingsManager.SaveMacro(macro);
            
            if (success)
            {
                // Refresh the list from disk to ensure consistency
                RefreshLoadedMacros();
            }
            
            return success;
        }

        /// <summary>
        /// Delete current macro
        /// </summary>
        public bool DeleteCurrentMacro()
        {
            if (_currentMacro == null)
                return false;

            bool success = _settingsManager.DeleteMacro(_currentMacro.Id);
            if (success)
            {
                _loadedMacros.Remove(_currentMacro);
                _currentMacro = null;
                MacrosChanged?.Invoke(this, EventArgs.Empty);
                CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
            }
            
            return success;
        }

        /// <summary>
        /// Export current macro
        /// </summary>
        public bool ExportMacro(string filePath)
        {
            if (_currentMacro == null)
                return false;

            return _settingsManager.ExportMacro(_currentMacro, filePath);
        }

        /// <summary>
        /// Import macro from file
        /// </summary>
        public MacroConfig ImportMacro(string filePath)
        {
            var imported = _settingsManager.ImportMacro(filePath);
            if (imported != null)
            {
                // Refresh the list from disk to ensure consistency
                RefreshLoadedMacros();
            }
            return imported;
        }

        /// <summary>
        /// Rename current macro
        /// </summary>
        public void RenameCurrentMacro(string newName)
        {
            if (_currentMacro != null && !string.IsNullOrEmpty(newName))
            {
                _currentMacro.Name = newName;
                SaveCurrentMacro();
            }
        }

        #endregion

        #region Shortcut Management

        /// <summary>
        /// Load a shortcut for editing
        /// </summary>
        public void LoadShortcut(MacroConfig shortcut)
        {
            _currentShortcut = shortcut;
            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Create a new shortcut
        /// </summary>
        public void CreateNewShortcut()
        {
            _currentShortcut = new MacroConfig { Name = $"Shortcut_{DateTime.Now:yyyyMMdd_HHmmss}" };
            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Start recording a new shortcut
        /// </summary>
        public void StartRecordingShortcut()
        {
            // Asegurar que no hay macro activo cuando empezamos a grabar un shortcut
            _currentMacro = null;
            _currentShortcut = new MacroConfig { Name = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}" };
            _recorder.StartRecording();
            ShortcutRecordingStarted?.Invoke(this, EventArgs.Empty);
            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stop recording shortcut
        /// </summary>
        public void StopRecordingShortcut()
        {
            _recorder.StopRecording();
            // Las acciones ya fueron agregadas a _currentShortcut en OnActionRecorded
            // Ahora solo necesitamos actualizar el estado y refrescar la UI
            if (_currentShortcut != null && _currentShortcut.Actions.Count > 0)
            {
                // El shortcut ya tiene las acciones, solo necesitamos notificar
                CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
            }
            ShortcutRecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Save current shortcut
        /// </summary>
        public bool SaveCurrentShortcut()
        {
            if (_currentShortcut == null)
                return false;

            if (!ValidateMacro(_currentShortcut))
                return false;

            _currentShortcut.LastModified = DateTime.Now;
            _currentShortcut.LastUsed = DateTime.Now;
            bool success = _settingsManager.SaveShortcut(_currentShortcut);
            
            if (success)
            {
                RefreshLoadedShortcuts();
            }
            
            return success;
        }

        /// <summary>
        /// Save a specific shortcut
        /// </summary>
        public bool SaveShortcut(MacroConfig shortcut)
        {
            if (shortcut == null)
                return false;

            if (!ValidateMacro(shortcut))
                return false;

            shortcut.LastModified = DateTime.Now;
            shortcut.LastUsed = DateTime.Now;
            bool success = _settingsManager.SaveShortcut(shortcut);
            
            if (success)
            {
                RefreshLoadedShortcuts();
            }
            
            return success;
        }

        /// <summary>
        /// Delete current shortcut
        /// </summary>
        public bool DeleteCurrentShortcut()
        {
            if (_currentShortcut == null)
                return false;

            bool success = _settingsManager.DeleteShortcut(_currentShortcut.Id);
            if (success)
            {
                _loadedShortcuts.Remove(_currentShortcut);
                _currentShortcut = null;
                ShortcutsChanged?.Invoke(this, EventArgs.Empty);
                CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
            }
            
            return success;
        }

        /// <summary>
        /// Export current shortcut
        /// </summary>
        public bool ExportShortcut(string filePath)
        {
            if (_currentShortcut == null)
                return false;

            return _settingsManager.ExportShortcut(_currentShortcut, filePath);
        }

        /// <summary>
        /// Import shortcut from file
        /// </summary>
        public MacroConfig ImportShortcut(string filePath)
        {
            var imported = _settingsManager.ImportShortcut(filePath);
            if (imported != null)
            {
                RefreshLoadedShortcuts();
            }
            return imported;
        }

        /// <summary>
        /// Rename current shortcut
        /// </summary>
        public void RenameCurrentShortcut(string newName)
        {
            if (_currentShortcut != null && !string.IsNullOrEmpty(newName))
            {
                _currentShortcut.Name = newName;
                SaveCurrentShortcut();
            }
        }

        /// <summary>
        /// Refresh loaded shortcuts from disk
        /// </summary>
        public void RefreshLoadedShortcuts()
        {
            _loadedShortcuts = _settingsManager.LoadAllShortcuts();
            ShortcutsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Get last used shortcut or first available
        /// </summary>
        public MacroConfig GetLastShortcut()
        {
            if (_loadedShortcuts.Count == 0)
                return null;

            return _loadedShortcuts.OrderByDescending(s => s.LastUsed).FirstOrDefault();
        }

        /// <summary>
        /// Get shortcut file path
        /// </summary>
        public string GetShortcutFilePath(MacroConfig shortcut)
        {
            if (shortcut == null)
                return null;

            string shortcutsDirectory = _settingsManager.GetShortcutsDirectory();
            string fileName = $"{SanitizeFileName(shortcut.Name)}.json";
            return Path.Combine(shortcutsDirectory, fileName);
        }

        /// <summary>
        /// Notifies that the current shortcut has changed
        /// </summary>
        public void NotifyCurrentShortcutChanged()
        {
            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifies that shortcuts have changed
        /// </summary>
        public void NotifyShortcutsChanged()
        {
            ShortcutsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Action Management

        /// <summary>
        /// Add a new action to current macro
        /// </summary>
        public void AddNewAction()
        {
            if (_currentMacro == null)
                return;

            var newAction = new MacroAction
            {
                Type = ActionType.KeyPress,
                KeyCode = (int)Keys.A,
                DelayMs = 0
            };

            if (ValidateAction(newAction))
            {
                _currentMacro.Actions.Add(newAction);
                CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Delete action at index
        /// </summary>
        public void DeleteAction(int actionIndex)
        {
            if (_currentMacro == null || actionIndex < 0 || actionIndex >= _currentMacro.Actions.Count)
                return;

            _currentMacro.Actions.RemoveAt(actionIndex);
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Duplicate action at index
        /// </summary>
        public void DuplicateAction(int actionIndex)
        {
            if (_currentMacro == null || actionIndex < 0 || actionIndex >= _currentMacro.Actions.Count)
                return;

            var originalAction = _currentMacro.Actions[actionIndex];
            var duplicateAction = new MacroAction
            {
                Type = originalAction.Type,
                KeyCode = originalAction.KeyCode,
                DelayMs = originalAction.DelayMs,
                X = originalAction.X,
                Y = originalAction.Y,
                TimestampMs = originalAction.TimestampMs
            };

            _currentMacro.Actions.Insert(actionIndex + 1, duplicateAction);
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Update action at index
        /// </summary>
        public void UpdateAction(int actionIndex, string keyText, int delay, ActionType actionType)
        {
            if (_currentMacro == null || actionIndex < 0 || actionIndex >= _currentMacro.Actions.Count)
                return;

            var action = _currentMacro.Actions[actionIndex];
            action.DelayMs = delay;
            action.Type = actionType;

            // Update key (for keyboard actions)
            if (!string.IsNullOrEmpty(keyText))
            {
                try
                {
                    Keys key = (Keys)Enum.Parse(typeof(Keys), keyText, true);
                    action.KeyCode = (int)key;
                }
                catch
                {
                    // Keep previous value if invalid
                }
            }

            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Add a new action to current shortcut
        /// </summary>
        public void AddNewShortcutAction()
        {
            if (_currentShortcut == null)
                return;

            var newAction = new MacroAction
            {
                Type = ActionType.KeyPress,
                KeyCode = (int)Keys.A,
                DelayMs = 0
            };

            if (ValidateAction(newAction))
            {
                _currentShortcut.Actions.Add(newAction);
                CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Delete shortcut action at index
        /// </summary>
        public void DeleteShortcutAction(int actionIndex)
        {
            if (_currentShortcut == null || actionIndex < 0 || actionIndex >= _currentShortcut.Actions.Count)
                return;

            _currentShortcut.Actions.RemoveAt(actionIndex);
            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Duplicate shortcut action at index
        /// </summary>
        public void DuplicateShortcutAction(int actionIndex)
        {
            if (_currentShortcut == null || actionIndex < 0 || actionIndex >= _currentShortcut.Actions.Count)
                return;

            var originalAction = _currentShortcut.Actions[actionIndex];
            var duplicateAction = new MacroAction
            {
                Type = originalAction.Type,
                KeyCode = originalAction.KeyCode,
                DelayMs = originalAction.DelayMs,
                X = originalAction.X,
                Y = originalAction.Y,
                TimestampMs = originalAction.TimestampMs
            };

            _currentShortcut.Actions.Insert(actionIndex + 1, duplicateAction);
            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Update shortcut action at index
        /// </summary>
        public void UpdateShortcutAction(int actionIndex, string keyText, int delay, ActionType actionType, Guid? macroId = null)
        {
            if (_currentShortcut == null || actionIndex < 0 || actionIndex >= _currentShortcut.Actions.Count)
                return;

            var action = _currentShortcut.Actions[actionIndex];
            action.DelayMs = delay;
            action.Type = actionType;

            // Update key (for keyboard actions) or MacroId (for macro actions)
            if (actionType == ActionType.Macro)
            {
                action.MacroId = macroId;
            }
            else
            {
                action.MacroId = null;
                if (!string.IsNullOrEmpty(keyText))
                {
                    try
                    {
                        Keys key = (Keys)Enum.Parse(typeof(Keys), keyText, true);
                        action.KeyCode = (int)key;
                    }
                    catch
                    {
                        // Keep previous value if invalid
                    }
                }
            }

            CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Playback Control

        /// <summary>
        /// Play current macro
        /// </summary>
        public async Task PlayCurrentMacro(int repeatCount = 1, bool releaseModifiersFirst = false)
        {
            if (_currentMacro?.Actions.Count == 0)
                return;

            // Stop any current playback
            if (_player.IsPlaying)
            {
                _player.ForceStop();
            }
            // Update last used and persist before playback
            try
            {
                _currentMacro.LastUsed = DateTime.Now;
                _ = SaveCurrentMacro();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PlayCurrentMacro persist LastUsed failed: {ex.Message}");
            }
            await _player.PlayAsync(_currentMacro, repeatCount, releaseModifiersFirst);
        }

        /// <summary>
        /// Stop current playback
        /// </summary>
        public void StopPlayback()
        {
            _player.ForceStop();
        }

        public void PausePlayback()
        {
            _player.Pause();
            PlaybackPaused?.Invoke(this, EventArgs.Empty);
        }

        public void ResumePlayback()
        {
            _player.Resume();
            PlaybackResumed?.Invoke(this, EventArgs.Empty);
        }

        public void SetConditionalArmed(bool armed)
        {
            ConditionalArmedChanged?.Invoke(this, armed);
        }

        #endregion

        #region Public Event Triggers

        /// <summary>
        /// Notifies that the current macro has changed
        /// </summary>
        public void NotifyCurrentMacroChanged()
        {
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifies that macros have changed
        /// </summary>
        public void NotifyMacrosChanged()
        {
            MacrosChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates a macro configuration
        /// </summary>
        public bool ValidateMacro(MacroConfig macro)
        {
            if (macro == null)
                return false;
            
            if (string.IsNullOrWhiteSpace(macro.Name))
                return false;
                
            if (macro.Actions == null || macro.Actions.Count == 0)
                return false;
                
            return true;
        }

        /// <summary>
        /// Validates a macro action
        /// </summary>
        public bool ValidateAction(MacroAction action)
        {
            if (action == null)
                return false;
                
            if (action.DelayMs < 0)
                return false;
                
            if (action.TimestampMs < 0)
                return false;
                
            return true;
        }

        #endregion

        #region Utility Methods
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public bool IsTargetWindowActive()
        {
            if (_targetWindowHandle == IntPtr.Zero) return true; // Desktop/global
            try
            {
                var fg = GetForegroundWindow();
                if (!IsWindow(fg)) return false;
                if (_targetProcessId != 0)
                {
                    GetWindowThreadProcessId(fg, out uint fgPid);
                    return fgPid == _targetProcessId;
                }
                return fg == _targetWindowHandle;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"IsTargetWindowActive error: {ex.Message}"); return false; }
        }

        /// <summary>
        /// Get display name for action
        /// </summary>
        public string GetKeyDisplay(MacroAction action)
        {
            if (action.Type == ActionType.Macro && action.MacroId.HasValue)
            {
                var macro = _settingsManager.LoadMacro(action.MacroId.Value);
                return macro != null ? macro.Name : "Macro (no encontrado)";
            }
            
            if (action.Type == ActionType.KeyDown || action.Type == ActionType.KeyUp || action.Type == ActionType.KeyPress)
            {
                return GetKeyName(action.KeyCode);
            }
            return action.Type.ToString();
        }

        /// <summary>
        /// Get key name from key code
        /// </summary>
        public string GetKeyName(int keyCode)
        {
            return ((Keys)keyCode).ToString();
        }

        /// <summary>
        /// Get action type display string
        /// </summary>
        public string GetActionTypeDisplay(ActionType type)
        {
            return type switch
            {
                ActionType.KeyPress => "⌨️  Key Press",
                ActionType.KeyDown => "⬇️  Key Down",
                ActionType.KeyUp => "⬆️  Key Up",
                ActionType.MouseLeftClick => "🖱️  Mouse Left Click",
                ActionType.MouseLeftDown => "🖱️  Mouse Click",
                ActionType.MouseLeftUp => "🖱️  Mouse Release",
                ActionType.MouseRightClick => "🖱️  Mouse Right Click",
                ActionType.MouseRightDown => "🖱️  Right Click",
                ActionType.MouseRightUp => "🖱️  Right Release",
                ActionType.MouseMove => "↔️  Mouse Move",
                ActionType.Delay => "⏱️  Delay",
                ActionType.Macro => "📜  Macro",
                _ => "❓ Unknown"
            };
        }

        /// <summary>
        /// Get last used macro or first available
        /// </summary>
        public MacroConfig GetLastMacro()
        {
            if (_loadedMacros.Count == 0)
                return null;

            return _loadedMacros.OrderByDescending(m => m.LastUsed).FirstOrDefault();
        }

        /// <summary>
        /// Get macro file path
        /// </summary>
        public string GetMacroFilePath(MacroConfig macro)
        {
            if (macro == null)
                return null;

            string macrosDirectory = _settingsManager.GetMacrosDirectory();
            string fileName = $"{SanitizeFileName(macro.Name)}.json";
            return Path.Combine(macrosDirectory, fileName);
        }

        /// <summary>
        /// Sanitize file name for safe file system usage
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "macro";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = new string(fileName.Select(c => 
                invalidChars.Contains(c) ? '_' : c
            ).ToArray());

            return sanitized.Trim();
        }

        #endregion

        #region Event Handlers

        private void OnActionRecorded(object sender, MacroAction action)
        {
            // Solo agregar acción al macro o shortcut que está siendo grabado actualmente
            // Usamos _isRecordingShortcut para determinar si estamos grabando un shortcut
            // El problema es que necesitamos saber qué estamos grabando
            // Solución: usar el nombre del shortcut/macro para detectar si es un recording
            bool isRecordingMacro = _currentMacro != null && _currentMacro.Name.StartsWith("Recording_");
            bool isRecordingShortcut = _currentShortcut != null && _currentShortcut.Name.StartsWith("Recording_");
            
            if (isRecordingMacro && !isRecordingShortcut)
            {
                // Solo estamos grabando un macro
                _currentMacro.Actions.Add(action);
                CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (isRecordingShortcut)
            {
                // Solo estamos grabando un shortcut
                _currentShortcut.Actions.Add(action);
                CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (_currentMacro != null && !isRecordingShortcut)
            {
                // Hay un macro activo pero no estamos grabando un shortcut, agregar al macro
                _currentMacro.Actions.Add(action);
                CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (_currentShortcut != null && !isRecordingMacro)
            {
                // Hay un shortcut activo pero no estamos grabando un macro, agregar al shortcut
                _currentShortcut.Actions.Add(action);
                CurrentShortcutChanged?.Invoke(this, EventArgs.Empty);
            }
            
            ActionRecorded?.Invoke(this, action);
        }

        private void OnPlaybackStarted(object sender, EventArgs e)
        {
            PlaybackStarted?.Invoke(this, e);
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            PlaybackStopped?.Invoke(this, e);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            _recorder?.StopRecording();
            _player?.ForceStop();
        }

        #endregion
    }
}
