using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;
using MacroManager.Services;
using MacroManager.Commands;

namespace MacroManager
{
    /// <summary>
    /// Main controller for the MacroManager application
    /// Handles all business logic and coordinates between Model and View
    /// </summary>
    public class Controller
    {
        private readonly Model _model;
        private readonly View _view;
        private readonly CommandManager _commandManager;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public Controller(IMacroRecorder recorder, IMacroPlayer player, ISettingsManager settingsManager, UIConfigurationService uiConfig)
        {
            _model = new Model(recorder, player, settingsManager, uiConfig);
            _view = new View(this, _model);
            _commandManager = new CommandManager();
            
            // Subscribe to model events
            _model.ActionRecorded += OnActionRecorded;
            _model.PlaybackStarted += OnPlaybackStarted;
            _model.PlaybackStopped += OnPlaybackStopped;
            _model.MacrosChanged += OnMacrosChanged;
            _model.CurrentMacroChanged += OnCurrentMacroChanged;
        }

        #region Public Interface

        /// <summary>
        /// Get the main form for the application
        /// </summary>
        public Form GetMainForm()
        {
            return _view.GetMainForm();
        }

        /// <summary>
        /// Initialize the application
        /// </summary>
        public void Initialize()
        {
            _view.Initialize();
            LoadLastMacro();
        }

        #endregion

        #region Macro Management Commands

        /// <summary>
        /// Create a new macro
        /// </summary>
        public void CreateNewMacro()
        {
            var command = new CreateMacroCommand(_model);
            _commandManager.ExecuteCommand(command);
        }

        /// <summary>
        /// Start recording a new macro
        /// </summary>
        public void StartRecording()
        {
            _model.StartRecording();
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void StopRecording()
        {
            _model.StopRecording();
        }

        /// <summary>
        /// Save current macro
        /// </summary>
        public void SaveCurrentMacro()
        {
            if (_model.CurrentMacro == null)
            {
                MessageBox.Show("No macro loaded to save.", "Warning");
                return;
            }

            bool success = _model.SaveCurrentMacro();
            if (success)
            {
                MessageBox.Show("Macro saved successfully.", "Success");
            }
            else
            {
                MessageBox.Show("Error saving macro.", "Error");
            }
        }

        /// <summary>
        /// Delete current macro
        /// </summary>
        public void DeleteCurrentMacro()
        {
            if (_model.CurrentMacro == null)
            {
                MessageBox.Show("No macro loaded to delete.", "Warning");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Delete macro '{_model.CurrentMacro.Name}'?",
                "Confirm",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                bool success = _model.DeleteCurrentMacro();
                if (success)
                {
                    LoadLastMacro();
                }
                else
                {
                    MessageBox.Show("Error deleting macro.", "Error");
                }
            }
        }

        /// <summary>
        /// Export current macro
        /// </summary>
        public void ExportMacro()
        {
            if (_model.CurrentMacro == null)
            {
                MessageBox.Show("No macro loaded to export.", "Warning");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Macro Files|*.macro|JSON Files|*.json",
                FileName = _model.CurrentMacro.Name,
                DefaultExt = ".macro"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                bool success = _model.ExportMacro(sfd.FileName);
                if (success)
                {
                    MessageBox.Show("Macro exported successfully.", "Success");
                }
                else
                {
                    MessageBox.Show("Error exporting macro.", "Error");
                }
            }
        }

        /// <summary>
        /// Import macro from file
        /// </summary>
        public void ImportMacro()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Macro Files|*.macro|JSON Files|*.json"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var imported = _model.ImportMacro(ofd.FileName);
                if (imported != null)
                {
                    _model.LoadMacro(imported);
                    MessageBox.Show("Macro imported successfully.", "Success");
                }
                else
                {
                    MessageBox.Show("Error importing macro.", "Error");
                }
            }
        }

        /// <summary>
        /// Rename current macro
        /// </summary>
        public void RenameCurrentMacro()
        {
            if (_model.CurrentMacro == null) return;

            string newName = PromptInput("Enter new name:", _model.CurrentMacro.Name);
            if (!string.IsNullOrEmpty(newName))
            {
                _model.RenameCurrentMacro(newName);
            }
        }

        /// <summary>
        /// Load a specific macro
        /// </summary>
        public void LoadMacro(MacroConfig macro)
        {
            _model.LoadMacro(macro);
        }

        /// <summary>
        /// Refresh macros from disk
        /// </summary>
        public void RefreshMacros()
        {
            _model.RefreshLoadedMacros();
        }

