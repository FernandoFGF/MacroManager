using System;
using System.Drawing;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    public partial class View
    {
        private Label _lblCoords;
        private Button _btnZone;
        private Panel _zonePanel;
        private void CreateRuleEditorWithButtons(Control parent)
        {
            Panel ruleEditorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = _model.PanelBackColor
            };

            // Removed instructional title to simplify the editor

            Label lblKey = new Label
            {
                Text = "⌨️ Key:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };

            TextBox txtKey = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 10),
                Name = "txtKey",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtKey.TextChanged += (s, e) => SaveActionChanges();
            txtKey.KeyDown += (s, e) => {
                _suppressEditorEvents = true;
                try
                {
                    _txtKey.Text = e.KeyCode.ToString();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                finally
                {
                    _suppressEditorEvents = false;
                }
                SaveActionChanges();
            };
            // Add input first, then label, so label appears above with DockStyle.Top
            ruleEditorPanel.Controls.Add(txtKey);
            ruleEditorPanel.Controls.Add(lblKey);

            Label lblDelay = new Label
            {
                Text = "⏱️ Delay (ms):",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };

            TextBox txtDelay = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 10),
                Name = "txtDelay",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtDelay.TextChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(txtDelay);
            ruleEditorPanel.Controls.Add(lblDelay);

            Label lblActionType = new Label
            {
                Text = "🎯 Action Type:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };

            ComboBox cmbActionType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 9),
                Name = "cmbActionType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat
            };
            cmbActionType.Items.AddRange(new[] { "KeyPress", "KeyDown", "KeyUp", "MouseLeftDown", "MouseLeftUp", "MouseLeftClick", "MouseRightDown", "MouseRightUp", "MouseRightClick", "MouseMove", "Delay" });
            cmbActionType.SelectedIndex = 0;
            cmbActionType.SelectedIndexChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(cmbActionType);
            ruleEditorPanel.Controls.Add(lblActionType);

            // Panel inferior para Zone (solo visible en acciones de ratón)
            Panel zonePanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = _model.PanelBackColor,
                Visible = false
            };

            // Botón Zone para elegir punto en pantalla
            Button btnZone = new Button
            {
                Text = "📍 Zone",
                Location = new Point(10, 5),
                Size = new Size(90, 30),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            btnZone.FlatAppearance.BorderSize = 0;
            ApplyRetroButtonStyle(btnZone, _model.AccentColor, _model.BorderColor);

            // Coordenadas mostradas para acciones de ratón
            Label lblCoords = new Label
            {
                Text = "📍 Coords: -",
                AutoSize = false,
                Location = new Point(110, 5),
                Size = new Size(220, 30),
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };

            btnZone.Click += (s, e) => PickZoneForCurrentAction(lblCoords);
            if (_toolTip != null) _toolTip.SetToolTip(btnZone, "Elegir punto en pantalla");

            zonePanel.Controls.Add(btnZone);
            zonePanel.Controls.Add(lblCoords);
            // Agregar tras otros paneles de fondo para que quede al fondo de todo
            ruleEditorPanel.Controls.Add(zonePanel);

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = _model.PanelBackColor
            };

            Panel topButtonRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = _model.PanelBackColor
            };

            Button btnAdd = new Button
            {
                Text = "➕",
                Location = new Point(10, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => { _controller.AddNewAction(); SetDirty(true); };
            ApplyRetroButtonStyle(btnAdd, _model.AccentColor, _model.BorderColor);
            topButtonRow.Controls.Add(btnAdd);
            if (_toolTip != null) _toolTip.SetToolTip(btnAdd, "Añadir acción");

            Button btnRemove = new Button
            {
                Text = "➖",
                Location = new Point(85, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) =>
            {
                if (_selectedActionIndices.Count > 1)
                {
                    _controller.DeleteActions(_selectedActionIndices);
                }
                else
                {
                    _controller.DeleteAction(_selectedActionIndex);
                }
                SetDirty(true);
            };
            ApplyRetroButtonStyle(btnRemove, Color.FromArgb(150, 30, 30), _model.BorderColor);
            topButtonRow.Controls.Add(btnRemove);
            if (_toolTip != null) _toolTip.SetToolTip(btnRemove, "Eliminar acción(es)");

            Button btnDuplicate = new Button
            {
                Text = "📋",
                Location = new Point(160, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDuplicate.FlatAppearance.BorderSize = 0;
            btnDuplicate.Click += (s, e) =>
            {
                if (_selectedActionIndices.Count > 1)
                {
                    _controller.DuplicateActions(_selectedActionIndices);
                }
                else
                {
                    _controller.DuplicateAction(_selectedActionIndex);
                }
                SetDirty(true);
            };
            ApplyRetroButtonStyle(btnDuplicate, _model.AccentColor, _model.BorderColor);
            topButtonRow.Controls.Add(btnDuplicate);
            if (_toolTip != null) _toolTip.SetToolTip(btnDuplicate, "Duplicar acción(es)");

            buttonPanel.Controls.Add(topButtonRow);

            Panel recordingRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = _model.PanelBackColor
            };

            _btnRecord = new Button
            {
                Text = "🔴",
                Location = new Point(10, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnRecord.FlatAppearance.BorderSize = 0;
            _btnRecord.Click += (s, e) => _controller.StartRecording();
            ApplyRetroButtonStyle(_btnRecord, Color.FromArgb(150, 30, 30), _model.BorderColor);
            recordingRow.Controls.Add(_btnRecord);
            if (_toolTip != null) _toolTip.SetToolTip(_btnRecord, "Iniciar grabación (Ctrl+R)");

            _btnStopRecord = new Button
            {
                Text = "⏹️",
                Location = new Point(85, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnStopRecord.FlatAppearance.BorderSize = 0;
            _btnStopRecord.Click += (s, e) => _controller.StopRecording();
            ApplyRetroButtonStyle(_btnStopRecord, Color.FromArgb(60, 60, 60), _model.BorderColor);
            recordingRow.Controls.Add(_btnStopRecord);
            if (_toolTip != null) _toolTip.SetToolTip(_btnStopRecord, "Detener grabación");

            Button btnSave = new Button
            {
                Text = "💾",
                Location = new Point(160, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SaveAndClearDirty();
            ApplyRetroButtonStyle(btnSave, _model.AccentColor, _model.BorderColor);
            recordingRow.Controls.Add(btnSave);
            if (_toolTip != null) _toolTip.SetToolTip(btnSave, "Guardar macro (Ctrl+S)");

            buttonPanel.Controls.Add(recordingRow);
            ruleEditorPanel.Controls.Add(buttonPanel);

            _txtKey = txtKey;
            _txtDelay = txtDelay;
            _cmbActionType = cmbActionType;
            _lblCoords = lblCoords;
            _btnZone = btnZone;
            _zonePanel = zonePanel;

            parent.Controls.Add(ruleEditorPanel);
        }

        private void LoadActionToEditor(int actionIndex)
        {
            if (_model.CurrentMacro == null || actionIndex < 0 || actionIndex >= _model.CurrentMacro.Actions.Count)
            {
                ClearRuleEditor();
                return;
            }

            _selectedActionIndex = actionIndex;
            var action = _model.CurrentMacro.Actions[actionIndex];

            _suppressEditorEvents = true;
            try
            {
                _txtKey.Text = _model.GetKeyDisplay(action);
                _txtDelay.Text = action.DelayMs.ToString();
                _cmbActionType.SelectedItem = action.Type.ToString();
                bool isMouse = action.Type.ToString().StartsWith("Mouse");
                _zonePanel.Visible = isMouse;
                _btnZone.Enabled = isMouse;
                _lblCoords.Text = isMouse ? $"📍 Coords: {action.X}, {action.Y}" : "";
            }
            finally
            {
                _suppressEditorEvents = false;
            }
        }

        private void ClearRuleEditor()
        {
            _selectedActionIndex = -1;
            _suppressEditorEvents = true;
            try
            {
                _txtKey.Text = "";
                _txtDelay.Text = "0";
                _cmbActionType.SelectedIndex = 0;
            }
            finally
            {
                _suppressEditorEvents = false;
            }
        }

        private void SaveActionChanges()
        {
            if (_suppressEditorEvents) return;
            if (_selectedActionIndex < 0 || _model.CurrentMacro == null || _selectedActionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            if (int.TryParse(_txtDelay.Text, out int delay))
            {
                ActionType actionType = ActionType.KeyPress;
                if (_cmbActionType.SelectedItem != null)
                {
                    Enum.TryParse<ActionType>(_cmbActionType.SelectedItem.ToString(), out actionType);
                }

                _controller.UpdateAction(_selectedActionIndex, _txtKey.Text, delay, actionType);
                var action = _model.CurrentMacro.Actions[_selectedActionIndex];
                bool isMouse = actionType.ToString().StartsWith("Mouse");
                _zonePanel.Visible = isMouse;
                _btnZone.Enabled = isMouse;
                _lblCoords.Text = isMouse ? $"📍 Coords: {action.X}, {action.Y}" : "";
                SetDirty(true);
            }
        }

        private void PickZoneForCurrentAction(Label targetLabel)
        {
            if (_selectedActionIndex < 0 || _model.CurrentMacro == null || _selectedActionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            var action = _model.CurrentMacro.Actions[_selectedActionIndex];
            if (!action.Type.ToString().StartsWith("Mouse")) return;

            // Minimizar ventana propia para que el overlay no tape coords si el usuario quiere ver debajo
            var main = _mainForm;
            bool wasTopMost = main.TopMost;
            try
            {
                using (var overlay = new ZonePickerForm())
                {
                    main.TopMost = false;
                    var result = overlay.ShowDialog(main);
                    if (result == DialogResult.OK)
                    {
                        Point p = overlay.SelectedPoint;
                        action.X = p.X;
                        action.Y = p.Y;
                        targetLabel.Text = $"📍 Coords: {action.X}, {action.Y}";
                        SetDirty(true);
                        RefreshActionsDisplay();
                    }
                }
            }
            finally
            {
                main.TopMost = wasTopMost;
            }
        }
    }
}


