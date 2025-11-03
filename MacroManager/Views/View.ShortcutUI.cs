using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    public partial class View
    {
        private void CreateShortcutRuleEditorWithButtons(Control parent)
        {
            // Similar to CreateRuleEditorWithButtons but for shortcuts
            Panel ruleEditorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = _model.PanelBackColor
            };

            // 1. Shortcut - Label
            Label lblHotkey = new Label
            {
                Text = "⌨️ Shortcut:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Margin = new Padding(0, 0, 0, 0)
            };
            
            // 2. Shortcut - TextBox
            _txtHotkeyShortcut = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(10),
                Name = "txtHotkeyShortcut",
                ReadOnly = true,
                TabStop = false,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };
            if (_toolTip != null) _toolTip.SetToolTip(_txtHotkeyShortcut, "Click para configurar atajo");
            
            // Captura de combinación: al enfocar el textbox, escuchamos KeyDown del formulario para permitir Ctrl+... etc.
            bool capturing = false;
            _txtHotkeyShortcut.MouseDown += (s, e) => { capturing = true; _txtHotkeyShortcut.Text = "Press keys..."; ruleEditorPanel.Focus(); };
            _mainForm.KeyDown += (s, e) =>
            {
                if (!capturing) return;
                if (e.KeyCode == Keys.Escape)
                {
                    capturing = false;
                    // Restaurar el hotkey actual del shortcut si existe, sino dejarlo vacío
                    if (_model.CurrentShortcut != null && !string.IsNullOrEmpty(_model.CurrentShortcut.Hotkey))
                    {
                        _txtHotkeyShortcut.Text = _model.CurrentShortcut.Hotkey;
                    }
                    else
                    {
                        _txtHotkeyShortcut.Text = "Ctrl+T";
                    }
                    ruleEditorPanel.Focus();
                    e.Handled = true; return;
                }
                // Ignora puras teclas modificadoras
                if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.Menu || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
                {
                    e.Handled = true; return;
                }
                bool ctrl = e.Control; bool alt = e.Alt; bool shift = e.Shift; bool win = (Control.ModifierKeys & (Keys.LWin | Keys.RWin)) != 0;
                Keys key = e.KeyCode;
                string text = $"{(ctrl?"Ctrl+":"")}{(alt?"Alt+":"")}{(shift?"Shift+":"")}{(win?"Win+":"")}{key}";
                _txtHotkeyShortcut.Text = text;
                capturing = false;
                ruleEditorPanel.Focus();

                // Guardar el hotkey en el shortcut actual
                if (_model.CurrentShortcut != null)
                {
                    _model.CurrentShortcut.Hotkey = text;
                    SetDirty(true);
                }

                // Registrar el hotkey globalmente (solo si estamos en la pestaña de shortcuts)
                TabControl tabControl = FindTabControl(_mainForm);
                if (tabControl != null && tabControl.SelectedTab != null && tabControl.SelectedTab.Text == "SHORTCUTS")
                {
                    bool ok = (_mainForm as MainForm).RegisterGlobalHotKey(ctrl, alt, shift, win, key);
                    if (!ok && _toolTip != null)
                    {
                        _toolTip.SetToolTip(_txtHotkeyShortcut, "No se pudo registrar. Prueba otra combinación.");
                    }
                    else if (ok)
                    {
                        // Re-registrar el handler para asegurar que está conectado
                        MainForm mainForm = _mainForm as MainForm;
                        if (mainForm != null)
                        {
                            if (_shortcutHotkeyHandler != null)
                            {
                                mainForm.GlobalHotKeyPressed -= _shortcutHotkeyHandler;
                                mainForm.GlobalHotKeyPressed += _shortcutHotkeyHandler;
                            }
                        }
                    }
                }
                e.Handled = true;
            };
            // Inicializar con el hotkey del shortcut actual o el valor por defecto
            if (_model.CurrentShortcut != null && !string.IsNullOrEmpty(_model.CurrentShortcut.Hotkey))
            {
                _txtHotkeyShortcut.Text = _model.CurrentShortcut.Hotkey;
            }
            else
            {
                _txtHotkeyShortcut.Text = "Ctrl+T";
                // Establecer el valor por defecto si no hay shortcut cargado
                if (_model.CurrentShortcut != null)
                {
                    _model.CurrentShortcut.Hotkey = "Ctrl+T";
                }
            }

            // Guardar referencia al handler (el registro se hace en OnTabChanged)
            // El handler verifica si el shortcut está habilitado antes de ejecutar
            _shortcutHotkeyHandler = async (s, e) => {
                if (_model.CurrentShortcut != null && _model.CurrentShortcut.IsEnabled)
                {
                    await _controller.PlayShortcutDirectly();
                }
            };

            // 3. Action - Label
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

            // 4. Action - Desplegable selector
            ComboBox cmbActionType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(9),
                Name = "cmbActionTypeShortcut",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 0)
            };
            cmbActionType.Items.AddRange(new[] { "KeyPress", "KeyDown", "KeyUp", "MouseLeftDown", "MouseLeftUp", "MouseLeftClick", "MouseRightDown", "MouseRightUp", "MouseRightClick", "MouseMove", "Delay", "Macro" });
            cmbActionType.SelectedIndex = 0;

            // 5. Delay - Label
            Label lblDelay = new Label
            {
                Text = "⏱️ Delay:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Margin = new Padding(0, 10, 0, 0) // Espacio arriba
            };

            // 6. Delay - TextBox
            TextBox txtDelay = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(10),
                Name = "txtDelayShortcut",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 0)
            };
            txtDelay.TextChanged += (s, e) => SaveShortcutActionChanges();

            // 7. Key/Zone/Macro - Label (condicional) - INVISIBLE por defecto
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

            Label lblMacro = new Label
            {
                Text = "📜 Macro:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = _model.CreateFont(9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Visible = false, // INVISIBLE por defecto
                Margin = new Padding(0, 10, 0, 0) // Espacio arriba
            };

            // 8. Key/Zone/Macro - Cuadro de texto/control (condicional)
            // Key - TextBox
            TextBox txtKey = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(10),
                Name = "txtKeyShortcut",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };
            txtKey.TextChanged += (s, e) => SaveShortcutActionChanges();
            txtKey.KeyDown += (s, e) => {
                _suppressShortcutEditorEvents = true;
                try
                {
                    _txtKeyShortcut.Text = e.KeyCode.ToString();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                finally
                {
                    _suppressShortcutEditorEvents = false;
                }
                SaveShortcutActionChanges();
            };

            // Zone - Panel con botón y coordenadas - INVISIBLE por defecto
            Panel zonePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = _model.PanelBackColor,
                Visible = false, // INVISIBLE por defecto
                Margin = new Padding(0, 0, 0, 0)
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

            btnZone.Click += (s, e) => PickZoneForCurrentShortcutAction(lblCoords);
            if (_toolTip != null) _toolTip.SetToolTip(btnZone, "Elegir punto en pantalla");

            zonePanel.Controls.Add(lblCoords);
            zonePanel.Controls.Add(btnZone);

            // Macro - ComboBox - INVISIBLE por defecto
            ComboBox cmbMacro = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = _model.CreateFont(9),
                Name = "cmbMacroShortcut",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Visible = false, // INVISIBLE por defecto
                Margin = new Padding(0, 0, 0, 0)
            };
            
            // Cargar macros disponibles
            RefreshMacroSelector(cmbMacro);
            
            // Actualizar la lista de macros cuando se abre el desplegable
            cmbMacro.DropDown += (s, e) => RefreshMacroSelector(cmbMacro);
            
            cmbMacro.SelectedIndexChanged += (s, e) => SaveShortcutActionChanges();
            
            // Agregar controles desde el ÚLTIMO visible hacia el PRIMERO
            // Con DockStyle.Top: último agregado = más arriba visualmente
            
            // Orden deseado: Shortcut, Delay, Action, [Key/Zone/Macro condicional]
            // Agregar en orden inverso:
            
            // PRIMERO: Condicionales (abajo, invisibles por defecto)
            // Orden invertido para los condicionales
            ruleEditorPanel.Controls.Add(txtKey);          // Key textbox
            ruleEditorPanel.Controls.Add(lblKey);         // Key label (aparece arriba del textbox)
            
            ruleEditorPanel.Controls.Add(zonePanel);       // Zone panel
            ruleEditorPanel.Controls.Add(lblZone);        // Zone label (aparece arriba del panel)
            
            ruleEditorPanel.Controls.Add(cmbMacro);        // Macro selector
            ruleEditorPanel.Controls.Add(lblMacro);       // Macro label (aparece arriba del selector)
            
            // SEGUNDO: Action
            ruleEditorPanel.Controls.Add(cmbActionType);   // Action selector
            ruleEditorPanel.Controls.Add(lblActionType);  // Action label (aparece arriba del selector)
            
            // TERCERO: Delay
            ruleEditorPanel.Controls.Add(txtDelay);        // Delay textbox
            ruleEditorPanel.Controls.Add(lblDelay);       // Delay label (aparece arriba del textbox)
            
            // CUARTO: Shortcut (más arriba, siempre visible)
            ruleEditorPanel.Controls.Add(_txtHotkeyShortcut);       // Shortcut textbox
            ruleEditorPanel.Controls.Add(lblHotkey);      // Shortcut label (aparece más arriba)

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
            btnAdd.Click += (s, e) => { _controller.AddNewShortcutAction(); SetDirty(true); };
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
                if (_selectedShortcutActionIndices.Count > 1)
                {
                    _controller.DeleteShortcutActions(_selectedShortcutActionIndices);
                }
                else
                {
                    _controller.DeleteShortcutAction(_selectedShortcutActionIndex);
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
                if (_selectedShortcutActionIndices.Count > 1)
                {
                    _controller.DuplicateShortcutActions(_selectedShortcutActionIndices);
                }
                else
                {
                    _controller.DuplicateShortcutAction(_selectedShortcutActionIndex);
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

            _btnRecordShortcut = new Button
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
            _btnRecordShortcut.FlatAppearance.BorderSize = 0;
            _btnRecordShortcut.Click += (s, e) => _controller.StartRecordingShortcut();
            ApplyRetroButtonStyle(_btnRecordShortcut, Color.FromArgb(150, 30, 30), _model.BorderColor);
            recordingRow.Controls.Add(_btnRecordShortcut);
            if (_toolTip != null) _toolTip.SetToolTip(_btnRecordShortcut, "Iniciar grabación");

            _btnStopRecordShortcut = new Button
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
            _btnStopRecordShortcut.FlatAppearance.BorderSize = 0;
            _btnStopRecordShortcut.Click += (s, e) => _controller.StopRecordingShortcut();
            ApplyRetroButtonStyle(_btnStopRecordShortcut, Color.FromArgb(60, 60, 60), _model.BorderColor);
            recordingRow.Controls.Add(_btnStopRecordShortcut);
            if (_toolTip != null) _toolTip.SetToolTip(_btnStopRecordShortcut, "Detener grabación");

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
            btnSave.Click += (s, e) => { _controller.SaveCurrentShortcut(); SetDirty(false); };
            ApplyRetroButtonStyle(btnSave, _model.AccentColor, _model.BorderColor);
            recordingRow.Controls.Add(btnSave);
            if (_toolTip != null) _toolTip.SetToolTip(btnSave, "Guardar shortcut");

            buttonPanel.Controls.Add(recordingRow);

            ruleEditorPanel.Controls.Add(buttonPanel);

            // Función para actualizar visibilidad según el tipo de acción
            void UpdateShortcutEditorVisibility()
            {
                if (cmbActionType.SelectedItem == null) return;
                
                string actionTypeStr = cmbActionType.SelectedItem.ToString();
                bool isKeyboard = actionTypeStr == "KeyPress" || actionTypeStr == "KeyDown" || actionTypeStr == "KeyUp";
                bool isMouse = actionTypeStr.StartsWith("Mouse");
                bool isMacro = actionTypeStr == "Macro";

                // Suspender layout para evitar reordenamientos automáticos
                ruleEditorPanel.SuspendLayout();

                // Key - solo visible para teclado
                lblKey.Visible = isKeyboard;
                txtKey.Visible = isKeyboard;

                // Zone - solo visible para ratón
                lblZone.Visible = isMouse;
                zonePanel.Visible = isMouse;
                btnZone.Enabled = isMouse;

                // Macro - solo visible para tipo Macro
                lblMacro.Visible = isMacro;
                cmbMacro.Visible = isMacro;

                // Reordenar controles para asegurar el orden correcto cuando son visibles
                // Orden deseado de arriba hacia abajo: Shortcut, Delay, Action, [Key/Zone/Macro condicional]
                // Con DockStyle.Top, BringToFront() mueve el control ARRIBA visualmente
                // Entonces traemos al frente en orden INVERSO: primero los fijos, luego los condicionales
                
                // Primero asegurar orden de controles siempre visibles (más arriba)
                lblHotkey.BringToFront();
                _txtHotkeyShortcut.BringToFront();
                lblDelay.BringToFront();
                txtDelay.BringToFront();
                lblActionType.BringToFront();
                cmbActionType.BringToFront();
                
                // Luego los condicionales (si son visibles) van DESPUÉS de Action
                // Para cada par: primero el label, luego el control (label arriba del control)
                if (isMacro)
                {
                    lblMacro.BringToFront();
                    cmbMacro.BringToFront();
                }
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

            // Evento cuando cambie el tipo de acción
            cmbActionType.SelectedIndexChanged += (s, e) => {
                if (!_suppressShortcutEditorEvents)
                {
                    UpdateShortcutEditorVisibility();
                    SaveShortcutActionChanges();
                }
            };

            _txtKeyShortcut = txtKey;
            _txtDelayShortcut = txtDelay;
            _cmbActionTypeShortcut = cmbActionType;
            _cmbMacroShortcut = cmbMacro;

            // Guardar referencias para Zone y otros controles
            _zonePanelShortcut = zonePanel;
            _btnZoneShortcut = btnZone;
            _lblCoordsShortcut = lblCoords;

            parent.Controls.Add(ruleEditorPanel);
        }

        private void LoadShortcutActionToEditor(int actionIndex)
        {
            if (_model.CurrentShortcut == null || actionIndex < 0 || actionIndex >= _model.CurrentShortcut.Actions.Count)
                return;

            _selectedShortcutActionIndex = actionIndex;
            var action = _model.CurrentShortcut.Actions[actionIndex];

            _suppressShortcutEditorEvents = true;
            try
            {
                _txtDelayShortcut.Text = action.DelayMs.ToString();
                _cmbActionTypeShortcut.SelectedItem = action.Type.ToString();
                
                // Actualizar visibilidad primero
                UpdateShortcutEditorVisibilityForCurrentAction();

                // Cargar valores según el tipo
                if (action.Type == ActionType.Macro && action.MacroId.HasValue)
                {
                    RefreshMacroSelector(_cmbMacroShortcut);
                    for (int i = 0; i < _cmbMacroShortcut.Items.Count; i++)
                    {
                        if (_cmbMacroShortcut.Items[i] is MacroConfig macro && macro.Id == action.MacroId.Value)
                        {
                            _cmbMacroShortcut.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else if (action.Type == ActionType.KeyPress || action.Type == ActionType.KeyDown || action.Type == ActionType.KeyUp)
                {
                    _txtKeyShortcut.Text = _model.GetKeyDisplay(action);
                }
                else if (action.Type.ToString().StartsWith("Mouse"))
                {
                    _lblCoordsShortcut.Text = $"📍 Coords: {action.X}, {action.Y}";
                }
            }
            finally
            {
                _suppressShortcutEditorEvents = false;
            }
        }
        
        private void RefreshMacroSelector(ComboBox cmbMacro)
        {
            if (cmbMacro == null) return;
            
            cmbMacro.Items.Clear();
            foreach (var macro in _model.LoadedMacros)
            {
                cmbMacro.Items.Add(macro);
            }
            
            cmbMacro.DisplayMember = "Name";
        }
        
        private void UpdateShortcutEditorVisibilityForCurrentAction()
        {
            if (_cmbActionTypeShortcut?.SelectedItem == null) return;
            
            string actionTypeStr = _cmbActionTypeShortcut.SelectedItem.ToString();
            bool isKeyboard = actionTypeStr == "KeyPress" || actionTypeStr == "KeyDown" || actionTypeStr == "KeyUp";
            bool isMouse = actionTypeStr.StartsWith("Mouse");
            bool isMacro = actionTypeStr == "Macro";

            // Buscar controles por nombre
            foreach (Control control in _cmbActionTypeShortcut.Parent.Controls)
            {
                if (control.Name == "txtKeyShortcut")
                {
                    control.Visible = isKeyboard;
                }
                else if (control is Label lblKeyCheck && lblKeyCheck.Text.Contains("⌨️ Key"))
                {
                    lblKeyCheck.Visible = isKeyboard;
                }
                else if (control is Label lblZoneCheck && lblZoneCheck.Text.Contains("📍 Zone"))
                {
                    lblZoneCheck.Visible = isMouse;
                }
                else if (control is Panel panel && panel.Controls.Contains(_btnZoneShortcut))
                {
                    panel.Visible = isMouse;
                }
                else if (control.Name == "cmbMacroShortcut")
                {
                    control.Visible = isMacro;
                }
                else if (control is Label lblMacroCheck && lblMacroCheck.Text.Contains("📜 Macro"))
                {
                    lblMacroCheck.Visible = isMacro;
                }
            }

            // También actualizar directamente las referencias si están disponibles
            if (_zonePanelShortcut != null)
            {
                _zonePanelShortcut.Visible = isMouse;
                if (_btnZoneShortcut != null)
                    _btnZoneShortcut.Enabled = isMouse;
            }
            
            // Buscar y actualizar el label de Zone
            foreach (Control control in _cmbActionTypeShortcut.Parent.Controls)
            {
                if (control is Label lblZoneLabel && lblZoneLabel.Text.Contains("📍 Zone:"))
                {
                    lblZoneLabel.Visible = isMouse;
                    break;
                }
            }
        }

        private void SaveShortcutActionChanges()
        {
            if (_suppressShortcutEditorEvents) return;
            if (_selectedShortcutActionIndex < 0 || _model.CurrentShortcut == null || _selectedShortcutActionIndex >= _model.CurrentShortcut.Actions.Count)
                return;

            if (int.TryParse(_txtDelayShortcut.Text, out int delay))
            {
                ActionType actionType = ActionType.KeyPress;
                if (_cmbActionTypeShortcut.SelectedItem != null)
                {
                    Enum.TryParse<ActionType>(_cmbActionTypeShortcut.SelectedItem.ToString(), out actionType);
                }

                Guid? macroId = null;
                if (actionType == ActionType.Macro && _cmbMacroShortcut?.SelectedItem is MacroConfig selectedMacro)
                {
                    macroId = selectedMacro.Id;
                }

                _controller.UpdateShortcutAction(_selectedShortcutActionIndex, _txtKeyShortcut.Text, delay, actionType, macroId);
                
                // Actualizar coordenadas para acciones de ratón
                var action = _model.CurrentShortcut.Actions[_selectedShortcutActionIndex];
                if (actionType.ToString().StartsWith("Mouse") && _lblCoordsShortcut != null)
                {
                    _lblCoordsShortcut.Text = $"📍 Coords: {action.X}, {action.Y}";
                }
                
                SetDirty(true);
            }
        }

        private void PickZoneForCurrentShortcutAction(Label targetLabel)
        {
            if (_selectedShortcutActionIndex < 0 || _model.CurrentShortcut == null || _selectedShortcutActionIndex >= _model.CurrentShortcut.Actions.Count)
                return;

            var action = _model.CurrentShortcut.Actions[_selectedShortcutActionIndex];
            if (!action.Type.ToString().StartsWith("Mouse")) return;

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
                        RefreshShortcutActionsDisplay();
                    }
                }
            }
            finally
            {
                main.TopMost = wasTopMost;
            }
        }

        private void CreateShortcutTextEditorWithSwitch(Control parent)
        {
            _shortcutActionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _model.CardBackColor,
                Padding = new Padding(10),
                AutoScroll = true
            };
            _shortcutActionsPanel.MouseDown += OnShortcutActionsPanelMouseDown;
            _shortcutActionsPanel.MouseMove += OnShortcutActionsPanelMouseMove;
            _shortcutActionsPanel.MouseUp += OnShortcutActionsPanelMouseUp;
            _shortcutActionsPanel.Paint += OnShortcutActionsPanelPaint;
            _shortcutActionsPanel.Resize += (s, e) => AdjustShortcutActionButtonsWidth();
            parent.Controls.Add(_shortcutActionsPanel);
        }

        private void RefreshShortcutActionsDisplay()
        {
            if (_shortcutActionsPanel == null || _model.CurrentShortcut == null) return;

            _shortcutActionsPanel.Controls.Clear();

            int yPosition = 10;
            int buttonHeight = 70;
            int buttonSpacing = 15;

            for (int i = 0; i < _model.CurrentShortcut.Actions.Count; i++)
            {
                var action = _model.CurrentShortcut.Actions[i];
                var actionButton = CreateShortcutActionButton(action, i, yPosition);
                _shortcutActionsPanel.Controls.Add(actionButton);
                yPosition += buttonHeight + buttonSpacing;
            }

            _shortcutActionsPanel.Height = Math.Max(_shortcutActionsPanel.Parent.Height, yPosition + 20);
            AdjustShortcutActionButtonsWidth();
            UpdateShortcutSelectionHighlight();
        }

        private void AdjustShortcutActionButtonsWidth()
        {
            if (_shortcutActionsPanel == null) return;
            int usableWidth = Math.Max(0, _shortcutActionsPanel.ClientSize.Width - 20);
            foreach (Control control in _shortcutActionsPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.Width = usableWidth;
                }
            }
        }

        private Button CreateShortcutActionButton(MacroAction action, int index, int yPosition)
        {
            string keyDisplay = _model.GetKeyDisplay(action);
            string actionType = _model.GetActionTypeDisplay(action.Type);
            string coords = (action.Type.ToString().StartsWith("Mouse")) ? $"\nPos: {action.X},{action.Y}" : string.Empty;
            
            var button = new Button
            {
                Text = $"#{index + 1}  {actionType}\nKey: {keyDisplay}\nDelay: {action.DelayMs}ms{coords}",
                Location = new Point(10, yPosition),
                Size = new Size(Math.Max(0, _shortcutActionsPanel.ClientSize.Width - 20), 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Font = _model.CreateFont(10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = index,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = _model.AccentColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);

            button.MouseDown += (s, e) => {
                _selectedShortcutActionIndex = index;
                _selectedShortcutActionIndices.Clear();
                _selectedShortcutActionIndices.Add(index);
                UpdateShortcutSelectionHighlight();
                LoadShortcutActionToEditor(index);
            };

            return button;
        }

        private void UpdateShortcutSelectionHighlight()
        {
            foreach (Control control in _shortcutActionsPanel.Controls)
            {
                if (control is Button button && button.Tag is int index)
                {
                    if (_selectedShortcutActionIndices.Contains(index))
                    {
                        button.BackColor = _model.AccentColor;
                        button.ForeColor = Color.White;
                    }
                    else
                    {
                        button.BackColor = _model.CardBackColor;
                        button.ForeColor = _model.PanelForeColor;
                    }
                }
            }
            _shortcutActionsPanel.Invalidate();
        }

        private Panel CreateShortcutPlaybackPanelControl()
        {
            // Solo muestra un checkbox de Enable
            Panel playbackPanel = new Panel
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(8),
                Height = 50
            };

            Panel controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _model.PanelBackColor
            };

            // Checkbox Enable
            _chkEnableShortcut = new CheckBox
            {
                Text = "✓ Enable",
                AutoSize = true,
                Font = _model.CreateFont(10, FontStyle.Bold),
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Location = new Point(10, 12)
            };
            // Inicializar con el estado del shortcut actual (si existe)
            _chkEnableShortcut.Checked = _model.CurrentShortcut?.IsEnabled ?? true;
            if (_toolTip != null) _toolTip.SetToolTip(_chkEnableShortcut, "Habilitar/deshabilitar este shortcut");
            
            // Conectar el evento CheckedChanged para habilitar/deshabilitar el shortcut
            _chkEnableShortcut.CheckedChanged += (s, e) => {
                if (_model.CurrentShortcut != null)
                {
                    _model.CurrentShortcut.IsEnabled = _chkEnableShortcut.Checked;
                    SetDirty(true);
                }
            };

            void CenterChildren()
            {
                int centerX = Math.Max(0, (controlsPanel.Width - _chkEnableShortcut.Width) / 2);
                int centerY = Math.Max(0, (controlsPanel.Height - _chkEnableShortcut.Height) / 2);
                _chkEnableShortcut.Location = new Point(centerX, centerY);
            }

            controlsPanel.Controls.Add(_chkEnableShortcut);
            controlsPanel.Resize += (s, e) => CenterChildren();
            playbackPanel.Resize += (s, e) => CenterChildren();
            _mainForm.Load += (s, e) => CenterChildren();

            playbackPanel.Controls.Add(controlsPanel);

            return playbackPanel;
        }

        private async Task TogglePlayPauseStopShortcut()
        {
            int repeatCount = 1;
            await _controller.TogglePlayPauseStopShortcut(repeatCount);
        }

        private void OnShortcutActionsPanelMouseDown(object sender, MouseEventArgs e) { }
        private void OnShortcutActionsPanelMouseMove(object sender, MouseEventArgs e) { }
        private void OnShortcutActionsPanelMouseUp(object sender, MouseEventArgs e) { }
        private void OnShortcutActionsPanelPaint(object sender, PaintEventArgs e) { }
    }
}
