using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MacroManager.Models;
using MacroManager.Services;

namespace MacroManager
{
    /// <summary>
    /// Formulario principal de la aplicación
    /// Gestiona la interfaz de usuario y coordina los servicios
    /// </summary>
    public partial class MainForm : Form
    {
        // Servicios de la aplicación
        private MacroRecorder _recorder;
        private MacroPlayer _player;
        private SettingsManager _settingsManager;

        // Datos actuales
        private List<MacroConfig> _loadedMacros;
        private MacroConfig _currentMacro;
        private MacroConfig _selectedMacro;

        /// <summary>
        /// Constructor del formulario principal
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadMacros();
            UpdateUI();
        }

        /// <summary>
        /// Inicializa los servicios de la aplicación
        /// </summary>
        private void InitializeServices()
        {
            _settingsManager = new SettingsManager();
            _recorder = new MacroRecorder();
            _player = new MacroPlayer();

            // Suscribirse a eventos
            _recorder.ActionRecorded += OnActionRecorded;
            _player.PlaybackStarted += OnPlaybackStarted;
            _player.PlaybackStopped += OnPlaybackStopped;
            _player.PlaybackProgress += OnPlaybackProgress;
        }

        /// <summary>
        /// Carga todas las macros guardadas
        /// </summary>
        private void LoadMacros()
        {
            _loadedMacros = _settingsManager.LoadAllMacros();
            RefreshMacroList();
        }

        /// <summary>
        /// Refresca la lista visual de macros
        /// </summary>
        private void RefreshMacroList()
        {
            lstMacros.Items.Clear();

            foreach (var macro in _loadedMacros)
            {
                string displayText = $"{macro.Name} ({macro.Actions.Count} acciones)";
                if (!string.IsNullOrEmpty(macro.Hotkey))
                    displayText += $" [{macro.Hotkey}]";

                lstMacros.Items.Add(displayText);
            }

            UpdateUI();
        }

        /// <summary>
        /// Actualiza el estado de los controles según el estado actual
        /// </summary>
        private void UpdateUI()
        {
            bool isRecording = _recorder.IsRecording;
            bool isPlaying = _player.IsPlaying;
            bool hasSelection = lstMacros.SelectedIndex >= 0;
            bool hasCurrentMacro = _currentMacro != null && _currentMacro.Actions.Count > 0;

            // Botones de grabación
            btnStartRecord.Enabled = !isRecording && !isPlaying;
            btnStopRecord.Enabled = isRecording;

            // Botones de reproducción
            btnPlay.Enabled = !isPlaying && !isRecording && hasSelection;
            btnStop.Enabled = isPlaying;

            // Botones de gestión
            btnSave.Enabled = hasCurrentMacro && !isRecording && !isPlaying;
            btnDelete.Enabled = hasSelection && !isRecording && !isPlaying;
            btnExport.Enabled = hasSelection && !isPlaying;
            btnImport.Enabled = !isRecording && !isPlaying;

            // Actualizar estado en la barra de estado
            if (isRecording)
            {
                lblStatus.Text = "⏺ Grabando...";
                lblStatus.ForeColor = Color.Red;
            }
            else if (isPlaying)
            {
                lblStatus.Text = "▶ Reproduciendo...";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = "⏸ Listo";
                lblStatus.ForeColor = Color.Black;
            }

            // Actualizar contador de acciones
            if (_currentMacro != null)
            {
                lblActionCount.Text = $"Acciones grabadas: {_currentMacro.Actions.Count}";
            }
            else
            {
                lblActionCount.Text = "Acciones grabadas: 0";
            }
        }

        #region Event Handlers - Botones de Grabación

