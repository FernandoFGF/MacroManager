using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    /// <summary>
    /// Main view for the MacroManager application
    /// Handles all UI logic and presentation
    /// </summary>
    public class View : IDisposable
    {
        private Controller _controller;
        private Model _model;
        private MainForm _mainForm;

        // UI Controls
        private Panel _actionsPanel;
        private VScrollBar _actionsScrollBar;
        private Button _btnPlay;
        private Button _btnRecord;
        private Button _btnStopRecord;
        private NumericUpDown _numLoopCount;
        private TreeView _macroTreeView;
        
        // Rule editor controls
        private TextBox _txtKey;
        private TextBox _txtDelay;
        private ComboBox _cmbActionType;
        
        // Selected action tracking
        private int _selectedActionIndex = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        public View(Controller controller, Model model)
        {
            _controller = controller;
            _model = model;
            _mainForm = new MainForm();
        }

        /// <summary>
        /// Initialize the view
        /// </summary>
        public void Initialize()
        {
            SetupUI();
            SubscribeToModelEvents();
        }

        /// <summary>
        /// Get the main form
        /// </summary>
        public Form GetMainForm()
        {
            return _mainForm;
        }

        #region UI Setup

        /// <summary>
        /// Setup the complete UI with TabControl containing 3 tabs
        /// Tab 1: Main macros functionality, Tab 2: Shortcuts, Tab 3: Mouse
        /// </summary>
        private void SetupUI()
        {
            // Set AutoScaleMode and minimum size from configuration
            _mainForm.AutoScaleMode = AutoScaleMode.Font;
            _mainForm.MinimumSize = new Size(_model.MinWindowWidth, _model.MinWindowHeight);
            _mainForm.Size = new Size(_model.DefaultWindowWidth, _model.DefaultWindowHeight);

            // 1. Create menu bar FIRST
            MenuStrip menu = CreateMenuBar();
            _mainForm.MainMenuStrip = menu;
            _mainForm.Controls.Add(menu);

            // 2. Create main content area - DO NOT use Dock.Fill to avoid overlap
            Panel mainContentPanel = new Panel
            {
                Name = "mainContentPanel",
                BackColor = _model.PanelBackColor
            };
            _mainForm.Controls.Add(mainContentPanel);

            // 3. Adjust the main panel size so it doesn't overlap with the menu
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

            // 4. Create TabControl
            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "mainTabControl",
                BackColor = _model.PanelBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            mainContentPanel.Controls.Add(tabControl);

            // 5. Create the three tabs
            CreateMainMacrosTab(tabControl);
            CreateShortcutsTab(tabControl);
            CreateMouseTab(tabControl);

            // Setup keyboard shortcuts
            _mainForm.KeyDown += OnKeyDown;
        }

        /// <summary>
        /// Create the main macros tab with all existing functionality
        /// </summary>
        private void CreateMainMacrosTab(TabControl tabControl)
        {
            TabPage mainTab = new TabPage("Macros")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(5)
            };

            // Create horizontal splitter (Left | Center+Right)
            SplitContainer horizontalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "horizontalSplit",
                BackColor = _model.PanelBackColor
            };
            mainTab.Controls.Add(horizontalSplit);

            // Left panel - TreeView (25% width)
            CreateMacroTree(horizontalSplit.Panel1);

            // Create right panel container for Center+Right
            Panel rightContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "rightContainer",
                BackColor = _model.PanelBackColor
            };
            horizontalSplit.Panel2.Controls.Add(rightContainer);

            // Create vertical splitter (Center | Right)
            SplitContainer verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "verticalSplit",
                BackColor = _model.PanelBackColor
            };
            rightContainer.Controls.Add(verticalSplit);

            // Center panel - Text editor with switch button
            CreateTextEditorWithSwitch(verticalSplit.Panel1);

            // Right panel - Rule editor with + and - buttons
            CreateRuleEditorWithButtons(verticalSplit.Panel2);

            // Create playback panel in the center, below editor
            Panel playbackPanel = CreatePlaybackPanelControl();
            playbackPanel.Dock = DockStyle.Bottom;
            playbackPanel.Height = _model.PlaybackPanelHeight;
            verticalSplit.Panel1.Controls.Add(playbackPanel); // Add to center panel only

            // Configure splitter distances after all controls are created
            _mainForm.Load += (s, e) => {
                // Set horizontal split based on configuration
                horizontalSplit.SplitterDistance = Math.Max(_model.MinimumTreeViewWidth, (int)(horizontalSplit.Width * _model.TreeViewPercentage));
                
                // Set vertical split based on configuration
                verticalSplit.SplitterDistance = Math.Max(_model.MinimumEditorWidth, (int)(verticalSplit.Width * _model.EditorPercentage));
            };

            tabControl.TabPages.Add(mainTab);
        }

        /// <summary>
        /// Create the shortcuts tab (empty for now)
        /// </summary>
        private void CreateShortcutsTab(TabControl tabControl)
        {
            TabPage shortcutsTab = new TabPage("Shortcuts")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(20)
            };

            // Create a placeholder label
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

        /// <summary>
        /// Create the mouse tab (empty for now)
        /// </summary>
        private void CreateMouseTab(TabControl tabControl)
        {
            TabPage mouseTab = new TabPage("Mouse")
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(20)
            };

            // Create a placeholder label
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

        /// <summary>
        /// Create the menu bar with File and Edit options
        /// </summary>
        private MenuStrip CreateMenuBar()
        {
            MenuStrip menu = new MenuStrip();

            // File menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&New Macro (Ctrl+N)", null, (s, e) => _controller.CreateNewMacro());
            fileMenu.DropDownItems.Add("&Record (Ctrl+R)", null, (s, e) => _controller.StartRecording());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Save (Ctrl+S)", null, (s, e) => _controller.SaveCurrentMacro());
            fileMenu.DropDownItems.Add("&Delete (Del)", null, (s, e) => _controller.DeleteCurrentMacro());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Export", null, (s, e) => _controller.ExportMacro());
            fileMenu.DropDownItems.Add("&Import", null, (s, e) => _controller.ImportMacro());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("E&xit (Alt+F4)", null, (s, e) => _mainForm.Close());

            // Edit menu
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

        /// <summary>
        /// Create macro tree in left sidebar with modern styling
        /// </summary>
        private void CreateMacroTree(Control parent)
        {
            _macroTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Nodes = { },
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Courier New", 10),
                Indent = 20,
                ShowLines = false,
                ShowPlusMinus = false,
                ShowRootLines = false,
                FullRowSelect = true,
                HotTracking = true
            };

            // Create context menu
            CreateTreeViewContextMenu();

            RefreshMacroTree();

            _macroTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Node.Tag is MacroConfig macro)
                {
                    _controller.LoadMacro(macro);
                }
            };

            _macroTreeView.DoubleClick += (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.RenameCurrentMacro();
                }
            };

            // Handle Delete key
            _macroTreeView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && _macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.DeleteCurrentMacro();
                }
            };

            parent.Controls.Add(_macroTreeView);
        }

        /// <summary>
        /// Create context menu for TreeView
        /// </summary>
        private void CreateTreeViewContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip
            {
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Segoe UI", 9)
            };

            // Rename macro
            ToolStripMenuItem renameItem = new ToolStripMenuItem("✏️ Rename", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.RenameCurrentMacro();
                }
            });
            contextMenu.Items.Add(renameItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Duplicate macro
            ToolStripMenuItem duplicateItem = new ToolStripMenuItem("📋 Duplicate", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.DuplicateCurrentMacro();
                }
            });
            contextMenu.Items.Add(duplicateItem);

            // Export macro
            ToolStripMenuItem exportItem = new ToolStripMenuItem("📤 Export", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.ExportMacro();
                }
            });
            contextMenu.Items.Add(exportItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Open file location
            ToolStripMenuItem openLocationItem = new ToolStripMenuItem("📁 Open Location", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.OpenMacroLocation(macro);
                }
            });
            contextMenu.Items.Add(openLocationItem);

            // Show properties
            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("ℹ️ Properties", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.ShowMacroProperties(macro);
                }
            });
            contextMenu.Items.Add(propertiesItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Refresh macros
            ToolStripMenuItem refreshItem = new ToolStripMenuItem("🔄 Refresh List", null, (s, e) =>
            {
                _controller.RefreshMacros();
            });
            contextMenu.Items.Add(refreshItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // Delete macro
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Delete", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.DeleteCurrentMacro();
                }
            });
            deleteItem.BackColor = Color.FromArgb(244, 67, 54);
            deleteItem.ForeColor = Color.White;
            contextMenu.Items.Add(deleteItem);

            // Assign context menu to TreeView
            _macroTreeView.ContextMenuStrip = contextMenu;

            // Enable right-click selection
            _macroTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _macroTreeView.SelectedNode = e.Node;
                }
            };
        }

        /// <summary>
        /// Create the actions panel with buttons (center area)
        /// </summary>
        private void CreateTextEditorWithSwitch(Control parent)
        {
            // Create main panel for actions
            _actionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _model.CardBackColor,
                Padding = new Padding(10),
                AutoScroll = true
            };

            // Create scroll bar for better control
            _actionsScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Width = 17
            };

            _actionsPanel.Controls.Add(_actionsScrollBar);
            parent.Controls.Add(_actionsPanel);
        }

        /// <summary>
        /// Create the rule editor panel with + and - buttons (right area)
        /// </summary>
        private void CreateRuleEditorWithButtons(Control parent)
        {
            Panel ruleEditorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = _model.PanelBackColor
            };

            // Modern title label
            Label lblTitle = new Label
            {
                Text = "📝 Click on a line in the editor to see and modify action parameters",
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Courier New", 9, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Padding = new Padding(10, 5, 10, 5)
            };
            ruleEditorPanel.Controls.Add(lblTitle);

            // Modern key input
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
            ruleEditorPanel.Controls.Add(lblKey);

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
            // Auto-save on text change
            txtKey.TextChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(txtKey);

            // Modern delay input
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
            ruleEditorPanel.Controls.Add(lblDelay);

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
            // Auto-save on text change
            txtDelay.TextChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(txtDelay);

            // Modern action type input
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
            ruleEditorPanel.Controls.Add(lblActionType);

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
            cmbActionType.Items.AddRange(new[] { "KeyPress", "KeyDown", "KeyUp", "MouseLeftDown", "MouseLeftUp", "MouseRightDown", "MouseRightUp", "MouseMove", "Delay" });
            cmbActionType.SelectedIndex = 0;
            // Auto-save on selection change
            cmbActionType.SelectedIndexChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(cmbActionType);

            // + and - buttons at the bottom with Save button
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = _model.PanelBackColor
            };

            // Top row: +, -, Duplicate, Save buttons
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
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => _controller.AddNewAction();
            topButtonRow.Controls.Add(btnAdd);

            Button btnRemove = new Button
            {
                Text = "➖",
                Location = new Point(85, 5),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) => _controller.DeleteAction(_selectedActionIndex);
            topButtonRow.Controls.Add(btnRemove);

            Button btnDuplicate = new Button
            {
                Text = "📋",
                Location = new Point(160, 5),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDuplicate.FlatAppearance.BorderSize = 0;
            btnDuplicate.Click += (s, e) => _controller.DuplicateAction(_selectedActionIndex);
            topButtonRow.Controls.Add(btnDuplicate);


            buttonPanel.Controls.Add(topButtonRow);

            // Recording row: Record and Stop Record buttons
            Panel recordingRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = _model.PanelBackColor
            };

            // Record Button
            _btnRecord = new Button
            {
                Text = "🔴",
                Location = new Point(10, 5),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnRecord.FlatAppearance.BorderSize = 0;
            _btnRecord.Click += (s, e) => _controller.StartRecording();
            recordingRow.Controls.Add(_btnRecord);

            // Stop Record Button
            _btnStopRecord = new Button
            {
                Text = "⏹️",
                Location = new Point(85, 5),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnStopRecord.FlatAppearance.BorderSize = 0;
            _btnStopRecord.Click += (s, e) => _controller.StopRecording();
            recordingRow.Controls.Add(_btnStopRecord);

            // Save Button
            Button btnSave = new Button
            {
                Text = "💾",
                Location = new Point(160, 5),
                Size = new Size(70, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => _controller.SaveCurrentMacro();
            recordingRow.Controls.Add(btnSave);

            buttonPanel.Controls.Add(recordingRow);
            ruleEditorPanel.Controls.Add(buttonPanel);

            // Store references for later use
            _txtKey = txtKey;
            _txtDelay = txtDelay;
            _cmbActionType = cmbActionType;

            parent.Controls.Add(ruleEditorPanel);
        }

        /// <summary>
        /// Create playback controls panel (bottom area)
        /// Returns the panel instead of adding directly
        /// </summary>
        private Panel CreatePlaybackPanelControl()
        {
            Panel playbackPanel = new Panel
            {
                BackColor = _model.PanelBackColor,
                Padding = new Padding(8),
                Height = 60
            };

            // Create main controls container
            FlowLayoutPanel controlsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(0),
                AutoScroll = false
            };

            // Play/Pause/Stop Button (Solo icono) - Cambia entre play, pausa y stop
            _btnPlay = new Button
            {
                Text = "▶️",
                Size = new Size(45, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 12, 15, 12),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _btnPlay.FlatAppearance.BorderSize = 0;
            _btnPlay.Click += async (s, e) => await TogglePlayPauseStop();

            // Loop Label
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

            // Loop Count NumericUpDown
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

            // Create a container for Loop controls (vertical layout)
            Panel loopContainer = new Panel
            {
                Size = new Size(80, 45),
                BackColor = _model.PanelBackColor,
                Margin = new Padding(0, 7, 0, 7)
            };
            
            // Position loop label at top (centered horizontally)
            lblLoop.Location = new Point(10, 2);
            lblLoop.TextAlign = ContentAlignment.MiddleCenter;
            loopContainer.Controls.Add(lblLoop);
            
            // Position numeric control below (centered horizontally)
            _numLoopCount.Location = new Point(10, 20);
            loopContainer.Controls.Add(_numLoopCount);

            // Add controls to panel
            controlsPanel.Controls.Add(_btnPlay);
            controlsPanel.Controls.Add(loopContainer);

            playbackPanel.Controls.Add(controlsPanel);

            return playbackPanel;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle keyboard shortcuts
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                _controller.SaveCurrentMacro();
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
        }

        /// <summary>
        /// Subscribe to model events
        /// </summary>
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

        #endregion

        #region UI Update Methods

        /// <summary>
        /// Refresh the actions display with buttons
        /// </summary>
        private void RefreshActionsDisplay()
        {
            if (_actionsPanel == null || _model.CurrentMacro == null) return;

            // Clear existing buttons
            _actionsPanel.Controls.Clear();
            _actionsPanel.Controls.Add(_actionsScrollBar);

            // Create buttons for each action
            int yPosition = 10;
            int buttonHeight = 70;
            int buttonSpacing = 15;

            for (int i = 0; i < _model.CurrentMacro.Actions.Count; i++)
            {
                var action = _model.CurrentMacro.Actions[i];
                var actionButton = CreateActionButton(action, i, yPosition);
                _actionsPanel.Controls.Add(actionButton);
                
                yPosition += buttonHeight + buttonSpacing;
            }

            // Update panel height to fit all buttons
            _actionsPanel.Height = Math.Max(_actionsPanel.Parent.Height, yPosition + 20);
        }

        /// <summary>
        /// Create a button for a specific action
        /// </summary>
        private Button CreateActionButton(MacroAction action, int index, int yPosition)
        {
            string keyDisplay = _model.GetKeyDisplay(action);
            string actionType = _model.GetActionTypeDisplay(action.Type);
            
            var button = new Button
            {
                Text = $"#{index + 1}  {actionType}\nKey: {keyDisplay}\nDelay: {action.DelayMs}ms",
                Location = new Point(10, yPosition),
                Size = new Size(_actionsPanel.Width - 50, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = index, // Store action index for easy reference
                Cursor = Cursors.Hand
            };

            // Style the button with better appearance
            button.FlatAppearance.BorderColor = _model.AccentColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);

            // Add hover effects
            button.MouseEnter += (s, e) => {
                if (button.BackColor != _model.AccentColor) // Don't change if already selected
                {
                    button.BackColor = Color.FromArgb(30, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);
                }
            };
            
            button.MouseLeave += (s, e) => {
                if (button.BackColor != _model.AccentColor) // Don't change if already selected
                {
                    button.BackColor = _model.CardBackColor;
                }
            };

            // Add click event
            button.Click += (s, e) => SelectAction(index);

            return button;
        }

        /// <summary>
        /// Select an action by index
        /// </summary>
        private void SelectAction(int actionIndex)
        {
            if (_model.CurrentMacro == null || actionIndex < 0 || actionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            _selectedActionIndex = actionIndex;

            // Highlight the selected button
            foreach (Control control in _actionsPanel.Controls)
            {
                if (control is Button button && button.Tag is int index)
                {
                    if (index == actionIndex)
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

            // Load action to editor
            LoadActionToEditor(actionIndex);
        }

        /// <summary>
        /// Load action to the right panel editor
        /// </summary>
        private void LoadActionToEditor(int actionIndex)
        {
            if (_model.CurrentMacro == null || actionIndex < 0 || actionIndex >= _model.CurrentMacro.Actions.Count)
            {
                ClearRuleEditor();
                return;
            }

            _selectedActionIndex = actionIndex;
            var action = _model.CurrentMacro.Actions[actionIndex];

            // Populate fields
            _txtKey.Text = _model.GetKeyDisplay(action);
            _txtDelay.Text = action.DelayMs.ToString();
            _cmbActionType.SelectedItem = action.Type.ToString();
        }

        /// <summary>
        /// Clear rule editor
        /// </summary>
        private void ClearRuleEditor()
        {
            _selectedActionIndex = -1;
            _txtKey.Text = "";
            _txtDelay.Text = "0";
            _cmbActionType.SelectedIndex = 0;
        }

        /// <summary>
        /// Save changes from right panel to the selected action
        /// </summary>
        private void SaveActionChanges()
        {
            if (_selectedActionIndex < 0 || _model.CurrentMacro == null || _selectedActionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            // Update delay
            if (int.TryParse(_txtDelay.Text, out int delay))
            {
                // Update action type
                ActionType actionType = ActionType.KeyPress;
                if (_cmbActionType.SelectedItem != null)
                {
                    Enum.TryParse<ActionType>(_cmbActionType.SelectedItem.ToString(), out actionType);
                }

                _controller.UpdateAction(_selectedActionIndex, _txtKey.Text, delay, actionType);
            }
        }

        /// <summary>
        /// Refresh macro tree
        /// </summary>
        private void RefreshMacroTree()
        {
            if (_macroTreeView == null) return;

            _macroTreeView.Nodes.Clear();

            // Add root node
            TreeNode rootNode = new TreeNode("Macros");
            _macroTreeView.Nodes.Add(rootNode);

            // Add all macros sorted by last used
            foreach (var macro in _model.LoadedMacros.OrderByDescending(m => m.LastUsed))
            {
                TreeNode macroNode = new TreeNode(macro.Name) { Tag = macro };
                rootNode.Nodes.Add(macroNode);
            }

            // Expand root
            rootNode.Expand();
        }

        /// <summary>
        /// Toggle play/pause/stop
        /// </summary>
        private async Task TogglePlayPauseStop()
        {
            int repeatCount = (int)_numLoopCount.Value;
            await _controller.TogglePlayPauseStop(repeatCount);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            _mainForm?.Dispose();
        }

        #endregion
    }
}
