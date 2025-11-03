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
        private Label _lblKey;
        private Label _lblZone;
        private Label _lblDelay;
        private Label _lblActionType;
        private void CreateRuleEditorWithButtons(Control parent)
        {
            Panel ruleEditorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = _model.PanelBackColor
            };

            // Removed instructional title to simplify the editor

            // Key - Label (condicional, invisible por defecto)
            Label lblKey = new Label
            {
                Text = "⌨️ Key:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Visible = false, // INVISIBLE por defecto
                Margin = new Padding(0, 10, 0, 0) // Espacio arriba
            };

            // Key - TextBox (condicional, invisible por defecto)
            TextBox txtKey = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(10),
                Name = "txtKey",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false // INVISIBLE por defecto
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

            Label lblDelay = new Label
            {
                Text = "⏱️ Delay (ms):",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };

            TextBox txtDelay = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(10),
                Name = "txtDelay",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtDelay.TextChanged += (s, e) => SaveActionChanges();

            Label lblActionType = new Label
            {
                Text = "🎯 Action:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Margin = new Padding(0, 10, 0, 0) // Espacio arriba
            };

            ComboBox cmbActionType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(9),
                Name = "cmbActionType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat
            };
            cmbActionType.Items.AddRange(new[] { "KeyPress", "KeyDown", "KeyUp", "MouseLeftDown", "MouseLeftUp", "MouseLeftClick", "MouseRightDown", "MouseRightUp", "MouseRightClick", "MouseMove", "Delay" });
            cmbActionType.SelectedIndex = 0;

            // Zone - Label (condicional, invisible por defecto)
            Label lblZone = new Label
            {
                Text = "📍 Zone:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Visible = false, // INVISIBLE por defecto
                Margin = new Padding(0, 10, 0, 0) // Espacio arriba
            };

            // Zone - Panel con botón y coordenadas (condicional, invisible por defecto)
            Panel zonePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = _model.PanelBackColor,
                Visible = false // INVISIBLE por defecto
            };

            Label lblCoords = new Label
            {
                Text = "📍 Coords: -",
                Dock = DockStyle.Top,
                Height = 30,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };

            // Botón Zone para elegir punto en pantalla
            Button btnZone = new Button
            {
                Text = "📍 Zone",
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            btnZone.FlatAppearance.BorderSize = 0;
            ApplyRetroButtonStyle(btnZone, _model.AccentColor, _model.BorderColor);

            btnZone.Click += (s, e) => PickZoneForCurrentAction(lblCoords);
            if (_toolTip != null) _toolTip.SetToolTip(btnZone, "Elegir punto en pantalla");

            zonePanel.Controls.Add(lblCoords);
            zonePanel.Controls.Add(btnZone);
            
            // Agregar controles desde el ÚLTIMO visible hacia el PRIMERO
            // Con DockStyle.Top: último agregado = más arriba visualmente
            // Orden deseado: Delay, Action, [Key/Zone condicional]
            // Agregar en orden inverso:
            
            // PRIMERO: Condicionales (abajo, invisibles por defecto)
            ruleEditorPanel.Controls.Add(txtKey);          // Key textbox
            ruleEditorPanel.Controls.Add(lblKey);         // Key label (aparece arriba del textbox)
            
            ruleEditorPanel.Controls.Add(zonePanel);       // Zone panel
            ruleEditorPanel.Controls.Add(lblZone);        // Zone label (aparece arriba del panel)
            
            // SEGUNDO: Action
            ruleEditorPanel.Controls.Add(cmbActionType);   // Action selector
            ruleEditorPanel.Controls.Add(lblActionType);  // Action label (aparece arriba del selector)
            
            // TERCERO: Delay (más arriba, siempre visible)
            ruleEditorPanel.Controls.Add(txtDelay);        // Delay textbox
            ruleEditorPanel.Controls.Add(lblDelay);       // Delay label (aparece más arriba)
            
            // Función para actualizar visibilidad según el tipo de acción
            void UpdateEditorVisibility()
            {
                if (cmbActionType.SelectedItem == null) return;
                
                string actionTypeStr = cmbActionType.SelectedItem.ToString();
                bool isKeyboard = actionTypeStr == "KeyPress" || actionTypeStr == "KeyDown" || actionTypeStr == "KeyUp";
                bool isMouse = actionTypeStr.StartsWith("Mouse");

                // Suspender layout para evitar reordenamientos automáticos
                ruleEditorPanel.SuspendLayout();

                // Key - solo visible para teclado
                lblKey.Visible = isKeyboard;
                txtKey.Visible = isKeyboard;

                // Zone - solo visible para ratón
                lblZone.Visible = isMouse;
                zonePanel.Visible = isMouse;
                btnZone.Enabled = isMouse;

                // Reordenar controles para asegurar el orden correcto cuando son visibles
                // Orden deseado de arriba hacia abajo: Delay, Action, [Key/Zone condicional]
                // Con DockStyle.Top, BringToFront() mueve el control ARRIBA visualmente
                // Entonces traemos al frente en orden INVERSO: primero los fijos, luego los condicionales
                
                // Primero asegurar orden de controles siempre visibles (más arriba)
                lblDelay.BringToFront();
                txtDelay.BringToFront();
                lblActionType.BringToFront();
                cmbActionType.BringToFront();
                
                // Luego los condicionales (si son visibles) van DESPUÉS de Action
                // Para cada par: primero el label, luego el control (label arriba del control)
                if (isMouse)
                {
                    lblZone.BringToFront();
                    zonePanel.BringToFront();
                }
                if (isKeyboard)
                {
                    lblKey.BringToFront();
                    txtKey.BringToFront();
                }

                ruleEditorPanel.ResumeLayout(true);
            }

            // Guardar referencia a la función de visibilidad para poder llamarla desde fuera
            Action updateVisibility = UpdateEditorVisibility;

            // Evento cuando cambie el tipo de acción
            cmbActionType.SelectedIndexChanged += (s, e) => {
                if (!_suppressEditorEvents)
                {
                    UpdateEditorVisibility();
                    SaveActionChanges();
                }
            };

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
                Font = _model.CreateFont(10, FontStyle.Bold),
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
                Font = _model.CreateFont(10, FontStyle.Bold),
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
                Font = _model.CreateFont(10, FontStyle.Bold),
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
                Font = _model.CreateFont(10, FontStyle.Bold),
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
                Font = _model.CreateFont(10, FontStyle.Bold),
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
                Font = _model.CreateFont(10, FontStyle.Bold),
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
            _lblKey = lblKey;
            _lblZone = lblZone;
            _lblDelay = lblDelay;
            _lblActionType = lblActionType;

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
                
                // Actualizar coordenadas
                bool isMouse = action.Type.ToString().StartsWith("Mouse");
                _lblCoords.Text = isMouse ? $"📍 Coords: {action.X}, {action.Y}" : "📍 Coords: -";
                
                // Actualizar visibilidad según el tipo de acción
                // Nota: UpdateEditorVisibility se llama automáticamente por el evento SelectedIndexChanged
                // pero también lo llamamos explícitamente aquí para asegurar que se actualice correctamente
                string actionTypeStr = action.Type.ToString();
                bool isKeyboard = actionTypeStr == "KeyPress" || actionTypeStr == "KeyDown" || actionTypeStr == "KeyUp";
                
                // Actualizar visibilidad manualmente
                _lblKey.Visible = isKeyboard;
                _txtKey.Visible = isKeyboard;
                
                _lblZone.Visible = isMouse;
                _zonePanel.Visible = isMouse;
                _btnZone.Enabled = isMouse;
                
                // Reordenar controles para asegurar el orden correcto
                Panel ruleEditorPanel = _zonePanel.Parent as Panel;
                if (ruleEditorPanel != null)
                {
                    ruleEditorPanel.SuspendLayout();
                    
                    // Primero asegurar orden de controles siempre visibles (más arriba)
                    _lblDelay?.BringToFront();
                    _txtDelay?.BringToFront();
                    _lblActionType?.BringToFront();
                    _cmbActionType?.BringToFront();
                    
                    // Luego los condicionales (si son visibles) van DESPUÉS de Action
                    if (isMouse)
                    {
                        _lblZone?.BringToFront();
                        _zonePanel?.BringToFront();
                    }
                    if (isKeyboard)
                    {
                        _lblKey?.BringToFront();
                        _txtKey?.BringToFront();
                    }
                    
                    ruleEditorPanel.ResumeLayout(true);
                }
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
                _lblCoords.Text = isMouse ? $"📍 Coords: {action.X}, {action.Y}" : "📍 Coords: -";
                SetDirty(true);
                // La visibilidad se actualiza automáticamente por el evento SelectedIndexChanged
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


