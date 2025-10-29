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
    /// Main application form with redesigned UI
    /// Menu bar, sidebar with macro tree, and action editor
    /// </summary>
    public partial class MainForm : Form
    {
        // Services
        private MacroRecorder _recorder;
        private MacroPlayer _player;
        private SettingsManager _settingsManager;

        // Data
        private List<MacroConfig> _loadedMacros;
        private MacroConfig _currentMacro;

        // UI Controls
        private Panel _actionsPanel;
        private VScrollBar _actionsScrollBar;
        private Button _btnPlay;
        private NumericUpDown _numLoopCount;
        private TreeView _macroTreeView;
        
        // Rule editor controls
        private TextBox _txtKey;
        private TextBox _txtDelay;
        private ComboBox _cmbActionType;
        
        // Selected action tracking
        private int _selectedActionIndex = -1;
        
        // Playback state tracking
        private enum PlaybackState { Stopped, Playing, Paused }
        private PlaybackState _currentPlaybackState = PlaybackState.Stopped;

        // Theme colors
        private Color _panelBackColor;
        private Color _panelForeColor;
        private Color _accentColor;
        private Color _cardBackColor;
        private Color _borderColor;

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

        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            this.Text = "MacroManager - Action Editor";
            LoadUIConfiguration();
            ApplySystemTheme();
            InitializeServices();
            LoadMacros();
            SetupUI();
        }

        /// <summary>
        /// Apply retro green LCD theme colors
        /// </summary>
        private void ApplySystemTheme()
        {
            // Retro green LCD color scheme (ignoring system theme)
            // Dark green background with bright LCD green text
            
            // Very dark green background (almost black with green tint)
            this.BackColor = Color.FromArgb(12, 32, 12);
            
            // Bright LCD green text
            this.ForeColor = Color.FromArgb(0, 255, 0);
            
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
                MessageBox.Show($"Error loading UI configuration:\n{ex.Message}\n\nUsing default settings.", "Configuration Warning");
            }
        }

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
        /// Load all macros and display UI
        /// </summary>
        private void LoadMacros()
        {
            _loadedMacros = _settingsManager.LoadAllMacros();
        }

        /// <summary>
        /// Setup the complete UI with 3-column layout
        /// Left: TreeView, Center: Text Editor, Right: Tools
        /// </summary>
        private void SetupUI()
        {
            // Set AutoScaleMode and minimum size from configuration
            this.AutoScaleMode = AutoScaleMode.Font;
            this.MinimumSize = new Size(_minWindowWidth, _minWindowHeight);
            this.Size = new Size(_defaultWindowWidth, _defaultWindowHeight);

            // 1. Create menu bar FIRST
            MenuStrip menu = CreateMenuBar();
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            // 2. Create main content area - NO usar Dock.Fill para evitar superposición
            Panel mainContentPanel = new Panel
            {
                Name = "mainContentPanel",
                BackColor = _panelBackColor
            };
            this.Controls.Add(mainContentPanel);

            // 3. Ajustar el tamaño del panel principal para que no se superponga con el menú
            this.Load += (s, e) => {
                int menuHeight = menu.Height;
                mainContentPanel.Location = new Point(0, menuHeight);
                mainContentPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - menuHeight);
            };
            
            this.Resize += (s, e) => {
                if (mainContentPanel != null && menu != null) {
                    int menuHeight = menu.Height;
                    mainContentPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - menuHeight);
                }
            };

            // 4. Create horizontal splitter (Left | Center+Right)
            SplitContainer horizontalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "horizontalSplit",
                BackColor = _panelBackColor
            };
            mainContentPanel.Controls.Add(horizontalSplit);

            // 5. Left panel - TreeView (25% width)
            CreateMacroTree(horizontalSplit.Panel1);

            // 6. Create right panel container for Center+Right
            Panel rightContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Name = "rightContainer",
                BackColor = _panelBackColor
            };
            horizontalSplit.Panel2.Controls.Add(rightContainer);

            // 7. Create vertical splitter (Center | Right)
            SplitContainer verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Name = "verticalSplit",
                BackColor = _panelBackColor
            };
            rightContainer.Controls.Add(verticalSplit);

            // 8. Center panel - Text editor with switch button
            CreateTextEditorWithSwitch(verticalSplit.Panel1);

            // 9. Right panel - Rule editor with + and - buttons
            CreateRuleEditorWithButtons(verticalSplit.Panel2);

            // 10. Create playback panel in the center, below editor
            Panel playbackPanel = CreatePlaybackPanelControl();
            playbackPanel.Dock = DockStyle.Bottom;
            playbackPanel.Height = _playbackPanelHeight;
            verticalSplit.Panel1.Controls.Add(playbackPanel); // Add to center panel only

            // 11. Configure splitter distances after all controls are created
            this.Load += (s, e) => {
                // Set horizontal split based on configuration
                horizontalSplit.SplitterDistance = Math.Max(_minimumTreeViewWidth, (int)(horizontalSplit.Width * _treeViewPercentage));
                
                // Set vertical split based on configuration
                verticalSplit.SplitterDistance = Math.Max(_minimumEditorWidth, (int)(verticalSplit.Width * _editorPercentage));
            };

            // Load last used macro or prompt to create first
            LoadLastMacro();
        }

        /// <summary>
        /// Create the menu bar with File and Edit options
        /// </summary>
        private MenuStrip CreateMenuBar()
        {
            MenuStrip menu = new MenuStrip();

            // File menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&New Macro (Ctrl+N)", null, (s, e) => CreateNewMacro());
            fileMenu.DropDownItems.Add("&Record (Ctrl+R)", null, (s, e) => StartRecording());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Save (Ctrl+S)", null, (s, e) => SaveCurrentMacro());
            fileMenu.DropDownItems.Add("&Delete (Del)", null, (s, e) => DeleteCurrentMacro());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Export", null, (s, e) => ExportMacro());
            fileMenu.DropDownItems.Add("&Import", null, (s, e) => ImportMacro());
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("E&xit (Alt+F4)", null, (s, e) => Close());

            // Edit menu
            ToolStripMenuItem editMenu = new ToolStripMenuItem("&Edit");
            editMenu.DropDownItems.Add("&Undo (Ctrl+Z)", null, (s, e) => UndoAction());
            editMenu.DropDownItems.Add("&Redo (Ctrl+Y)", null, (s, e) => RedoAction());
            editMenu.DropDownItems.Add("-");
            editMenu.DropDownItems.Add("Cu&t (Ctrl+X)", null, (s, e) => CutAction());
            editMenu.DropDownItems.Add("&Copy (Ctrl+C)", null, (s, e) => CopyAction());
            editMenu.DropDownItems.Add("&Paste (Ctrl+V)", null, (s, e) => PasteAction());
            editMenu.DropDownItems.Add("-");
            editMenu.DropDownItems.Add("&Select All (Ctrl+A)", null, (s, e) => SelectAllAction());

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
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Courier New", 10),
                Indent = 20,
                ShowLines = false,
                ShowPlusMinus = false,
                ShowRootLines = false,
                FullRowSelect = true,
                HotTracking = true
            };

            RefreshMacroTree();

            _macroTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Node.Tag is MacroConfig macro)
                {
                    LoadMacro(macro);
                }
            };

            _macroTreeView.DoubleClick += (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    RenameSelectedMacro();
                }
            };

            parent.Controls.Add(_macroTreeView);
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
                BackColor = _cardBackColor,
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
                BackColor = _panelBackColor
            };

            // Modern title label
            Label lblTitle = new Label
            {
                Text = "📝 Click on a line in the editor to see and modify action parameters",
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Courier New", 9, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor,
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
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor
            };
            ruleEditorPanel.Controls.Add(lblKey);

            TextBox txtKey = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 10),
                Name = "txtKey",
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
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
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor
            };
            ruleEditorPanel.Controls.Add(lblDelay);

            TextBox txtDelay = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 10),
                Name = "txtDelay",
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
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
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor
            };
            ruleEditorPanel.Controls.Add(lblActionType);

            ComboBox cmbActionType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 9),
                Name = "cmbActionType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
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
                Height = 100,
                BackColor = _panelBackColor
            };

            // Top row: +, -, Duplicate buttons
            Panel topButtonRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = _panelBackColor
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
            btnAdd.Click += (s, e) => AddNewAction();
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
            btnRemove.Click += (s, e) => DeleteSelectedAction();
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
            btnDuplicate.Click += (s, e) => DuplicateSelectedAction();
            topButtonRow.Controls.Add(btnDuplicate);

            buttonPanel.Controls.Add(topButtonRow);

            // Bottom row: Save button
            Panel bottomButtonRow = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = _panelBackColor
            };

            Button btnSave = new Button
            {
                Text = "💾",
                Dock = DockStyle.Fill,
                Height = 45,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Courier New", 11, FontStyle.Bold),
                Margin = new Padding(5),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SaveCurrentMacro();
            bottomButtonRow.Controls.Add(btnSave);

            buttonPanel.Controls.Add(bottomButtonRow);
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
                BackColor = _panelBackColor,
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
            _btnPlay.Click += (s, e) => TogglePlayPauseStop();

            // Loop Label
            Label lblLoop = new Label
            {
                Text = "🔄 Loop",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor,
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
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 2, 0, 0)
            };

            // Create a container for Loop controls (vertical layout)
            Panel loopContainer = new Panel
            {
                Size = new Size(80, 45),
                BackColor = _panelBackColor,
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

        /// <summary>
        /// Select an action by index
        /// </summary>
        private void SelectAction(int actionIndex)
        {
            if (_currentMacro == null || actionIndex < 0 || actionIndex >= _currentMacro.Actions.Count)
                return;

            _selectedActionIndex = actionIndex;

            // Highlight the selected button
            foreach (Control control in _actionsPanel.Controls)
            {
                if (control is Button button && button.Tag is int index)
                {
                    if (index == actionIndex)
                    {
                        button.BackColor = _accentColor;
                        button.ForeColor = Color.White;
                    }
                    else
                    {
                        button.BackColor = _cardBackColor;
                        button.ForeColor = _panelForeColor;
                    }
                }
            }

            // Load action to editor
            LoadActionToEditor(actionIndex);
        }

        /// <summary>
        /// Select action from click location
        /// </summary>
        private void SelectActionFromClick(Point clickPoint)
        {
            // This method is no longer needed as we use button clicks directly
            // Keeping for compatibility but not used
        }

        /// <summary>
        /// Load action to the right panel editor
        /// </summary>
        private void LoadActionToEditor(int actionIndex)
        {
            if (_currentMacro == null || actionIndex < 0 || actionIndex >= _currentMacro.Actions.Count)
            {
                ClearRuleEditor();
                return;
            }

            _selectedActionIndex = actionIndex;
            var action = _currentMacro.Actions[actionIndex];

            // Populate fields
            _txtKey.Text = GetKeyDisplay(action);
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
        /// Highlight selected action in the editor
        /// </summary>
        private void HighlightAction(int actionIndex)
        {
            if (_actionsPanel == null || actionIndex < 0) return;

            // Highlight the selected button
            foreach (Control control in _actionsPanel.Controls)
            {
                if (control is Button button && button.Tag is int index)
                {
                    if (index == actionIndex)
                    {
                        button.BackColor = _accentColor;
                        button.ForeColor = Color.White;
                    }
                    else
                    {
                        button.BackColor = _cardBackColor;
                        button.ForeColor = _panelForeColor;
                    }
                }
            }
        }

        /// <summary>
        /// Update rule editor when selection changes in text editor (Legacy - no longer used)
        /// </summary>
        private void UpdateRuleEditor()
        {
            // This is now handled by SelectActionFromClick and LoadActionToEditor
        }

        /// <summary>
        /// Save changes from right panel to the selected action
        /// </summary>
        private void SaveActionChanges()
        {
            if (_selectedActionIndex < 0 || _currentMacro == null || _selectedActionIndex >= _currentMacro.Actions.Count)
                return;

            var action = _currentMacro.Actions[_selectedActionIndex];

            // Update delay
            if (int.TryParse(_txtDelay.Text, out int delay))
            {
                action.DelayMs = delay;
            }

            // Update action type
            if (_cmbActionType.SelectedItem != null)
            {
                if (Enum.TryParse<ActionType>(_cmbActionType.SelectedItem.ToString(), out ActionType actionType))
                {
                    action.Type = actionType;
                }
            }

            // Update key (for keyboard actions)
            if (_txtKey.Text.Length > 0)
            {
                try
                {
                    Keys key = (Keys)Enum.Parse(typeof(Keys), _txtKey.Text, true);
                    action.KeyCode = (int)key;
                }
                catch
                {
                    // Keep previous value if invalid
                }
            }

            // Refresh the display
            RefreshActionsDisplay();
            HighlightAction(_selectedActionIndex);
        }

        /// <summary>
        /// Save rule changes
        /// </summary>
        private void SaveRuleChanges(TextBox txtKey, TextBox txtDelay, ComboBox cmbActionType)
        {
            if (_currentMacro == null || _selectedActionIndex < 0 || _selectedActionIndex >= _currentMacro.Actions.Count) return;

            try
            {
                var action = _currentMacro.Actions[_selectedActionIndex];
                
                // Update delay
                if (int.TryParse(txtDelay.Text, out int delay))
                {
                    action.DelayMs = delay;
                }

                // Update action type
                if (cmbActionType.SelectedItem != null)
                {
                    if (Enum.TryParse<ActionType>(cmbActionType.SelectedItem.ToString(), out ActionType actionType))
                    {
                        action.Type = actionType;
                    }
                }

                // Update key (for keyboard actions)
                if (txtKey.Text.Length > 0)
                {
                    try
                    {
                        Keys key = (Keys)Enum.Parse(typeof(Keys), txtKey.Text, true);
                        action.KeyCode = (int)key;
                    }
                    catch
                    {
                        // Keep previous value if invalid
                    }
                }
                
                // Refresh the display to show updated action
                RefreshActionsDisplay();
                HighlightAction(_selectedActionIndex);
                
                MessageBox.Show("Acción actualizada correctamente.", "Éxito");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Show original JSON in the editor
        /// </summary>
        private void ShowOriginalJSON()
        {
            if (_actionsPanel == null) return;
            
            if (_currentMacro != null)
            {
                // Generate JSON representation of the macro
                string json = System.Text.Json.JsonSerializer.Serialize(_currentMacro, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                MessageBox.Show(json, "JSON de la Macro", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No hay macro cargada para mostrar JSON", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Edit JSON directly
        /// </summary>
        private void EditJSON()
        {
            if (_actionsPanel == null) return;
            
            MessageBox.Show("Edición de JSON no disponible en la interfaz de botones. Use el editor de acciones individuales.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Validate JSON syntax
        /// </summary>
        private void ValidateJSON()
        {
            if (_actionsPanel == null) return;
            
            MessageBox.Show("Validación de JSON no disponible en la interfaz de botones.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Format JSON with proper indentation
        /// </summary>
        private void FormatJSON()
        {
            if (_actionsPanel == null) return;
            
            MessageBox.Show("Formateo de JSON no disponible en la interfaz de botones.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Load last used macro or prompt to create first
        /// </summary>
        private void LoadLastMacro()
        {
            if (_loadedMacros.Count == 0)
            {
                ShowFirstMacroPrompt();
            }
            else
            {
                var lastMacro = _loadedMacros.OrderByDescending(m => m.LastUsed).FirstOrDefault();
                if (lastMacro != null)
                {
                    LoadMacro(lastMacro);
                }
            }
        }

        /// <summary>
        /// Show prompt to create first macro
        /// </summary>
        private void ShowFirstMacroPrompt()
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
        /// Load a macro for editing
        /// </summary>
        private void LoadMacro(MacroConfig macro)
        {
            _currentMacro = macro;
            RefreshActionsDisplay();
        }

        /// <summary>
        /// Refresh the actions display with buttons
        /// </summary>
        private void RefreshActionsDisplay()
        {
            if (_actionsPanel == null || _currentMacro == null) return;

            // Clear existing buttons
            _actionsPanel.Controls.Clear();
            _actionsPanel.Controls.Add(_actionsScrollBar);

            // Create buttons for each action
            int yPosition = 10;
            int buttonHeight = 70;
            int buttonSpacing = 15;

            for (int i = 0; i < _currentMacro.Actions.Count; i++)
            {
                var action = _currentMacro.Actions[i];
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
            string keyDisplay = GetKeyDisplay(action);
            string actionType = GetActionTypeDisplay(action.Type);
            
            var button = new Button
            {
                Text = $"#{index + 1}  {actionType}\nTecla: {keyDisplay}\nEspera: {action.DelayMs}ms",
                Location = new Point(10, yPosition),
                Size = new Size(_actionsPanel.Width - 50, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = index, // Store action index for easy reference
                Cursor = Cursors.Hand
            };

            // Style the button with better appearance
            button.FlatAppearance.BorderColor = _accentColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, _accentColor.R, _accentColor.G, _accentColor.B);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, _accentColor.R, _accentColor.G, _accentColor.B);

            // Add hover effects
            button.MouseEnter += (s, e) => {
                if (button.BackColor != _accentColor) // Don't change if already selected
                {
                    button.BackColor = Color.FromArgb(30, _accentColor.R, _accentColor.G, _accentColor.B);
                }
            };
            
            button.MouseLeave += (s, e) => {
                if (button.BackColor != _accentColor) // Don't change if already selected
                {
                    button.BackColor = _cardBackColor;
                }
            };

            // Add click event
            button.Click += (s, e) => SelectAction(index);

            return button;
        }

        /// <summary>
        /// Get display name for action
        /// </summary>
        private string GetKeyDisplay(MacroAction action)
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
        private string GetKeyName(int keyCode)
        {
            return ((Keys)keyCode).ToString();
        }

        /// <summary>
        /// Add a new action to the macro
        /// </summary>
        private void AddActionFromText()
        {
            if (_currentMacro == null) return;

            string keyName = PromptInput("Ingrese nombre de tecla (Q, W, A, S, etc.):", "");
            if (!string.IsNullOrEmpty(keyName))
            {
                try
                {
                    Keys key = (Keys)Enum.Parse(typeof(Keys), keyName, true);
                    
                    var newAction = new MacroAction
                    {
                        Type = ActionType.KeyPress,
                        KeyCode = (int)key,
                        DelayMs = 0
                    };
                    
                    _currentMacro.Actions.Add(newAction);
                    RefreshActionsDisplay();
                }
                catch
                {
                    MessageBox.Show("Nombre de tecla inválido.", "Error");
                }
            }
        }

        /// <summary>
        /// Remove the selected action from the macro
        /// </summary>
        private void RemoveLineFromText()
        {
            if (_currentMacro == null || _selectedActionIndex < 0 || _selectedActionIndex >= _currentMacro.Actions.Count) return;

            _currentMacro.Actions.RemoveAt(_selectedActionIndex);
            _selectedActionIndex = -1;
            RefreshActionsDisplay();
        }

        /// <summary>
        /// Duplicate the selected action
        /// </summary>
        private void DuplicateLineFromText()
        {
            if (_currentMacro == null || _selectedActionIndex < 0 || _selectedActionIndex >= _currentMacro.Actions.Count) return;

            var selectedAction = _currentMacro.Actions[_selectedActionIndex];
            var duplicatedAction = new MacroAction
            {
                Type = selectedAction.Type,
                KeyCode = selectedAction.KeyCode,
                X = selectedAction.X,
                Y = selectedAction.Y,
                DelayMs = selectedAction.DelayMs,
                TimestampMs = selectedAction.TimestampMs
            };

            _currentMacro.Actions.Insert(_selectedActionIndex + 1, duplicatedAction);
            RefreshActionsDisplay();
        }

        #region Action Handlers

        private void CreateNewMacro()
        {
            _currentMacro = new MacroConfig { Name = $"Macro_{DateTime.Now:yyyyMMdd_HHmmss}" };
            RefreshActionsDisplay();
        }

        private void StartRecording()
        {
            _currentMacro = new MacroConfig { Name = $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}" };
            _recorder.StartRecording();
        }

        private void SaveCurrentMacro()
        {
            if (_currentMacro == null)
            {
                MessageBox.Show("No macro loaded to save.", "Warning");
                return;
            }

            _currentMacro.LastModified = DateTime.Now;
            _currentMacro.LastUsed = DateTime.Now;
            _settingsManager.SaveMacro(_currentMacro);
            
            // Add to loaded macros if not already there
            if (!_loadedMacros.Any(m => m.Id == _currentMacro.Id))
            {
                _loadedMacros.Add(_currentMacro);
            }
            
            // Refresh UI
            RefreshMacroTree();
            MessageBox.Show("Macro saved successfully.", "Success");
        }

        private void DeleteCurrentMacro()
        {
            if (_currentMacro == null)
            {
                MessageBox.Show("No macro loaded to delete.", "Warning");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Delete macro '{_currentMacro.Name}'?",
                "Confirm",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                _settingsManager.DeleteMacro(_currentMacro.Id);
                _loadedMacros.Remove(_currentMacro);
                _currentMacro = null;
                LoadLastMacro();
            }
        }

        private void ExportMacro()
        {
            if (_currentMacro == null)
            {
                MessageBox.Show("No macro loaded to export.", "Warning");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Macro Files|*.macro|JSON Files|*.json",
                FileName = _currentMacro.Name,
                DefaultExt = ".macro"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _settingsManager.ExportMacro(_currentMacro, sfd.FileName);
                MessageBox.Show("Macro exported successfully.", "Success");
            }
        }

        private void ImportMacro()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Macro Files|*.macro|JSON Files|*.json"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var imported = _settingsManager.ImportMacro(ofd.FileName);
                if (imported != null)
                {
                    _loadedMacros.Add(imported);
                    RefreshMacroTree();
                    LoadMacro(imported);
                    MessageBox.Show("Macro imported successfully.", "Success");
                }
            }
        }

        private void RenameSelectedMacro()
        {
            if (_currentMacro == null) return;

            string newName = PromptInput("Enter new name:", _currentMacro.Name);
            if (!string.IsNullOrEmpty(newName))
            {
                _currentMacro.Name = newName;
                SaveCurrentMacro();
            }
        }

        private void TogglePlayPauseStop()
        {
            // Lógica simplificada - solo play/stop, sin pausa
            if (_btnPlay.Text == "▶️")
            {
                // Iniciar reproducción
                if (_currentMacro?.Actions.Count == 0)
                {
                    MessageBox.Show("No actions to play.", "Warning");
                    return;
                }
                
                // Detener cualquier reproducción en curso
                if (_player.IsPlaying)
                {
                    _player.ForceStop();
                }
                
                // Get loop count (0 = infinite loop, other values = specific count)
                int repeatCount = (int)_numLoopCount.Value;
                _ = _player.PlayAsync(_currentMacro, repeatCount);
                
                _currentPlaybackState = PlaybackState.Playing;
                _btnPlay.Text = "⏹️"; // Cambiar a stop
                _btnPlay.BackColor = Color.FromArgb(244, 67, 54);
            }
            else if (_btnPlay.Text == "⏹️")
            {
                // Detener reproducción
                _player.ForceStop();
                _currentPlaybackState = PlaybackState.Stopped;
                _btnPlay.Text = "▶️"; // Cambiar a play
                _btnPlay.BackColor = Color.FromArgb(33, 150, 243);
            }
        }

        private void PlayCurrentMacro()
        {
            // Este método ahora se maneja a través de TogglePlayPauseStop()
            TogglePlayPauseStop();
        }

        private void StopCurrentMacro()
        {
            _player.ForceStop();
            
            // Reset button states
            _currentPlaybackState = PlaybackState.Stopped;
            _btnPlay.Text = "▶️"; // Volver a play cuando se pare
            _btnPlay.BackColor = Color.FromArgb(33, 150, 243);
        }


        private void UndoAction()
        {
            // Undo functionality not applicable to button-based interface
            MessageBox.Show("Funcionalidad de deshacer no disponible en la interfaz de botones", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RedoAction()
        {
            // Redo functionality not applicable to button-based interface
            MessageBox.Show("Funcionalidad de rehacer no disponible en la interfaz de botones", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CutAction()
        {
            // Cut functionality not applicable to button-based interface
            MessageBox.Show("Funcionalidad de cortar no disponible en la interfaz de botones", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyAction()
        {
            // Copy functionality not applicable to button-based interface
            MessageBox.Show("Funcionalidad de copiar no disponible en la interfaz de botones", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PasteAction()
        {
            // Paste functionality not applicable to button-based interface
            MessageBox.Show("Funcionalidad de pegar no disponible en la interfaz de botones", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SelectAllAction()
        {
            // Select all functionality not applicable to button-based interface
            MessageBox.Show("Funcionalidad de seleccionar todo no disponible en la interfaz de botones", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GenerateMacroText()
        {
            // This method is no longer used as we display actions as buttons
            // Keeping for compatibility but not used
            return "Acciones mostradas como botones";
        }

        private string GetActionTypeDisplay(ActionType type)
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

        private void ParseMacroText(string text)
        {
            _currentMacro.Actions.Clear();
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            string currentKeyName = null;
            int currentDelay = 0;

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();
                
                // Parse new format: "│ Tecla: H"
                if (trimmedLine.Contains("Tecla:"))
                {
                    var keyPart = trimmedLine.Split(new[] { "Tecla:" }, StringSplitOptions.None);
                    if (keyPart.Length > 1)
                    {
                        currentKeyName = keyPart[1].Trim().Split(' ')[0];
                    }
                }
                
                // Parse new format: "│ Espera: 100ms"
                if (trimmedLine.Contains("Espera:"))
                {
                    var delayPart = trimmedLine.Split(new[] { "Espera:" }, StringSplitOptions.None);
                    if (delayPart.Length > 1)
                    {
                        string delayStr = delayPart[1].Trim().Replace("ms", "").Trim();
                        if (int.TryParse(delayStr, out int delay))
                        {
                            currentDelay = delay;
                        }
                    }
                    
                    // Try to create action
                    if (!string.IsNullOrEmpty(currentKeyName))
                    {
                        try
                        {
                            Keys key = (Keys)Enum.Parse(typeof(Keys), currentKeyName, true);
                            var action = new MacroAction
                            {
                                Type = ActionType.KeyPress,
                                KeyCode = (int)key,
                                DelayMs = currentDelay
                            };
                            _currentMacro.Actions.Add(action);
                            currentKeyName = null;
                            currentDelay = 0;
                        }
                        catch
                        {
                            // Skip invalid
                        }
                    }
                }
                
                // Also support old format: "Q -> 0ms"
                var parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                if (parts.Length == 2 && !trimmedLine.Contains("│"))
                {
                    string keyName = parts[0].Trim();
                    string delayStr = parts[1].Trim().Replace("ms", "").Trim();

                    try
                    {
                        Keys key = (Keys)Enum.Parse(typeof(Keys), keyName, true);
                        int delay = int.Parse(delayStr);

                        var action = new MacroAction
                        {
                            Type = ActionType.KeyPress,
                            KeyCode = (int)key,
                            DelayMs = delay
                        };
                        _currentMacro.Actions.Add(action);
                    }
                    catch
                    {
                        // Skip invalid lines
                    }
                }
            }
        }

        private void RefreshMacroTree()
        {
            if (_macroTreeView == null) return;

            _macroTreeView.Nodes.Clear();

            // Add root node
            TreeNode rootNode = new TreeNode("Macros");
            _macroTreeView.Nodes.Add(rootNode);

            // Add all macros sorted by last used
            foreach (var macro in _loadedMacros.OrderByDescending(m => m.LastUsed))
            {
                TreeNode macroNode = new TreeNode(macro.Name) { Tag = macro };
                rootNode.Nodes.Add(macroNode);
            }

            // Expand root
            rootNode.Expand();
        }

        private void OnActionRecorded(object sender, MacroAction action)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnActionRecorded(sender, action)));
                return;
            }
            RefreshActionsDisplay();
        }

        private void OnPlaybackStarted(object sender, EventArgs e)
        {
            // El estado ya se maneja en TogglePlayPauseStop()
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            // Actualizar estado cuando la reproducción termine
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnPlaybackStopped(sender, e)));
                return;
            }
            
            _currentPlaybackState = PlaybackState.Stopped;
            _btnPlay.Text = "▶️";
            _btnPlay.BackColor = Color.FromArgb(33, 150, 243);
        }

        private string PromptInput(string prompt, string defaultValue = "")
        {
            Form form = new Form
            {
                Text = "Input",
                Width = 300,
                Height = 150,
                StartPosition = FormStartPosition.CenterParent,
                Owner = this
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



        /// <summary>
        /// Add a new action
        /// </summary>
        private void AddNewAction()
        {
            if (_currentMacro == null)
            {
                MessageBox.Show("No macro is currently open.", "Info");
                return;
            }

            var newAction = new MacroAction
            {
                Type = ActionType.KeyPress,
                KeyCode = (int)Keys.A,
                DelayMs = 0
            };

            _currentMacro.Actions.Add(newAction);
            _selectedActionIndex = _currentMacro.Actions.Count - 1;
            RefreshActionsDisplay();
            LoadActionToEditor(_selectedActionIndex);
            HighlightAction(_selectedActionIndex);
        }

        /// <summary>
        /// Delete the currently selected action
        /// </summary>
        private void DeleteSelectedAction()
        {
            if (_selectedActionIndex < 0 || _currentMacro == null || _selectedActionIndex >= _currentMacro.Actions.Count)
            {
                MessageBox.Show("Please select an action to delete.", "Info");
                return;
            }

            _currentMacro.Actions.RemoveAt(_selectedActionIndex);
            
            // Adjust selection after deletion
            if (_currentMacro.Actions.Count == 0)
            {
                // No actions left, clear selection
                _selectedActionIndex = -1;
                RefreshActionsDisplay();
                ClearRuleEditor();
            }
            else
            {
                // Adjust selection to stay within bounds
                if (_selectedActionIndex >= _currentMacro.Actions.Count)
                    _selectedActionIndex = _currentMacro.Actions.Count - 1;

                RefreshActionsDisplay();
                LoadActionToEditor(_selectedActionIndex);
                HighlightAction(_selectedActionIndex);
            }
        }

        /// <summary>
        /// Duplicate the currently selected action
        /// </summary>
        private void DuplicateSelectedAction()
        {
            if (_selectedActionIndex < 0 || _currentMacro == null || _selectedActionIndex >= _currentMacro.Actions.Count)
            {
                MessageBox.Show("Please select an action to duplicate.", "Info");
                return;
            }

            var originalAction = _currentMacro.Actions[_selectedActionIndex];
            var duplicateAction = new MacroAction
            {
                Type = originalAction.Type,
                KeyCode = originalAction.KeyCode,
                DelayMs = originalAction.DelayMs
            };

            _currentMacro.Actions.Insert(_selectedActionIndex + 1, duplicateAction);
            _selectedActionIndex = _selectedActionIndex + 1;
            RefreshActionsDisplay();
            LoadActionToEditor(_selectedActionIndex);
            HighlightAction(_selectedActionIndex);
        }

        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                SaveCurrentMacro();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.N && e.Control)
            {
                CreateNewMacro();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.R && e.Control)
            {
                StartRecording();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
    }
}