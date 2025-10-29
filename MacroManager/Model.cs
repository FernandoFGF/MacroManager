using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;
using MacroManager.Services;

namespace MacroManager
{
    /// <summary>
    /// Modelo de datos para la aplicación MacroManager
    /// Contiene toda la lógica de datos, configuración y estado de la aplicación
    /// </summary>
    public class Model
    {
        // Services - ahora inyectados
        private readonly IMacroRecorder _recorder;
        private readonly IMacroPlayer _player;
        private readonly ISettingsManager _settingsManager;
        private readonly UIConfigurationService _uiConfig;

        // Data
        private List<MacroConfig> _loadedMacros;
        private MacroConfig _currentMacro;

        // Events
        public event EventHandler<MacroAction> ActionRecorded;
        public event EventHandler PlaybackStarted;
        public event EventHandler PlaybackStopped;
        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler MacrosChanged;
        public event EventHandler CurrentMacroChanged;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        public Model(IMacroRecorder recorder, IMacroPlayer player, ISettingsManager settingsManager, UIConfigurationService uiConfig)
        {
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _uiConfig = uiConfig ?? throw new ArgumentNullException(nameof(uiConfig));

            InitializeServices();
            LoadMacros();
        }

        #region Properties

        public List<MacroConfig> LoadedMacros => _loadedMacros;
        public MacroConfig CurrentMacro => _currentMacro;
        public bool IsRecording => _recorder?.IsRecording ?? false;
        public bool IsPlaying => _player?.IsPlaying ?? false;

        // UI Configuration Properties - delegadas al servicio
        public int MinWindowWidth => _uiConfig.MinWindowWidth;
        public int MinWindowHeight => _uiConfig.MinWindowHeight;
        public int DefaultWindowWidth => _uiConfig.DefaultWindowWidth;
        public int DefaultWindowHeight => _uiConfig.DefaultWindowHeight;
        public double TreeViewPercentage => _uiConfig.TreeViewPercentage;
        public double EditorPercentage => _uiConfig.EditorPercentage;
        public int PlaybackPanelHeight => _uiConfig.PlaybackPanelHeight;
        public int MinimumTreeViewWidth => _uiConfig.MinimumTreeViewWidth;
        public int MinimumEditorWidth => _uiConfig.MinimumEditorWidth;

        // Theme Properties - delegadas al servicio
        public Color PanelBackColor => _uiConfig.PanelBackColor;
        public Color PanelForeColor => _uiConfig.PanelForeColor;
        public Color AccentColor => _uiConfig.AccentColor;
        public Color CardBackColor => _uiConfig.CardBackColor;
        public Color BorderColor => _uiConfig.BorderColor;

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
        }


        /// <summary>
        /// Load all macros and display UI
        /// </summary>
        private void LoadMacros()
        {
            _loadedMacros = _settingsManager.LoadAllMacros();
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

        #region Public Event Triggers

        /// <summary>
        /// Notifica que la macro actual ha cambiado
        /// </summary>
        public void NotifyCurrentMacroChanged()
        {
            CurrentMacroChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifica que las macros han cambiado
        /// </summary>
        public void NotifyMacrosChanged()
        {
            MacrosChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Valida una configuración de macro
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
        /// Valida una acción de macro
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