        /// <summary>
        /// Duplicate current macro
        /// </summary>
        public void DuplicateCurrentMacro()
        {
            if (_model.CurrentMacro == null)
            {
                MessageBox.Show("No macro loaded to duplicate.", "Warning");
                return;
            }

            string newName = PromptInput("Enter name for duplicate:", $"{_model.CurrentMacro.Name}_Copy");
            if (!string.IsNullOrEmpty(newName))
            {
                var duplicatedMacro = new MacroConfig
                {
                    Name = newName,
                    Actions = new List<MacroAction>(_model.CurrentMacro.Actions.Select(a => new MacroAction
                    {
                        Type = a.Type,
                        KeyCode = a.KeyCode,
                        DelayMs = a.DelayMs,
                        X = a.X,
                        Y = a.Y,
                        TimestampMs = a.TimestampMs
                    })),
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    LastUsed = DateTime.Now
                };

                // Save the duplicated macro instead of the current one
                bool success = _model.SaveMacro(duplicatedMacro);
                if (success)
                {
                    _model.LoadMacro(duplicatedMacro);
                    MessageBox.Show("Macro duplicated successfully.", "Success");
                }
                else
                {
                    MessageBox.Show("Error duplicating macro.", "Error");
                }
            }
        }

        /// <summary>
        /// Open macro file location in explorer
        /// </summary>
        public void OpenMacroLocation(MacroConfig macro)
        {
            try
            {
                // Get the macro file path from model
                string macroPath = _model.GetMacroFilePath(macro);
                if (!string.IsNullOrEmpty(macroPath) && System.IO.File.Exists(macroPath))
                {
                    // Open file location in Windows Explorer
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{macroPath}\"");
                }
                else
                {
                    MessageBox.Show("Macro file location not found.", "Warning");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file location: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Show macro properties dialog
        /// </summary>
        public void ShowMacroProperties(MacroConfig macro)
        {
            string properties = $"Name: {macro.Name}\n" +
                              $"Actions: {macro.Actions.Count}\n" +
                              $"Created: {macro.CreatedDate:dd/MM/yyyy HH:mm:ss}\n" +
                              $"Modified: {macro.LastModified:dd/MM/yyyy HH:mm:ss}\n" +
                              $"Last used: {macro.LastUsed:dd/MM/yyyy HH:mm:ss}\n" +
                              $"ID: {macro.Id}";

            MessageBox.Show(properties, "Macro Properties", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Action Management Commands

        /// <summary>
        /// Add a new action to current macro
        /// </summary>
        public void AddNewAction()
        {
            if (_model.CurrentMacro == null)
            {
                MessageBox.Show("No macro is currently open.", "Info");
                return;
            }

            var command = new AddActionCommand(_model);
            _commandManager.ExecuteCommand(command);
        }

        /// <summary>
        /// Delete action at index
        /// </summary>
        public void DeleteAction(int actionIndex)
        {
            if (actionIndex < 0 || _model.CurrentMacro == null || actionIndex >= _model.CurrentMacro.Actions.Count)
            {
                MessageBox.Show("Please select an action to delete.", "Info");
                return;
            }

            var command = new DeleteActionCommand(_model, actionIndex);
            _commandManager.ExecuteCommand(command);
        }

        /// <summary>
        /// Delete multiple actions by indices
        /// </summary>
        public void DeleteActions(IEnumerable<int> actionIndices)
        {
            if (_model.CurrentMacro == null || actionIndices == null) return;
            var list = actionIndices.Distinct().Where(i => i >= 0 && i < _model.CurrentMacro.Actions.Count).OrderByDescending(i => i).ToList();
            if (list.Count == 0)
            {
                MessageBox.Show("Please select actions to delete.", "Info");
                return;
            }
            foreach (var idx in list)
            {
                _model.DeleteAction(idx);
            }
        }

        /// <summary>
        /// Duplicate action at index
        /// </summary>
        public void DuplicateAction(int actionIndex)
        {
            if (actionIndex < 0 || _model.CurrentMacro == null || actionIndex >= _model.CurrentMacro.Actions.Count)
            {
                MessageBox.Show("Please select an action to duplicate.", "Info");
                return;
            }

            _model.DuplicateAction(actionIndex);
        }

        /// <summary>
        /// Duplicate multiple actions by indices
        /// </summary>
        public void DuplicateActions(IEnumerable<int> actionIndices)
        {
            if (_model.CurrentMacro == null || actionIndices == null) return;
            var list = actionIndices.Distinct().Where(i => i >= 0 && i < _model.CurrentMacro.Actions.Count).OrderBy(i => i).ToList();
            if (list.Count == 0)
            {
                MessageBox.Show("Please select actions to duplicate.", "Info");
                return;
            }
            foreach (var idx in list)
            {
                _model.DuplicateAction(idx);
            }
        }

        /// <summary>
        /// Update action at index
        /// </summary>
        public void UpdateAction(int actionIndex, string keyText, int delay, ActionType actionType)
        {
            _model.UpdateAction(actionIndex, keyText, delay, actionType);
        }

        #endregion

        #region Playback Control Commands

        /// <summary>
        /// Toggle play/pause/stop for current macro
        /// </summary>
        public async Task TogglePlayPauseStop(int repeatCount = 1)
        {
            if (_model.CurrentMacro?.Actions.Count == 0)
            {
                MessageBox.Show("No actions to play.", "Warning");
                return;
            }

            if (_model.IsPlaying)
            {
                _model.StopPlayback();
            }
            else
            {
                await _model.PlayCurrentMacro(repeatCount);
            }
        }

        /// <summary>
        /// Stop current playback
        /// </summary>
        public void StopPlayback()
        {
            _model.StopPlayback();
        }

        #endregion

        #region UI Helper Commands

        /// <summary>
        /// Show first macro prompt if no macros exist
        /// </summary>
        public void ShowFirstMacroPrompt()
        {
            DialogResult result = MessageBox.Show(
                "No macros found. Would you like to create your first macro?",
                "MacroManager",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                CreateNewMacro();
            }
        }

        /// <summary>
        /// Show original JSON of current macro
        /// </summary>
        public void ShowOriginalJSON()
        {
            if (_model.CurrentMacro != null)
            {
                // Generate JSON representation of the macro
                string json = System.Text.Json.JsonSerializer.Serialize(_model.CurrentMacro, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                MessageBox.Show(json, "Macro JSON", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No macro loaded to show JSON", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Edit Commands (Legacy - Not applicable to button interface)

        public void UndoAction()
        {
            if (_commandManager.CanUndo)
            {
                _commandManager.Undo();
            }
            else
            {
                MessageBox.Show("No actions to undo", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void RedoAction()
        {
            if (_commandManager.CanRedo)
            {
                _commandManager.Redo();
            }
            else
            {
                MessageBox.Show("No actions to redo", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void CutAction()
        {
            MessageBox.Show("Cut functionality not available in button interface", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void CopyAction()
        {
            MessageBox.Show("Copy functionality not available in button interface", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void PasteAction()
        {
            MessageBox.Show("Paste functionality not available in button interface", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SelectAllAction()
        {
            MessageBox.Show("Select all functionality not available in button interface", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Load last used macro or prompt to create first
        /// </summary>
        private void LoadLastMacro()
        {
            if (_model.LoadedMacros.Count == 0)
            {
                ShowFirstMacroPrompt();
            }
            else
            {
                var lastMacro = _model.GetLastMacro();
                if (lastMacro != null)
                {
                    LoadMacro(lastMacro);
                }
            }
        }

        /// <summary>
        /// Show input prompt dialog
        /// </summary>
        private string PromptInput(string prompt, string defaultValue = "")
        {
            Form form = new Form
            {
                Text = "Input",
                Width = 300,
                Height = 150,
                StartPosition = FormStartPosition.CenterParent,
                Owner = _view.GetMainForm()
            };

            Label label = new Label { Text = prompt, Top = 20, Left = 20, Width = 250 };
            TextBox textBox = new TextBox { Top = 50, Left = 20, Width = 250, Text = defaultValue };
            Button btn = new Button { Text = "OK", Top = 80, Left = 150, Width = 80, DialogResult = DialogResult.OK };

            form.Controls.Add(label);
            form.Controls.Add(textBox);
            form.Controls.Add(btn);
            form.AcceptButton = btn;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        #endregion

        #region Event Handlers

        private void OnActionRecorded(object sender, MacroAction action)
        {
            // This is handled by the model, just pass through
        }

        private void OnPlaybackStarted(object sender, EventArgs e)
        {
            // This is handled by the model, just pass through
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            // This is handled by the model, just pass through
        }

        private void OnMacrosChanged(object sender, EventArgs e)
        {
            // This is handled by the model, just pass through
        }

        private void OnCurrentMacroChanged(object sender, EventArgs e)
        {
            // This is handled by the model, just pass through
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            _model?.Dispose();
            _view?.Dispose();
        }

        #endregion
    }
}
