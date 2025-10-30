using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;

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

            MenuStrip menu = CreateMenuBar();
            _mainForm.MainMenuStrip = menu;
            _mainForm.Controls.Add(menu);

            Panel mainContentPanel = new Panel
            {
                Name = "mainContentPanel",
                BackColor = _model.PanelBackColor
            };
            _mainForm.Controls.Add(mainContentPanel);

            // Nota: quitamos la banda visual; el estado sin guardar se indicará en el título

            _mainForm.Load += (s, e) => {
                int menuHeight = menu.Height;
                mainContentPanel.Location = new Point(0, menuHeight);
                mainContentPanel.Size = new Size(_mainForm.ClientSize.Width, _mainForm.ClientSize.Height - menuHeight);
            };
            
            _mainForm.Resize += (s, e) => {
                if (mainContentPanel != null && menu != null) {
                    int menuHeight = menu.Height;
                    mainContentPanel.Size = new Size(_mainForm.ClientSize.Width, _mainForm.ClientSize.Height - menuHeight);
                }
            };

            // Centro: icono de advertencia (solo cuando hay cambios sin guardar)
            _unsavedCenterIcon = new Label
            {
                AutoSize = true,
                Text = "⚠",
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Symbol", 16, FontStyle.Bold),
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
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            tabControl.Parent = mainContentPanel;

            CreateMainMacrosTab(tabControl);
            CreateShortcutsTab(tabControl);
            CreateMouseTab(tabControl);

            _mainForm.KeyDown += OnKeyDown;
        }

        private void CreateMainMacrosTab(TabControl tabControl)
        {
            TabPage mainTab = new TabPage("Macros")
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
            playbackPanel.Height = _model.PlaybackPanelHeight;
            verticalSplit.Panel1.Controls.Add(playbackPanel);

            _mainForm.Load += (s, e) => {
                horizontalSplit.SplitterDistance = Math.Max(_model.MinimumTreeViewWidth, (int)(horizontalSplit.Width * _model.TreeViewPercentage));
                verticalSplit.SplitterDistance = Math.Max(_model.MinimumEditorWidth, (int)(verticalSplit.Width * _model.EditorPercentage));
                // Asegurar que el panel de acciones se dibuja con tamaños correctos tras layout inicial
                RefreshActionsDisplay();
                PositionUnsavedIcon();
            };

            tabControl.TabPages.Add(mainTab);
        }

        private void CreateShortcutsTab(TabControl tabControl)
        {
            TabPage shortcutsTab = new TabPage("Shortcuts")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(20)
            };

            Label placeholderLabel = new Label
            {
                Text = "🚀 Shortcuts\n\nThis tab will be available soon.\nHere you can configure custom keyboard shortcuts.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                TextAlign = ContentAlignment.MiddleCenter
            };
            shortcutsTab.Controls.Add(placeholderLabel);

            tabControl.TabPages.Add(shortcutsTab);
        }

        private void CreateMouseTab(TabControl tabControl)
        {
            TabPage mouseTab = new TabPage("Mouse")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(20)
            };

            Label placeholderLabel = new Label
            {
                Text = "🖱️ Mouse\n\nThis tab will be available soon.\nHere you can configure specific mouse actions.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
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
            fileMenu.DropDownItems.Add("&New Macro (Ctrl+N)", null, (s, e) => _controller.CreateNewMacro());
            fileMenu.DropDownItems.Add("&Record (Ctrl+R)", null, (s, e) => _controller.StartRecording());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Save (Ctrl+S)", null, (s, e) => SaveAndClearDirty());
            fileMenu.DropDownItems.Add("&Delete (Del)", null, (s, e) => _controller.DeleteCurrentMacro());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Export", null, (s, e) => _controller.ExportMacro());
            fileMenu.DropDownItems.Add("&Import", null, (s, e) => _controller.ImportMacro());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("E&xit (Alt+F4)", null, (s, e) => _mainForm.Close());

            ToolStripMenuItem editMenu = new ToolStripMenuItem("&Edit");
            editMenu.DropDownItems.Add("&Undo (Ctrl+Z)", null, (s, e) => _controller.UndoAction());
            editMenu.DropDownItems.Add("&Redo (Ctrl+Y)", null, (s, e) => _controller.RedoAction());
            editMenu.DropDownItems.Add("-");
            editMenu.DropDownItems.Add("Cu&t (Ctrl+X)", null, (s, e) => _controller.CutAction());
            editMenu.DropDownItems.Add("&Copy (Ctrl+C)", null, (s, e) => _controller.CopyAction());
            editMenu.DropDownItems.Add("&Paste (Ctrl+V)", null, (s, e) => _controller.PasteAction());
            editMenu.DropDownItems.Add("-");
            editMenu.DropDownItems.Add("&Select All (Ctrl+A)", null, (s, e) => _controller.SelectAllAction());

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
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 15, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _btnPlay.FlatAppearance.BorderSize = 0;
            _btnPlay.Click += async (s, e) => await TogglePlayPauseStop();
            ApplyRetroButtonStyle(_btnPlay, _model.AccentColor, _model.BorderColor);

            Label lblLoop = new Label
            {
                Text = "🔄 Loop",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
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
                Value = 1,
                Font = new Font("Segoe UI", 9),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 2, 0, 0)
            };

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

            centerPanel.Controls.Add(_btnPlay);
            centerPanel.Controls.Add(loopContainer);

            void CenterChildren()
            {
                int x = Math.Max(0, (controlsPanel.Width - centerPanel.Width) / 2);
                int y = Math.Max(0, (controlsPanel.Height - centerPanel.Height) / 2);
                centerPanel.Location = new Point(x, y);
            }

            controlsPanel.Controls.Add(centerPanel);
            controlsPanel.Resize += (s, e) => CenterChildren();
            playbackPanel.Resize += (s, e) => CenterChildren();
            _mainForm.Load += (s, e) => CenterChildren();

            playbackPanel.Controls.Add(controlsPanel);

            return playbackPanel;
        }

        // Eventos
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                SaveAndClearDirty();
                e.Handled = true;
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
                // Si el TreeView tiene foco y hay macro seleccionada, elimina la macro actual
                if (_macroTreeView != null && _macroTreeView.Focused && _macroTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DeleteCurrentMacro();
                    e.Handled = true;
                }
                else
                {
                    // Si no, elimina acciones seleccionadas (múltiples o única)
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
            // Mostrar solo icono central; no añadir marcas al título
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
                    "Hay cambios sin guardar. ¿Deseas guardar antes de salir?",
                    "Cambios sin guardar",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    SaveAndClearDirty();
                    // continuar cierre
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                // No = salir sin guardar
            }
        }

        private void SubscribeToModelEvents()
        {
            _model.ActionRecorded += OnActionRecorded;
            _model.PlaybackStarted += OnPlaybackStarted;
            _model.PlaybackStopped += OnPlaybackStopped;
            _model.RecordingStarted += OnRecordingStarted;
            _model.RecordingStopped += OnRecordingStopped;
            _model.MacrosChanged += OnMacrosChanged;
            _model.CurrentMacroChanged += OnCurrentMacroChanged;
        }

        private void OnActionRecorded(object sender, MacroAction action)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnActionRecorded(sender, action)));
                return;
            }
            RefreshActionsDisplay();
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
            _btnPlay.BackColor = Color.FromArgb(33, 150, 243);
        }

        private void OnRecordingStarted(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnRecordingStarted(sender, e)));
                return;
            }
            _btnRecord.Enabled = false;
            _btnRecord.BackColor = Color.FromArgb(100, 100, 100);
            _btnStopRecord.Enabled = true;
            _btnStopRecord.BackColor = Color.FromArgb(244, 67, 54);
        }

        private void OnRecordingStopped(object sender, EventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new Action(() => OnRecordingStopped(sender, e)));
                return;
            }
            _btnRecord.Enabled = true;
            _btnRecord.BackColor = Color.FromArgb(244, 67, 54);
            _btnStopRecord.Enabled = false;
            _btnStopRecord.BackColor = Color.FromArgb(158, 158, 158);
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
    }
}


