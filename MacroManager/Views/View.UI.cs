using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace MacroManager
{
    public partial class View
    {
        // UI Setup
        private void SetupUI()
        {
            _mainForm.AutoScaleMode = AutoScaleMode.Font;
            _mainForm.MinimumSize = new Size(_model.MinWindowWidth, _model.MinWindowHeight);
            _mainForm.Size = new Size(_model.DefaultWindowWidth, _model.DefaultWindowHeight);
            _mainForm.KeyPreview = true;
            _mainForm.FormClosing += OnMainFormClosing;

            // Top menu disabled for a cleaner appearance

            Panel mainContentPanel = new Panel
            {
                Name = "mainContentPanel",
                BackColor = _model.PanelBackColor
            };
            _mainForm.Controls.Add(mainContentPanel);

            // Note: we removed the visual banner; the unsaved state will be indicated in the title

            _mainForm.Load += (s, e) => {
                int menuHeight = 0;
                mainContentPanel.Location = new Point(0, menuHeight);
                mainContentPanel.Size = new Size(_mainForm.ClientSize.Width, _mainForm.ClientSize.Height - menuHeight);
            };
            
            _mainForm.Resize += (s, e) => {
                if (mainContentPanel != null) {
                    int menuHeight = 0;
                    mainContentPanel.Size = new Size(_mainForm.ClientSize.Width, _mainForm.ClientSize.Height - menuHeight);
                }
            };

            // Center: warning icon (only when there are unsaved changes)
            _unsavedCenterIcon = new Label
            {
                AutoSize = true,
                Text = "⚠",
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                Font = _model.CreateFont(16, FontStyle.Bold),
                Visible = false
            };
            mainContentPanel.Controls.Add(_unsavedCenterIcon);

            mainContentPanel.Resize += (s, e) => PositionUnsavedIcon();

            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "mainTabControl",
                BackColor = _model.PanelBackColor,
                ForeColor = _model.PanelForeColor,
                Font = _model.CreateFont(10, FontStyle.Regular)
            };
            SetupCustomTabControl(tabControl);
            tabControl.Parent = mainContentPanel;

            CreateMainMacrosTab(tabControl);
            CreateShortcutsTab(tabControl);
            CreateMouseTab(tabControl);

            // Registrar el hotkey de la pestaña activa cuando cambia la pestaña
            tabControl.SelectedIndexChanged += (s, e) => OnTabChanged(tabControl);

            _mainForm.KeyDown += OnKeyDown;
            
            // Registrar el hotkey inicial DESPUÉS de que todas las pestañas estén creadas
            // y después de que el formulario esté completamente cargado
            // (esto asegura que los handlers estén asignados y los controles inicializados)
            _mainForm.Shown += (s, e) => {
                // Usar BeginInvoke para asegurar que todo esté completamente renderizado
                _mainForm.BeginInvoke(new Action(() => {
                    OnTabChanged(tabControl);
                }));
            };
        }

        private void CreateMainMacrosTab(TabControl tabControl)
        {
            TabPage mainTab = new TabPage("MACROS")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(5)
            };

            SplitContainer horizontalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "horizontalSplit",
                BackColor = _model.PanelBackColor
            };
            mainTab.Controls.Add(horizontalSplit);

            CreateMacroTree(horizontalSplit.Panel1);

            Panel rightContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "rightContainer",
                BackColor = _model.PanelBackColor
            };
            horizontalSplit.Panel2.Controls.Add(rightContainer);

            SplitContainer verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "verticalSplit",
                BackColor = _model.PanelBackColor
            };
            rightContainer.Controls.Add(verticalSplit);

            CreateTextEditorWithSwitch(verticalSplit.Panel1);
            CreateRuleEditorWithButtons(verticalSplit.Panel2);

            Panel playbackPanel = CreatePlaybackPanelControl();
            playbackPanel.Dock = DockStyle.Bottom;
            playbackPanel.Height = Math.Max(_model.PlaybackPanelHeight, 120);
            verticalSplit.Panel1.Controls.Add(playbackPanel);

            _mainForm.Load += (s, e) => {
                try
                {
                    if (horizontalSplit.Width > 0 && horizontalSplit.IsHandleCreated)
                    {
                        int calculatedDistance = Math.Max(_model.MinimumTreeViewWidth, (int)(horizontalSplit.Width * _model.TreeViewPercentage));
                        int minDistance = horizontalSplit.Panel1MinSize;
                        int maxDistance = horizontalSplit.Width - horizontalSplit.Panel2MinSize - horizontalSplit.SplitterWidth;
                        int finalDistance = Math.Max(minDistance, Math.Min(calculatedDistance, maxDistance));
                        
                        // Validar que el valor esté dentro de los límites antes de asignar
                        if (finalDistance >= minDistance && finalDistance <= maxDistance && maxDistance > minDistance)
                        {
                            horizontalSplit.SplitterDistance = finalDistance;
                        }
                    }
                }
                catch { /* Ignore if still not ready */ }
                
                try
                {
                    if (verticalSplit.Width > 0 && verticalSplit.IsHandleCreated)
                    {
                        int calculatedDistance = Math.Max(_model.MinimumEditorWidth, (int)(verticalSplit.Width * _model.EditorPercentage));
                        int minDistance = verticalSplit.Panel1MinSize;
                        int maxDistance = verticalSplit.Width - verticalSplit.Panel2MinSize - verticalSplit.SplitterWidth;
                        int finalDistance = Math.Max(minDistance, Math.Min(calculatedDistance, maxDistance));
                        
                        // Validar que el valor esté dentro de los límites antes de asignar
                        if (finalDistance >= minDistance && finalDistance <= maxDistance && maxDistance > minDistance)
                        {
                            verticalSplit.SplitterDistance = finalDistance;
                        }
                    }
                }
                catch { /* Ignore if still not ready */ }
                // Ensure the actions panel draws with correct sizes after initial layout
                RefreshActionsDisplay();
                PositionUnsavedIcon();
            };

            tabControl.TabPages.Add(mainTab);
        }

        private void CreateShortcutsTab(TabControl tabControl)
        {
            TabPage shortcutsTab = new TabPage("SHORTCUTS")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(5)
            };

            SplitContainer horizontalSplitShortcut = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "horizontalSplitShortcut",
                BackColor = _model.PanelBackColor
            };
            shortcutsTab.Controls.Add(horizontalSplitShortcut);

            CreateShortcutTree(horizontalSplitShortcut.Panel1);

            Panel rightContainerShortcut = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "rightContainerShortcut",
                BackColor = _model.PanelBackColor
            };
            horizontalSplitShortcut.Panel2.Controls.Add(rightContainerShortcut);

            SplitContainer verticalSplitShortcut = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "verticalSplitShortcut",
                BackColor = _model.PanelBackColor
            };
            rightContainerShortcut.Controls.Add(verticalSplitShortcut);

            CreateShortcutTextEditorWithSwitch(verticalSplitShortcut.Panel1);
            CreateShortcutRuleEditorWithButtons(verticalSplitShortcut.Panel2);

            Panel playbackPanelShortcut = CreateShortcutPlaybackPanelControl();
            playbackPanelShortcut.Dock = DockStyle.Bottom;
            playbackPanelShortcut.Height = Math.Max(_model.PlaybackPanelHeight, 120);
            verticalSplitShortcut.Panel1.Controls.Add(playbackPanelShortcut);

            void SetShortcutSplitterDistances()
            {
                // Usa exactamente la misma lógica que la pestaña de macros
                try
                {
                    if (horizontalSplitShortcut.Width > 0 && horizontalSplitShortcut.IsHandleCreated && horizontalSplitShortcut.Visible)
                    {
                        int splitterWidth = horizontalSplitShortcut.SplitterWidth;
                        int minPanel1 = Math.Max(horizontalSplitShortcut.Panel1MinSize, 25); // Mínimo razonable
                        int minPanel2 = Math.Max(horizontalSplitShortcut.Panel2MinSize, 25);
                        int availableWidth = horizontalSplitShortcut.Width - splitterWidth;
                        
                        int calculatedDistance = Math.Max(_model.MinimumTreeViewWidth, (int)(horizontalSplitShortcut.Width * _model.TreeViewPercentage));
                        int minDistance = minPanel1;
                        int maxDistance = Math.Max(minDistance + 10, availableWidth - minPanel2);
                        
                        if (maxDistance <= minDistance) return;
                        
                        int finalDistance = Math.Max(minDistance, Math.Min(calculatedDistance, maxDistance));
                        
                        if (finalDistance >= minDistance && finalDistance <= maxDistance)
                        {
                            horizontalSplitShortcut.SplitterDistance = finalDistance;
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"Error setting horizontalSplitShortcut: {ex.Message}");
                }
                
                try
                {
                    if (verticalSplitShortcut.Width > 0 && verticalSplitShortcut.IsHandleCreated && verticalSplitShortcut.Visible)
                    {
                        int splitterWidth = verticalSplitShortcut.SplitterWidth;
                        int minPanel1 = Math.Max(verticalSplitShortcut.Panel1MinSize, 25);
                        int minPanel2 = Math.Max(verticalSplitShortcut.Panel2MinSize, 25);
                        int availableWidth = verticalSplitShortcut.Width - splitterWidth;
                        
                        int calculatedDistance = Math.Max(_model.MinimumEditorWidth, (int)(verticalSplitShortcut.Width * _model.EditorPercentage));
                        int minDistance = minPanel1;
                        int maxDistance = Math.Max(minDistance + 10, availableWidth - minPanel2);
                        
                        if (maxDistance <= minDistance) return;
                        
                        int finalDistance = Math.Max(minDistance, Math.Min(calculatedDistance, maxDistance));
                        
                        if (finalDistance >= minDistance && finalDistance <= maxDistance)
                        {
                            verticalSplitShortcut.SplitterDistance = finalDistance;
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"Error setting verticalSplitShortcut: {ex.Message}");
                }
            }

            // Usar el evento Shown en lugar de Load para asegurar que los controles estén completamente listos
            _mainForm.Shown += (s, e) => {
                SetShortcutSplitterDistances();
                RefreshShortcutActionsDisplay();
                PositionUnsavedIcon();
            };
            
            // También configurar cuando el tab se muestre
            shortcutsTab.VisibleChanged += (s, ev) => {
                if (shortcutsTab.Visible)
                {
                    _mainForm.BeginInvoke(new Action(() => {
                        SetShortcutSplitterDistances();
                    }));
                }
            };

            tabControl.TabPages.Add(shortcutsTab);
        }

        private void CreateMouseTab(TabControl tabControl)
        {
            TabPage mouseTab = new TabPage("MOUSE")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(20)
            };

            Label placeholderLabel = new Label
            {
                Text = "🖱️ Mouse\n\nThis tab will be available soon.\nHere you can configure specific mouse actions.",
                Dock = DockStyle.Fill,
                Font = _model.CreateFont(12, FontStyle.Regular),
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mouseTab.Controls.Add(placeholderLabel);

            tabControl.TabPages.Add(mouseTab);
        }

        private MenuStrip CreateMenuBar()
        {
            MenuStrip menu = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File");

            var miNew = new ToolStripMenuItem("&New Macro")
            {
                ShortcutKeys = Keys.Control | Keys.N,
                ShowShortcutKeys = true
            };
            miNew.Click += (s, e) => _controller.CreateNewMacro();
            fileMenu.DropDownItems.Add(miNew);

            var miRecord = new ToolStripMenuItem("&Record")
            {
                ShortcutKeys = Keys.Control | Keys.R,
                ShowShortcutKeys = true
            };
            miRecord.Click += (s, e) => _controller.StartRecording();
            fileMenu.DropDownItems.Add(miRecord);

            fileMenu.DropDownItems.Add("-");

            var miSave = new ToolStripMenuItem("&Save")
            {
                ShortcutKeys = Keys.Control | Keys.S,
                ShowShortcutKeys = true
            };
            miSave.Click += (s, e) => SaveAndClearDirty();
            fileMenu.DropDownItems.Add(miSave);
            fileMenu.DropDownItems.Add("&Delete (Del)", null, (s, e) => _controller.DeleteCurrentMacro());
            fileMenu.DropDownItems.Add("-");
            // Removed Export/Import options
            fileMenu.DropDownItems.Add("E&xit (Alt+F4)", null, (s, e) => _mainForm.Close());

            ToolStripMenuItem editMenu = new ToolStripMenuItem("&Edit");

            var miUndo = new ToolStripMenuItem("&Undo")
            {
                ShortcutKeys = Keys.Control | Keys.Z,
                ShowShortcutKeys = true
            };
            miUndo.Click += (s, e) => _controller.UndoAction();
            editMenu.DropDownItems.Add(miUndo);

            var miRedo = new ToolStripMenuItem("&Redo")
            {
                ShortcutKeys = Keys.Control | Keys.Y,
                ShowShortcutKeys = true
            };
            miRedo.Click += (s, e) => _controller.RedoAction();
            editMenu.DropDownItems.Add(miRedo);
            editMenu.DropDownItems.Add("-");
            editMenu.DropDownItems.Add(new ToolStripMenuItem("Cu&t") { ShortcutKeys = Keys.Control | Keys.X, ShowShortcutKeys = true, Tag = (Action)(() => _controller.CutAction()) });
            editMenu.DropDownItems.Add(new ToolStripMenuItem("&Copy") { ShortcutKeys = Keys.Control | Keys.C, ShowShortcutKeys = true, Tag = (Action)(() => _controller.CopyAction()) });
            editMenu.DropDownItems.Add(new ToolStripMenuItem("&Paste") { ShortcutKeys = Keys.Control | Keys.V, ShowShortcutKeys = true, Tag = (Action)(() => _controller.PasteAction()) });
            editMenu.DropDownItems.Add("-");
            var miSelectAll = new ToolStripMenuItem("&Select All")
            {
                ShortcutKeys = Keys.Control | Keys.A,
                ShowShortcutKeys = true
            };
            miSelectAll.Click += (s, e) => _controller.SelectAllAction();
            editMenu.DropDownItems.Add(miSelectAll);

            // Assign clicks for items with Tag Action
            foreach (ToolStripItem item in editMenu.DropDownItems)
            {
                if (item is ToolStripMenuItem tsmi && tsmi.Tag is Action act)
                {
                    tsmi.Click += (s, e) => act();
                }
            }

            menu.Items.Add(fileMenu);
            menu.Items.Add(editMenu);

            return menu;
        }

        private Panel CreatePlaybackPanelControl()
        {
            Panel playbackPanel = new Panel
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(8),
                Height = 60
            };

            Panel controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _model.PanelBackColor
            };

            _btnPlay = new Button
            {
                Text = "▶️",
                Size = new Size(45, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = _model.CreateFont(12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
				Margin = new Padding(0, 10, 15, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _btnPlay.FlatAppearance.BorderSize = 0;
            _btnPlay.Click += async (s, e) => await TogglePlayPauseStop();
            ApplyRetroButtonStyle(_btnPlay, _model.AccentColor, _model.BorderColor);
            if (_toolTip != null) _toolTip.SetToolTip(_btnPlay, "Reproducir/Pausar/Detener");

            Label lblLoop = new Label
            {
                Text = "🔄 Loop",
                AutoSize = true,
                Font = _model.CreateFont(9, FontStyle.Bold),
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 8, 5, 0)
            };

            _numLoopCount = new NumericUpDown
            {
                Size = new Size(60, 22),
                Minimum = 0,
                Maximum = 999,
                Value = 0,
                Font = _model.CreateFont(9),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 2, 0, 0)
            };
            if (_toolTip != null) _toolTip.SetToolTip(_numLoopCount, "0 = infinito; >0 repeticiones");

            Panel loopContainer = new Panel
            {
                Size = new Size(80, 45),
                BackColor = _model.PanelBackColor,
                Margin = new Padding(0, 0, 0, 0)
            };
            
            lblLoop.Location = new Point(10, 2);
            lblLoop.TextAlign = ContentAlignment.MiddleCenter;
            loopContainer.Controls.Add(lblLoop);
            
            _numLoopCount.Location = new Point(10, 20);
            loopContainer.Controls.Add(_numLoopCount);

            FlowLayoutPanel centerPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = _model.PanelBackColor,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Target window selector (container to align label above dropdown like Loop)
            Label lblTarget = new Label
            {
                Text = "🎯 Target",
                AutoSize = true,
                Font = _model.CreateFont(9, FontStyle.Bold),
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _cmbTargetWindow = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150,
                BackColor = _model.CardBackColor,
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            PopulateOpenWindows();
            // Actualizar la lista de ventanas cuando se abre el desplegable
            _cmbTargetWindow.DropDown += (s, e) => PopulateOpenWindows();
            _cmbTargetWindow.SelectedIndexChanged += (s, e) => OnTargetWindowChanged();
            _cmbTargetWindow.DrawItem += DrawTargetComboItem;
            if (_toolTip != null) _toolTip.SetToolTip(_cmbTargetWindow, "Ventana objetivo para reproducir");

            Panel targetContainer = new Panel
            {
                Size = new Size(170, 45),
                BackColor = _model.PanelBackColor,
                Margin = new Padding(10, 0, 0, 0)
            };
            lblTarget.Location = new Point(10, 2);
            lblTarget.TextAlign = ContentAlignment.MiddleCenter;
            targetContainer.Controls.Add(lblTarget);
            _cmbTargetWindow.Location = new Point(10, 17);
            targetContainer.Controls.Add(_cmbTargetWindow);

            centerPanel.Controls.Add(_btnPlay);
            centerPanel.Controls.Add(loopContainer);
            centerPanel.Controls.Add(targetContainer);

            // Hotkey row: sits below the three controls
            Panel hotkeyPanel = new Panel
            {
                AutoSize = false,
                Height = 28,
                Width = 400,
                BackColor = _model.PanelBackColor
            };
            Label lblHotkey = new Label
            {
                Text = "Toggle:",
                Location = new Point(0, 5),
                Size = new Size(90, 20),
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Font = _model.CreateFont(9, FontStyle.Bold)
            };
            TextBox txtHotkey = new TextBox
            {
                Location = new Point(95, 3),
                Size = new Size(160, 22),
                ReadOnly = true,
                TabStop = false,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtHotkey.Cursor = Cursors.Hand;
            if (_toolTip != null) _toolTip.SetToolTip(txtHotkey, "Click para configurar atajo");

            // Captura de combinación: al enfocar el textbox, escuchamos KeyDown del formulario para permitir Ctrl+... etc.
            bool capturing = false;
            txtHotkey.MouseDown += (s, e) => { capturing = true; txtHotkey.Text = "Press keys..."; controlsPanel.Focus(); };
            _mainForm.KeyDown += (s, e) =>
            {
                if (!capturing) return;
                if (e.KeyCode == Keys.Escape)
                {
                    capturing = false; txtHotkey.Text = ""; controlsPanel.Focus();
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
                txtHotkey.Text = text;
                capturing = false;
                controlsPanel.Focus();

                bool ok = (_mainForm as MainForm).RegisterGlobalHotKey(ctrl, alt, shift, win, key);
                if (!ok && _toolTip != null)
                {
                    _toolTip.SetToolTip(txtHotkey, "No se pudo registrar. Prueba otra combinación.");
                }
                e.Handled = true;
            };
            txtHotkey.Text = "Ctrl+M"; // Mostrar el valor por defecto

            // Guardar referencia al handler (el registro se hace en OnTabChanged)
            // IMPORTANTE: usar el valor de _numLoopCount en el momento del evento, no en la creación
            _macroHotkeyHandler = async (s, e) => {
                if (_numLoopCount != null)
                {
                    await _controller.TogglePlayPauseStop((int)_numLoopCount.Value);
                }
            };

            hotkeyPanel.Controls.Add(lblHotkey);
            hotkeyPanel.Controls.Add(txtHotkey);

            void CenterChildren()
            {
                // Colocar los tres controles centrados, y el panel de hotkey justo debajo centrado también
                int centerX = Math.Max(0, (controlsPanel.Width - centerPanel.Width) / 2);
                int centerY = Math.Max(0, (controlsPanel.Height - centerPanel.Height) / 2) - 10;
                centerPanel.Location = new Point(centerX, centerY);
                // Alinear el bloque de toggle al borde derecho del contenedor
                int spacing = 5;
                int totalWidth = lblHotkey.Width + spacing + txtHotkey.Width; // ancho real del grupo dentro del panel
                if (totalWidth > 0) hotkeyPanel.Width = totalWidth;
                int marginRight = 70; // ajusta este margen si quieres más/menos aire
                hotkeyPanel.Location = new Point(Math.Max(0, controlsPanel.ClientSize.Width - hotkeyPanel.Width - marginRight), centerPanel.Bottom + 6);
            }

            controlsPanel.Controls.Add(centerPanel);
            controlsPanel.Controls.Add(hotkeyPanel);
            controlsPanel.Resize += (s, e) => CenterChildren();
            playbackPanel.Resize += (s, e) => CenterChildren();
            _mainForm.Load += (s, e) => CenterChildren();

            playbackPanel.Controls.Add(controlsPanel);

            return playbackPanel;
        }

        // Enumeration of open windows and selection handling
        private void PopulateOpenWindows()
        {
            _cmbTargetWindow?.Items.Clear();
            _cmbTargetWindow?.Items.Add(new WindowItem("Desktop (global)", IntPtr.Zero, 0));
            foreach (var item in EnumerateTopLevelWindows())
            {
                _cmbTargetWindow.Items.Add(item);
            }
            _cmbTargetWindow.SelectedIndex = 0;
        }

        private void OnTargetWindowChanged()
        {
            if (_cmbTargetWindow?.SelectedItem is WindowItem item)
            {
                _model.TargetWindowHandle = item.Handle;
                _model.TargetProcessId = item.ProcessId;
                _model.TargetWindowTitle = item.Title;
            }
        }

        private sealed class WindowItem
        {
            public string Title { get; }
            public IntPtr Handle { get; }
            public uint ProcessId { get; }
            public WindowItem(string title, IntPtr handle, uint processId)
            {
                Title = title;
                Handle = handle;
                ProcessId = processId;
            }
            public override string ToString() => Title;
        }

        private void DrawTargetComboItem(object sender, DrawItemEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null)
                return;
            e.DrawBackground();
            Color back = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? _model.AccentColor : _model.CardBackColor;
            using (var b = new SolidBrush(back))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }
            if (e.Index >= 0 && e.Index < cb.Items.Count)
            {
                string text = cb.Items[e.Index].ToString();
                using (var f = new SolidBrush(Color.Yellow))
                {
                    e.Graphics.DrawString(text, cb.Font, f, e.Bounds);
                }
            }
            e.DrawFocusRectangle();
        }

        private IEnumerable<WindowItem> EnumerateTopLevelWindows()
        {
            List<WindowItem> list = new List<WindowItem>();
            IntPtr shellWindow = GetShellWindow();
            EnumWindows((hWnd, lParam) =>
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;
                StringBuilder sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    GetWindowThreadProcessId(hWnd, out uint pid);
                    list.Add(new WindowItem(title, hWnd, pid));
                }
                return true;
            }, IntPtr.Zero);
            return list;
        }

        // Win32 P/Invoke to enumerate windows
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Events
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Control+S para guardar - debe funcionar incluso si hay hotkeys globales registrados
            // Solo procesar si la ventana tiene foco y no estamos capturando un hotkey
            if (e.KeyCode == Keys.S && e.Control && !e.Handled)
            {
                // Detectar qué pestaña está activa usando el formulario principal
                TabControl tabControl = FindTabControl(_mainForm);
                if (tabControl != null && tabControl.SelectedTab != null)
                {
                    string tabName = tabControl.SelectedTab.Text;
                    if (tabName == "SHORTCUTS")
                    {
                        // Guardar shortcut si estamos en la pestaña de shortcuts
                        _controller.SaveCurrentShortcut();
                        SetDirty(false);
                    }
                    else if (tabName == "MACROS")
                    {
                        // Guardar macro si estamos en la pestaña de macros
                        SaveAndClearDirty();
                    }
                }
                else
                {
                    // Por defecto, guardar macro
                    SaveAndClearDirty();
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.N && e.Control)
            {
                _controller.CreateNewMacro();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.R && e.Control)
            {
                _controller.StartRecording();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Detectar qué pestaña está activa
                TabControl tabControl = FindTabControl(_mainForm);
                if (tabControl != null && tabControl.SelectedTab != null)
                {
                    string tabName = tabControl.SelectedTab.Text;
                    
                    if (tabName == "SHORTCUTS")
                    {
                        // Si estamos en la pestaña de shortcuts
                        if (_shortcutTreeView != null && _shortcutTreeView.Focused && _shortcutTreeView.SelectedNode?.Tag is MacroConfig)
                        {
                            // Eliminar shortcut seleccionado
                            _controller.DeleteCurrentShortcut();
                            e.Handled = true;
                        }
                        else
                        {
                            // Eliminar acciones de shortcut seleccionadas
                            if (_selectedShortcutActionIndices != null && _selectedShortcutActionIndices.Count > 1)
                            {
                                _controller.DeleteShortcutActions(_selectedShortcutActionIndices);
                                SetDirty(true);
                                e.Handled = true;
                            }
                            else if (_selectedShortcutActionIndex >= 0)
                            {
                                _controller.DeleteShortcutAction(_selectedShortcutActionIndex);
                                SetDirty(true);
                                e.Handled = true;
                            }
                        }
                    }
                    else if (tabName == "MACROS")
                    {
                        // Si estamos en la pestaña de macros
                        if (_macroTreeView != null && _macroTreeView.Focused && _macroTreeView.SelectedNode?.Tag is MacroConfig)
                        {
                            // Eliminar macro seleccionado
                            _controller.DeleteCurrentMacro();
                            e.Handled = true;
                        }
                        else
                        {
                            // Eliminar acciones de macro seleccionadas
                            if (_selectedActionIndices != null && _selectedActionIndices.Count > 1)
                            {
                                _controller.DeleteActions(_selectedActionIndices);
                                SetDirty(true);
                                e.Handled = true;
                            }
                            else if (_selectedActionIndex >= 0)
                            {
                                _controller.DeleteAction(_selectedActionIndex);
                                SetDirty(true);
                                e.Handled = true;
                            }
                        }
                    }
                }
                else
                {
                    // Por defecto, comportamiento de macros
                    if (_macroTreeView != null && _macroTreeView.Focused && _macroTreeView.SelectedNode?.Tag is MacroConfig)
                    {
                        _controller.DeleteCurrentMacro();
                        e.Handled = true;
                    }
                    else
                    {
                        if (_selectedActionIndices != null && _selectedActionIndices.Count > 1)
                        {
                            _controller.DeleteActions(_selectedActionIndices);
                            SetDirty(true);
                            e.Handled = true;
                        }
                        else if (_selectedActionIndex >= 0)
                        {
                            _controller.DeleteAction(_selectedActionIndex);
                            SetDirty(true);
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        // Helper para encontrar el TabControl en el formulario
        private TabControl FindTabControl(Control parent)
        {
            if (parent == null) return null;
            
            foreach (Control control in parent.Controls)
            {
                if (control is TabControl tc)
                    return tc;
                
                var found = FindTabControl(control);
                if (found != null)
                    return found;
            }
            
            return null;
        }

        // Tab change handler - registra el hotkey de la pestaña activa
        private void OnTabChanged(TabControl tabControl)
        {
            if (tabControl == null || tabControl.SelectedTab == null) return;
            
            MainForm mainForm = _mainForm as MainForm;
            if (mainForm == null) return;
            
            // Desregistrar todos los handlers y el hotkey actual
            if (_macroHotkeyHandler != null)
            {
                mainForm.GlobalHotKeyPressed -= _macroHotkeyHandler;
            }
            if (_shortcutHotkeyHandler != null)
            {
                mainForm.GlobalHotKeyPressed -= _shortcutHotkeyHandler;
            }
            mainForm.UnregisterGlobalHotKey();
            
            // Registrar el hotkey y handler de la pestaña activa
            string tabName = tabControl.SelectedTab.Text;
            if (tabName == "MACROS")
            {
                // Asegurar que el treeview de macros solo muestre macros
                RefreshMacroTree();
                // Registrar Ctrl+M para macros
                mainForm.RegisterGlobalHotKey(true, false, false, false, Keys.M);
                if (_macroHotkeyHandler != null)
                {
                    mainForm.GlobalHotKeyPressed += _macroHotkeyHandler;
                }
            }
            else if (tabName == "SHORTCUTS")
            {
                // Asegurar que el treeview de shortcuts solo muestre shortcuts
                RefreshShortcutTree();
                // Leer el hotkey del shortcut actual
                if (_model.CurrentShortcut != null && !string.IsNullOrEmpty(_model.CurrentShortcut.Hotkey))
                {
                    // Parsear el hotkey string (ej: "Ctrl+T" -> ctrl=true, key=Keys.T)
                    if (ParseHotkey(_model.CurrentShortcut.Hotkey, out bool ctrl, out bool alt, out bool shift, out bool win, out Keys key))
                    {
                        mainForm.RegisterGlobalHotKey(ctrl, alt, shift, win, key);
                        if (_shortcutHotkeyHandler != null)
                        {
                            mainForm.GlobalHotKeyPressed += _shortcutHotkeyHandler;
                        }
                    }
                    else
                    {
                        // Si no se puede parsear, usar el valor por defecto
                        mainForm.RegisterGlobalHotKey(true, false, false, false, Keys.T);
                        if (_shortcutHotkeyHandler != null)
                        {
                            mainForm.GlobalHotKeyPressed += _shortcutHotkeyHandler;
                        }
                    }
                }
                else
                {
                    // Si no hay hotkey configurado, usar el valor por defecto
                    mainForm.RegisterGlobalHotKey(true, false, false, false, Keys.T);
                    if (_shortcutHotkeyHandler != null)
                    {
                        mainForm.GlobalHotKeyPressed += _shortcutHotkeyHandler;
                    }
                }
            }
            // MOUSE tab no tiene hotkey registrado
        }

        // Helper para parsear un string de hotkey (ej: "Ctrl+T", "Alt+Shift+F1")
        private bool ParseHotkey(string hotkeyText, out bool ctrl, out bool alt, out bool shift, out bool win, out Keys key)
        {
            ctrl = false;
            alt = false;
            shift = false;
            win = false;
            key = Keys.None;

            if (string.IsNullOrEmpty(hotkeyText))
                return false;

            string[] parts = hotkeyText.Split('+');
            if (parts.Length == 0)
                return false;

            // El último elemento es la tecla
            string keyStr = parts[parts.Length - 1].Trim();

            // Intentar parsear la tecla
            if (!Enum.TryParse<Keys>(keyStr, true, out key))
            {
                return false;
            }

            // Verificar los modificadores
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string mod = parts[i].Trim().ToLower();
                if (mod == "ctrl" || mod == "control")
                    ctrl = true;
                else if (mod == "alt")
                    alt = true;
                else if (mod == "shift")
                    shift = true;
                else if (mod == "win" || mod == "windows")
                    win = true;
            }

            return true;
        }

        // Dirty state helpers
        private void SetDirty(bool isDirty)
        {
            _hasUnsavedChanges = isDirty;
            UpdateUnsavedBanner();
        }

        private void SaveAndClearDirty()
        {
            _controller.SaveCurrentMacro();
            SetDirty(false);
        }

        private void UpdateUnsavedBanner()
        {
            // Show only the central icon; do not add markers to the title
            _unsavedCenterIcon.Visible = _hasUnsavedChanges;
        }

        private void PositionUnsavedIcon()
        {
            if (_unsavedCenterIcon == null) return;
            Control host = _unsavedCenterIcon.Parent;
            if (host == null) return;
            int x = Math.Max(0, (host.ClientSize.Width - _unsavedCenterIcon.Width) / 2);
            int y = 6;
            _unsavedCenterIcon.Location = new Point(x, y);
            _unsavedCenterIcon.BringToFront();
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "There are unsaved changes. Do you want to save before exiting?",
                    "Unsaved changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    SaveAndClearDirty();
                    // continue closing
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                // No = exit without saving
            }
        }

        private void SubscribeToModelEvents()
        {
            _model.ActionRecorded += OnActionRecorded;
            _model.PlaybackStarted += OnPlaybackStarted;
            _model.PlaybackStopped += OnPlaybackStopped;
            _model.PlaybackPaused += OnPlaybackPaused;
            _model.PlaybackResumed += OnPlaybackResumed;
            _model.ConditionalArmedChanged += OnConditionalArmedChanged;
            _model.RecordingStarted += OnRecordingStarted;
            _model.RecordingStopped += OnRecordingStopped;
            _model.ShortcutRecordingStarted += OnShortcutRecordingStarted;
            _model.ShortcutRecordingStopped += OnShortcutRecordingStopped;
            _model.MacrosChanged += OnMacrosChanged;
            _model.CurrentMacroChanged += OnCurrentMacroChanged;
            _model.ShortcutsChanged += OnShortcutsChanged;
            _model.CurrentShortcutChanged += OnCurrentShortcutChanged;
        }

        private void OnActionRecorded(object sender, MacroAction action)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnActionRecorded(sender, action)));
                return;
            }
            // Refrescar display según si estamos grabando macro o shortcut
            if (_model.CurrentMacro != null)
            {
                RefreshActionsDisplay();
            }
            if (_model.CurrentShortcut != null)
            {
                RefreshShortcutActionsDisplay();
            }
            SetDirty(true);
        }

        private void OnPlaybackStarted(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnPlaybackStarted(sender, e)));
                return;
            }
            _btnPlay.Text = "⏹️";
            _btnPlay.BackColor = Color.FromArgb(244, 67, 54);
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnPlaybackStopped(sender, e)));
                return;
            }
            _btnPlay.Text = "▶️";
            _btnPlay.BackColor = _model.AccentColor;
        }

        private void OnPlaybackPaused(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnPlaybackPaused(sender, e)));
                return;
            }
            _btnPlay.Text = "⏸️";
            _btnPlay.BackColor = Color.FromArgb(255, 152, 0); // amber for pause/armed
        }

        private void OnPlaybackResumed(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnPlaybackResumed(sender, e)));
                return;
            }
            _btnPlay.Text = "⏹️";
            _btnPlay.BackColor = Color.FromArgb(244, 67, 54);
        }

        private void OnConditionalArmedChanged(object sender, bool armed)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnConditionalArmedChanged(sender, armed)));
                return;
            }
            if (!_model.IsPlaying && !_model.IsPaused)
            {
                if (armed)
                {
                    _btnPlay.Text = "⏸️"; // armed/waiting for window
                    _btnPlay.BackColor = Color.FromArgb(255, 152, 0);
                }
                else
                {
                    _btnPlay.Text = "▶️";
                    _btnPlay.BackColor = _model.AccentColor;
                }
            }
        }

        private void OnRecordingStarted(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnRecordingStarted(sender, e)));
                return;
            }
            // Solo actualizar botones de macros
            if (_btnRecord != null)
            {
                _btnRecord.Enabled = false;
                _btnRecord.BackColor = Color.FromArgb(100, 100, 100);
            }
            if (_btnStopRecord != null)
            {
                _btnStopRecord.Enabled = true;
                _btnStopRecord.BackColor = Color.FromArgb(244, 67, 54);
            }
        }

        private void OnRecordingStopped(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnRecordingStopped(sender, e)));
                return;
            }
            // Solo actualizar botones de macros
            if (_btnRecord != null)
            {
                _btnRecord.Enabled = true;
                _btnRecord.BackColor = Color.FromArgb(244, 67, 54);
            }
            if (_btnStopRecord != null)
            {
                _btnStopRecord.Enabled = false;
                _btnStopRecord.BackColor = Color.FromArgb(158, 158, 158);
            }
        }

        private void OnShortcutRecordingStarted(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnShortcutRecordingStarted(sender, e)));
                return;
            }
            // Solo actualizar botones de shortcuts
            if (_btnRecordShortcut != null)
            {
                _btnRecordShortcut.Enabled = false;
                _btnRecordShortcut.BackColor = Color.FromArgb(100, 100, 100);
            }
            if (_btnStopRecordShortcut != null)
            {
                _btnStopRecordShortcut.Enabled = true;
                _btnStopRecordShortcut.BackColor = Color.FromArgb(244, 67, 54);
            }
        }

        private void OnShortcutRecordingStopped(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnShortcutRecordingStopped(sender, e)));
                return;
            }
            // Solo actualizar botones de shortcuts
            if (_btnRecordShortcut != null)
            {
                _btnRecordShortcut.Enabled = true;
                _btnRecordShortcut.BackColor = Color.FromArgb(244, 67, 54);
            }
            if (_btnStopRecordShortcut != null)
            {
                _btnStopRecordShortcut.Enabled = false;
                _btnStopRecordShortcut.BackColor = Color.FromArgb(60, 60, 60);
            }
        }

        private void OnMacrosChanged(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnMacrosChanged(sender, e)));
                return;
            }
            RefreshMacroTree();
        }

        private void OnCurrentMacroChanged(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnCurrentMacroChanged(sender, e)));
                return;
            }
            RefreshActionsDisplay();
        }

        private void OnShortcutsChanged(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnShortcutsChanged(sender, e)));
                return;
            }
            RefreshShortcutTree();
        }

        private void OnCurrentShortcutChanged(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnCurrentShortcutChanged(sender, e)));
                return;
            }
            RefreshShortcutActionsDisplay();
            
            // Actualizar el checkbox Enable según el estado del shortcut
            if (_chkEnableShortcut != null && _model.CurrentShortcut != null)
            {
                _chkEnableShortcut.Checked = _model.CurrentShortcut.IsEnabled;
            }

            // Actualizar el campo de hotkey según el shortcut actual
            if (_txtHotkeyShortcut != null && _model.CurrentShortcut != null)
            {
                if (!string.IsNullOrEmpty(_model.CurrentShortcut.Hotkey))
                {
                    _txtHotkeyShortcut.Text = _model.CurrentShortcut.Hotkey;
                }
                else
                {
                    _txtHotkeyShortcut.Text = "Ctrl+T";
                    _model.CurrentShortcut.Hotkey = "Ctrl+T";
                }
                
                // Re-registrar el hotkey si estamos en la pestaña de shortcuts
                TabControl tabControl = FindTabControl(_mainForm);
                if (tabControl != null && tabControl.SelectedTab != null && tabControl.SelectedTab.Text == "SHORTCUTS")
                {
                    MainForm mainForm = _mainForm as MainForm;
                    if (mainForm != null)
                    {
                        // Desregistrar el anterior
                        if (_shortcutHotkeyHandler != null)
                        {
                            mainForm.GlobalHotKeyPressed -= _shortcutHotkeyHandler;
                        }
                        mainForm.UnregisterGlobalHotKey();

                        // Registrar el nuevo
                        if (!string.IsNullOrEmpty(_model.CurrentShortcut.Hotkey))
                        {
                            if (ParseHotkey(_model.CurrentShortcut.Hotkey, out bool ctrl, out bool alt, out bool shift, out bool win, out Keys key))
                            {
                                mainForm.RegisterGlobalHotKey(ctrl, alt, shift, win, key);
                                if (_shortcutHotkeyHandler != null)
                                {
                                    mainForm.GlobalHotKeyPressed += _shortcutHotkeyHandler;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configura un TabControl con dibujo personalizado para pestañas verdes retro
        /// </summary>
        private void SetupCustomTabControl(TabControl tabControl)
        {
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.BackColor = _model.PanelBackColor;

            tabControl.DrawItem += (s, e) =>
            {
                Graphics g = e.Graphics;
                TabPage tabPage = tabControl.TabPages[e.Index];
                Rectangle tabBounds = tabControl.GetTabRect(e.Index);

                // Color de fondo de la pestaña
                Color backColor = (e.State == DrawItemState.Selected)
                    ? _model.CardBackColor  // Verde oscuro cuando está seleccionada (RGB 18, 48, 18)
                    : _model.PanelBackColor;  // Verde más oscuro cuando no está seleccionada (RGB 15, 40, 15)

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, tabBounds);
                }

                // Borde verde del tema
                using (Pen pen = new Pen(_model.BorderColor, 1))
                {
                    g.DrawRectangle(pen, tabBounds.X, tabBounds.Y, tabBounds.Width - 1, tabBounds.Height - 1);
                }

                // Texto
                Color textColor = (e.State == DrawItemState.Selected)
                    ? _model.PanelForeColor  // Verde brillante si está seleccionada (RGB 0, 255, 0)
                    : Color.FromArgb(0, 180, 0);  // Verde más oscuro si no está seleccionada

                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    g.DrawString(
                        tabPage.Text,
                        tabControl.Font,
                        textBrush,
                        tabBounds,
                        sf
                    );
                }
            };

            // Dibujar el fondo del área de tabs (para cubrir el blanco)
            tabControl.Paint += (s, e) =>
            {
                TabControl tc = s as TabControl;
                if (tc.TabPages.Count == 0) return;

                int tabHeight = 0;
                try
                {
                    tabHeight = tc.GetTabRect(0).Height;
                }
                catch
                {
                    tabHeight = 23;
                }

                // Dibujar fondo del área de tabs
                Rectangle tabStripArea = new Rectangle(0, 0, tc.Width, tabHeight + 2);
                using (SolidBrush brush = new SolidBrush(_model.PanelBackColor))
                {
                    e.Graphics.FillRectangle(brush, tabStripArea);
                }

                // Dibujar línea separadora en la parte inferior del área de tabs
                using (Pen pen = new Pen(_model.BorderColor, 1))
                {
                    int lineY = tabHeight + 1;
                    e.Graphics.DrawLine(pen, 0, lineY, tc.Width, lineY);
                }
            };

            // Estilizar las páginas de las pestañas
            foreach (TabPage page in tabControl.TabPages)
            {
                page.BackColor = _model.PanelBackColor;
                page.ForeColor = _model.PanelForeColor;
            }
        }
    }
}


