using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using MacroManager.Models;
using MacroManager.Services;
using Newtonsoft.Json.Linq;

namespace MacroManager
{
    /// <summary>
    /// Modelo de datos para la aplicación MacroManager
    /// Contiene toda la lógica de datos, configuración y estado de la aplicación
    /// </summary>
    public class Model
    {
        // Services
        private MacroRecorder _recorder;
        private MacroPlayer _player;
        private SettingsManager _settingsManager;

        // Data
        private List<MacroConfig> _loadedMacros;
        private MacroConfig _currentMacro;

        // UI Configuration
        private int _minWindowWidth = 1000;
        private int _minWindowHeight = 700;
        private int _defaultWindowWidth = 1200;
        private int _defaultWindowHeight = 800;
        private double _treeViewPercentage = 0.25;
        private double _editorPercentage = 0.6666;
        private int _playbackPanelHeight = 80;
        private int _minimumTreeViewWidth = 200;
        private int _minimumEditorWidth = 400;

        // Theme colors
        private Color _panelBackColor;
        private Color _panelForeColor;
        private Color _accentColor;
        private Color _cardBackColor;
        private Color _borderColor;

        // Events
        public event EventHandler<MacroAction> ActionRecorded;
        public event EventHandler PlaybackStarted;
        public event EventHandler PlaybackStopped;
        public event EventHandler MacrosChanged;
        public event EventHandler CurrentMacroChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public Model()
        {
            InitializeServices();
            LoadUIConfiguration();
            ApplySystemTheme();
            LoadMacros();
        }

        #region Properties

        public List<MacroConfig> LoadedMacros => _loadedMacros;
        public MacroConfig CurrentMacro => _currentMacro;
        public bool IsRecording => _recorder?.IsRecording ?? false;
        public bool IsPlaying => _player?.IsPlaying ?? false;

        // UI Configuration Properties
        public int MinWindowWidth => _minWindowWidth;
        public int MinWindowHeight => _minWindowHeight;
        public int DefaultWindowWidth => _defaultWindowWidth;
        public int DefaultWindowHeight => _defaultWindowHeight;
        public double TreeViewPercentage => _treeViewPercentage;
        public double EditorPercentage => _editorPercentage;
        public int PlaybackPanelHeight => _playbackPanelHeight;
        public int MinimumTreeViewWidth => _minimumTreeViewWidth;
        public int MinimumEditorWidth => _minimumEditorWidth;

        // Theme Properties
        public Color PanelBackColor => _panelBackColor;
        public Color PanelForeColor => _panelForeColor;
        public Color AccentColor => _accentColor;
        public Color CardBackColor => _cardBackColor;
        public Color BorderColor => _borderColor;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize services
        /// </summary>
        private void InitializeServices()
        {
            _settingsManager = new SettingsManager();
            _recorder = new MacroRecorder();
            _player = new MacroPlayer();

            _recorder.ActionRecorded += OnActionRecorded;
            _player.PlaybackStarted += OnPlaybackStarted;
            _player.PlaybackStopped += OnPlaybackStopped;
        }

        /// <summary>
        /// Apply retro green LCD theme colors
        /// </summary>
        private void ApplySystemTheme()
        {
            // Retro green LCD color scheme (ignoring system theme)
            // Dark green background with bright LCD green text
            
            // Very dark green background (almost black with green tint)
            // this.BackColor = Color.FromArgb(12, 32, 12);
            
            // Bright LCD green text
            // this.ForeColor = Color.FromArgb(0, 255, 0);
            
            // Panel backgrounds - slightly lighter dark green
            _panelBackColor = Color.FromArgb(15, 40, 15);
            
            // Panel text - bright green
            _panelForeColor = Color.FromArgb(0, 255, 0);
            
            // Accent color - lime green for buttons
            _accentColor = Color.FromArgb(50, 205, 50);
            
            // Card background - dark green with slight variation
            _cardBackColor = Color.FromArgb(18, 48, 18);
            
            // Border color - darker green
            _borderColor = Color.FromArgb(34, 100, 34);
        }