        private void btnStartRecord_Click(object sender, EventArgs e)
        {
            _currentMacro = new MacroConfig
            {
                Name = $"Macro_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            _recorder.StartRecording();
            UpdateUI();
        }

        private void btnStopRecord_Click(object sender, EventArgs e)
        {
            _recorder.StopRecording();
            
            if (_currentMacro != null)
            {
                _currentMacro.Actions = _recorder.RecordedActions;
                
                // Mostrar diálogo para nombrar la macro
                string newName = PromptForMacroName(_currentMacro.Name);
                if (!string.IsNullOrEmpty(newName))
                {
                    _currentMacro.Name = newName;
                }
            }

            UpdateUI();
        }

        private void OnActionRecorded(object sender, MacroAction action)
        {
            // Actualizar UI desde el hilo principal
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnActionRecorded(sender, action)));
                return;
            }

            UpdateUI();
        }

        #endregion

        #region Event Handlers - Botones de Reproducción

        private async void btnPlay_Click(object sender, EventArgs e)
        {
            if (lstMacros.SelectedIndex < 0)
                return;

            _selectedMacro = _loadedMacros[lstMacros.SelectedIndex];

            // Preguntar cuántas veces repetir
            int repeatCount = PromptForRepeatCount();
            
            UpdateUI();
            await _player.PlayAsync(_selectedMacro, repeatCount);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _player.Stop();
            UpdateUI();
        }

        private void OnPlaybackStarted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPlaybackStarted(sender, e)));
                return;
            }

            UpdateUI();
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPlaybackStopped(sender, e)));
                return;
            }

            UpdateUI();
        }

        private void OnPlaybackProgress(object sender, int iteration)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPlaybackProgress(sender, iteration)));
                return;
            }

            lblStatus.Text = $"▶ Reproduciendo... (Iteración {iteration})";
        }

        #endregion

        #region Event Handlers - Gestión de Macros

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentMacro == null || _currentMacro.Actions.Count == 0)
            {
                MessageBox.Show("No hay macro para guardar.", "Advertencia", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_settingsManager.SaveMacro(_currentMacro))
            {
                MessageBox.Show("Macro guardada correctamente.", "Éxito", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                LoadMacros();
                _currentMacro = null;
                UpdateUI();
            }
            else
            {
                MessageBox.Show("Error al guardar la macro.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstMacros.SelectedIndex < 0)
                return;

            var macro = _loadedMacros[lstMacros.SelectedIndex];

            var result = MessageBox.Show($"¿Está seguro de eliminar la macro '{macro.Name}'?", 
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (_settingsManager.DeleteMacro(macro.Id))
                {
                    LoadMacros();
                }
                else
                {
                    MessageBox.Show("Error al eliminar la macro.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (lstMacros.SelectedIndex < 0)
                return;

            var macro = _loadedMacros[lstMacros.SelectedIndex];

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Archivo de Macro (*.macro)|*.macro|Todos los archivos (*.*)|*.*",
                FileName = $"{macro.Name}.macro",
                Title = "Exportar Macro"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (_settingsManager.ExportMacro(macro, dialog.FileName))
                {
                    MessageBox.Show("Macro exportada correctamente.", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error al exportar la macro.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Archivo de Macro (*.macro)|*.macro|Archivos JSON (*.json)|*.json|Todos los archivos (*.*)|*.*",
                Title = "Importar Macro"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var macro = _settingsManager.ImportMacro(dialog.FileName);
                
                if (macro != null)
                {
                    MessageBox.Show("Macro importada correctamente.", "Éxito", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMacros();
                }
                else
                {
                    MessageBox.Show("Error al importar la macro.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lstMacros_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Solicita al usuario un nombre para la macro
        /// </summary>
        private string PromptForMacroName(string defaultName)
        {
            using (var form = new Form())
            {
                form.Text = "Nombrar Macro";
                form.Width = 400;
                form.Height = 150;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label { Left = 20, Top = 20, Text = "Nombre de la macro:", AutoSize = true };
                var textBox = new TextBox { Left = 20, Top = 45, Width = 340, Text = defaultName };
                var btnOk = new Button { Text = "Aceptar", Left = 220, Width = 70, Top = 75, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Cancelar", Left = 295, Width = 70, Top = 75, DialogResult = DialogResult.Cancel };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : defaultName;
            }
        }

        /// <summary>
        /// Solicita al usuario el número de repeticiones
        /// </summary>
        private int PromptForRepeatCount()
        {
            using (var form = new Form())
            {
                form.Text = "Repeticiones";
                form.Width = 350;
                form.Height = 150;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label { Left = 20, Top = 20, Text = "¿Cuántas veces repetir? (0 = infinito):", AutoSize = true };
                var numericUpDown = new NumericUpDown { Left = 20, Top = 45, Width = 290, Minimum = 0, Maximum = 1000, Value = 1 };
                var btnOk = new Button { Text = "Aceptar", Left = 165, Width = 70, Top = 75, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "Cancelar", Left = 240, Width = 70, Top = 75, DialogResult = DialogResult.Cancel };

                form.Controls.Add(label);
                form.Controls.Add(numericUpDown);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog() == DialogResult.OK ? (int)numericUpDown.Value : 1;
            }
        }

        #endregion

        /// <summary>
        /// Limpia recursos al cerrar
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_recorder.IsRecording)
                _recorder.StopRecording();

            if (_player.IsPlaying)
                _player.Stop();

            base.OnFormClosing(e);
        }
    }
}