        /// <summary>
        /// Check if Windows system theme is set to dark mode
        /// </summary>
        private bool IsSystemDarkMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppsUseLightTheme");
                        if (value != null && value is int intValue)
                        {
                            return intValue == 0; // 0 = Dark mode, 1 = Light mode
                        }
                    }
                }
            }
            catch
            {
                // If unable to read registry, default to light mode
            }
            return false;
        }

        /// <summary>
        /// Load UI configuration from uiconfig.json
        /// Searches in multiple locations: executable directory, project directory
        /// </summary>
        private void LoadUIConfiguration()
        {
            try
            {
                // Try multiple locations for the config file
                string[] possiblePaths = new string[]
                {
                    // First: Next to the executable (from bin/Release/)
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uiconfig.json"),
                    // Second: In the project root (for development)
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "uiconfig.json"),
                    // Third: In the MacroManager folder (source location)
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "MacroManager", "uiconfig.json"),
                    // Fourth: Try finding it relative to source
                    "uiconfig.json"
                };

                string configPath = null;
                foreach (var path in possiblePaths)
                {
                    string fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        configPath = fullPath;
                        break;
                    }
                }

                if (configPath != null && File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    JObject config = JObject.Parse(jsonContent);

                    // Load window settings
                    if (config["window"] != null)
                    {
                        _minWindowWidth = config["window"]["minWidth"]?.Value<int>() ?? _minWindowWidth;
                        _minWindowHeight = config["window"]["minHeight"]?.Value<int>() ?? _minWindowHeight;
                        _defaultWindowWidth = config["window"]["defaultWidth"]?.Value<int>() ?? _defaultWindowWidth;
                        _defaultWindowHeight = config["window"]["defaultHeight"]?.Value<int>() ?? _defaultWindowHeight;
                    }

                    // Load layout settings
                    if (config["layout"] != null)
                    {
                        double treePercent = config["layout"]["treeViewPercentage"]?.Value<double>() ?? _treeViewPercentage;
                        _treeViewPercentage = treePercent / 100.0; // Convert percentage to decimal

                        double editorPercent = config["layout"]["editorPercentage"]?.Value<double>() ?? _editorPercentage;
                        _editorPercentage = editorPercent / 100.0; // Convert percentage to decimal

                        _playbackPanelHeight = config["layout"]["playbackPanelHeight"]?.Value<int>() ?? _playbackPanelHeight;
                    }

                    // Load size constraints
                    if (config["sizes"] != null)
                    {
                        _minimumTreeViewWidth = config["sizes"]["minimumTreeViewWidth"]?.Value<int>() ?? _minimumTreeViewWidth;
                        _minimumEditorWidth = config["sizes"]["minimumEditorWidth"]?.Value<int>() ?? _minimumEditorWidth;
                    }

                    System.Diagnostics.Debug.WriteLine($"✓ Config loaded from: {configPath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠ Config file not found. Using default settings.");
                    System.Diagnostics.Debug.WriteLine($"Searched in:\n  - {string.Join("\n  - ", possiblePaths.Select(p => Path.GetFullPath(p)))}");
                }
            }
            catch (Exception ex)
            {
                // If there's an error loading config, use defaults
                System.Diagnostics.Debug.WriteLine($"✗ Error loading config: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all macros and display UI
        /// </summary>
        private void LoadMacros()
        {
            _loadedMacros = _settingsManager.LoadAllMacros();
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
            _currentMacro = new MacroConfig { Name = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}" };
            _recorder.StartRecording();
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void StopRecording()
        {
            _recorder.StopRecording();
        }

        /// <summary>
        /// Save current macro
        /// </summary>
        public bool SaveCurrentMacro()
        {
            if (_currentMacro == null)
                return false;

            _currentMacro.LastModified = DateTime.Now;
            _currentMacro.LastUsed = DateTime.Now;
            bool success = _settingsManager.SaveMacro(_currentMacro);
            
            if (success)
            {
                // Add to loaded macros if not already there
                if (!_loadedMacros.Any(m => m.Id == _currentMacro.Id))
                {
                    _loadedMacros.Add(_currentMacro);
                }
                
                MacrosChanged?.Invoke(this, EventArgs.Empty);
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
                _loadedMacros.Add(imported);
                MacrosChanged?.Invoke(this, EventArgs.Empty);
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

            _currentMacro.Actions.Add(newAction);
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
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

        #endregion

        #region Playback Control

        /// <summary>
        /// Play current macro
        /// </summary>
        public async Task PlayCurrentMacro(int repeatCount = 1)
        {
            if (_currentMacro?.Actions.Count == 0)
                return;

            // Stop any current playback
            if (_player.IsPlaying)
            {
                _player.ForceStop();
            }

            await _player.PlayAsync(_currentMacro, repeatCount);
        }

        /// <summary>
        /// Stop current playback
        /// </summary>
        public void StopPlayback()
        {
            _player.ForceStop();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get display name for action
        /// </summary>
        public string GetKeyDisplay(MacroAction action)
        {
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
                ActionType.KeyPress => "⌨️  Pulsar Tecla",
                ActionType.KeyDown => "⬇️  Tecla Pulsada",
                ActionType.KeyUp => "⬆️  Tecla Soltada",
                ActionType.MouseLeftDown => "🖱️  Click Ratón",
                ActionType.MouseLeftUp => "🖱️  Soltar Ratón",
                ActionType.MouseRightDown => "🖱️  Click Derecho",
                ActionType.MouseRightUp => "🖱️  Soltar Derecho",
                ActionType.MouseMove => "↔️  Mover Ratón",
                ActionType.Delay => "⏱️  Espera",
                _ => "❓ Desconocida"
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

        #endregion

        #region Event Handlers

        private void OnActionRecorded(object sender, MacroAction action)
        {
            if (_currentMacro != null)
            {
                _currentMacro.Actions.Add(action);
                CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
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
