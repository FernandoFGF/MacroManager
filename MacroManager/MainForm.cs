using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using MacroManager.Models;
using MacroManager.Services;

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
        private RichTextBox _textEditorRtb;
        private Button _btnPlay;
        private Button _btnStop;
        private TreeView _macroTreeView;
        
        // Rule editor controls
        private TextBox _txtKey;
        private TextBox _txtDelay;
        private ComboBox _cmbActionType;

        // Theme colors
        private bool _isDarkMode;
        private Color _panelBackColor;
        private Color _panelForeColor;
        private Color _accentColor;
        private Color _cardBackColor;
        private Color _borderColor;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            this.Text = "MacroManager - Action Editor";
            ApplySystemTheme();
            InitializeServices();
            LoadMacros();
            SetupUI();
        }

        /// <summary>
        /// Detect system theme and apply modern colors
        /// </summary>
        private void ApplySystemTheme()
        {
            _isDarkMode = IsSystemDarkMode();
            
            if (_isDarkMode)
            {
                // Modern dark theme colors
                this.BackColor = Color.FromArgb(18, 18, 18);
                this.ForeColor = Color.FromArgb(255, 255, 255);
                _panelBackColor = Color.FromArgb(25, 25, 25);
                _panelForeColor = Color.FromArgb(255, 255, 255);
                _accentColor = Color.FromArgb(0, 120, 215);
                _cardBackColor = Color.FromArgb(32, 32, 32);
                _borderColor = Color.FromArgb(64, 64, 64);
            }
            else
            {
                // Modern light theme colors
                this.BackColor = Color.FromArgb(248, 248, 248);
                this.ForeColor = Color.FromArgb(32, 32, 32);
                _panelBackColor = Color.FromArgb(255, 255, 255);
                _panelForeColor = Color.FromArgb(32, 32, 32);
                _accentColor = Color.FromArgb(0, 120, 215);
                _cardBackColor = Color.FromArgb(248, 248, 248);
                _borderColor = Color.FromArgb(200, 200, 200);
            }
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
                        object? value = key.GetValue("AppsUseLightTheme");
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
            // Set AutoScaleMode and minimum size
            this.AutoScaleMode = AutoScaleMode.Font;
            this.MinimumSize = new Size(1000, 700);

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
            playbackPanel.Height = 80;
            verticalSplit.Panel1.Controls.Add(playbackPanel); // Add to center panel only

            // 11. Configure splitter distances after all controls are created
            this.Load += (s, e) => {
                // Set horizontal split to 25% for TreeView
                horizontalSplit.SplitterDistance = Math.Max(200, (int)(horizontalSplit.Width * 0.25));
                
                // Set vertical split to 66.66% for Editor
                verticalSplit.SplitterDistance = Math.Max(400, (int)(verticalSplit.Width * 0.6666));
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
                Font = new Font("Segoe UI", 10),
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
        /// Create the text editor panel with switch button (center area)
        /// </summary>
        private void CreateTextEditorWithSwitch(Control parent)
        {
            // Create panel for editor and button
            Panel editorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _panelBackColor
            };

            // Create text editor with modern styling
            _textEditorRtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Cascadia Code", 11),
                Padding = new Padding(15),
                WordWrap = false,
                Text = "// Action list will appear here",
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Create modern switch button
            Button btnSwitch = new Button
            {
                Text = "🔄 JSON ↔ Simplified",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = _accentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSwitch.FlatAppearance.BorderSize = 0;

            btnSwitch.Click += (s, e) => ToggleEditorMode();

            // Add selection change event to update rule editor
            _textEditorRtb.SelectionChanged += (s, e) => UpdateRuleEditor();

            editorPanel.Controls.Add(_textEditorRtb);
            editorPanel.Controls.Add(btnSwitch);

            parent.Controls.Add(editorPanel);
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
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
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
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor
            };
            ruleEditorPanel.Controls.Add(lblKey);

            TextBox txtKey = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Cascadia Code", 10),
                Name = "txtKey",
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            ruleEditorPanel.Controls.Add(txtKey);

            // Modern delay input
            Label lblDelay = new Label
            {
                Text = "⏱️ Delay (ms):",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor
            };
            ruleEditorPanel.Controls.Add(lblDelay);

            TextBox txtDelay = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Cascadia Code", 10),
                Name = "txtDelay",
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            ruleEditorPanel.Controls.Add(txtDelay);

            // Modern action type input
            Label lblActionType = new Label
            {
                Text = "🎯 Action Type:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _panelForeColor,
                BackColor = _panelBackColor
            };
            ruleEditorPanel.Controls.Add(lblActionType);

            ComboBox cmbActionType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Segoe UI", 9),
                Name = "cmbActionType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _cardBackColor,
                ForeColor = _panelForeColor,
                FlatStyle = FlatStyle.Flat
            };
            cmbActionType.Items.AddRange(new[] { "KeyPress", "KeyDown", "KeyUp", "MouseClick", "MouseMove" });
            cmbActionType.SelectedIndex = 0;
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
                Size = new Size(45, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => AddActionFromText();
            topButtonRow.Controls.Add(btnAdd);

            Button btnRemove = new Button
            {
                Text = "➖",
                Location = new Point(65, 5),
                Size = new Size(45, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) => RemoveLineFromText();
            topButtonRow.Controls.Add(btnRemove);

            Button btnDuplicate = new Button
            {
                Text = "📋 Duplicate",
                Location = new Point(120, 5),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDuplicate.FlatAppearance.BorderSize = 0;
            btnDuplicate.Click += (s, e) => DuplicateLineFromText();
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
                Text = "💾 Save Changes",
                Dock = DockStyle.Fill,
                Height = 45,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Margin = new Padding(5),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SaveRuleChanges(txtKey, txtDelay, cmbActionType);
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
                Padding = new Padding(8)
            };

            Label lblPlayback = new Label
            {
                Text = "🎮 Playback Controls: Record, Play, Stop, and Repeat Options",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 5),
                BackColor = _panelBackColor,
                ForeColor = _panelForeColor
            };

            _btnPlay = new Button
            {
                Text = "▶️ Play",
                Width = 100,
                Height = 40,
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnPlay.FlatAppearance.BorderSize = 0;
            _btnPlay.Click += (s, e) => PlayCurrentMacro();

            _btnStop = new Button
            {
                Text = "⏹️ Stop",
                Width = 100,
                Height = 40,
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(5, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnStop.FlatAppearance.BorderSize = 0;
            _btnStop.Click += (s, e) => _player.Stop();

            // Add buttons with FlowLayoutPanel for positioning
            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                AutoSize = false,
                WrapContents = false,
                Padding = new Padding(0)
            };
            flowPanel.Controls.Add(_btnPlay);
            flowPanel.Controls.Add(_btnStop);

            playbackPanel.Controls.Add(flowPanel);
            playbackPanel.Controls.Add(lblPlayback);

            return playbackPanel;
        }

        /// <summary>
        /// Update rule editor when selection changes in text editor
        /// </summary>
        private void UpdateRuleEditor()
        {
            if (_textEditorRtb == null || _txtKey == null) return;

            // Get selected text or current line
            string selectedText = _textEditorRtb.SelectedText;
            if (string.IsNullOrEmpty(selectedText))
            {
                int lineIndex = _textEditorRtb.GetLineFromCharIndex(_textEditorRtb.SelectionStart);
                int lineStart = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex);
                int lineEnd = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex + 1);
                if (lineEnd == -1) lineEnd = _textEditorRtb.Text.Length;
                selectedText = _textEditorRtb.Text.Substring(lineStart, lineEnd - lineStart).Trim();
            }

            // Parse the selected rule (format: "Key -> DelayMs")
            if (selectedText.Contains("->"))
            {
                var parts = selectedText.Split(new[] { "->" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    _txtKey.Text = parts[0].Trim();
                    string delayStr = parts[1].Trim().Replace("ms", "").Trim();
                    _txtDelay.Text = delayStr;
                }
            }
            else
            {
                _txtKey.Text = selectedText;
                _txtDelay.Text = "0";
            }
        }

        /// <summary>
        /// Save rule changes
        /// </summary>
        private void SaveRuleChanges(TextBox txtKey, TextBox txtDelay, ComboBox cmbActionType)
        {
            if (_textEditorRtb == null) return;

            try
            {
                string newRule = $"{txtKey.Text} -> {txtDelay.Text}ms";
                
                // Replace selected text or current line
                if (_textEditorRtb.SelectionLength > 0)
                {
                    _textEditorRtb.SelectedText = newRule;
                }
                else
                {
                    int lineIndex = _textEditorRtb.GetLineFromCharIndex(_textEditorRtb.SelectionStart);
                    int lineStart = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex);
                    int lineEnd = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex + 1);
                    if (lineEnd == -1) lineEnd = _textEditorRtb.Text.Length;
                    
                    _textEditorRtb.Text = _textEditorRtb.Text.Remove(lineStart, lineEnd - lineStart);
                    _textEditorRtb.Text = _textEditorRtb.Text.Insert(lineStart, newRule);
                }
                
                MessageBox.Show("Regla guardada correctamente.", "Éxito");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la regla: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Show original JSON in the editor
        /// </summary>
        private void ShowOriginalJSON()
        {
            if (_textEditorRtb == null) return;
            
            if (_currentMacro != null)
            {
                // Generate JSON representation of the macro
                string json = System.Text.Json.JsonSerializer.Serialize(_currentMacro, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                _textEditorRtb.Text = json;
            }
            else
            {
                _textEditorRtb.Text = "No hay macro cargada para mostrar JSON";
            }
        }

        /// <summary>
        /// Edit JSON directly
        /// </summary>
        private void EditJSON()
        {
            if (_textEditorRtb == null) return;
            
            _textEditorRtb.Text = "Editar JSON aquí...";
            _textEditorRtb.Focus();
        }

        /// <summary>
        /// Validate JSON syntax
        /// </summary>
        private void ValidateJSON()
        {
            if (_textEditorRtb == null) return;
            
            try
            {
                var json = _textEditorRtb.Text;
                System.Text.Json.JsonDocument.Parse(json);
                MessageBox.Show("JSON válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JSON inválido: {ex.Message}", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Format JSON with proper indentation
        /// </summary>
        private void FormatJSON()
        {
            if (_textEditorRtb == null) return;
            
            try
            {
                var json = _textEditorRtb.Text;
                var parsed = System.Text.Json.JsonDocument.Parse(json);
                var formatted = System.Text.Json.JsonSerializer.Serialize(parsed, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                _textEditorRtb.Text = formatted;
                MessageBox.Show("JSON formateado correctamente.", "Formateo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al formatear JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Toggle between JSON original and simplified mode
        /// </summary>
        private void ToggleEditorMode()
        {
            if (_textEditorRtb == null) return;

            if (_textEditorRtb.Text.Contains("JSON") || _textEditorRtb.Text.Contains("{"))
            {
                // Switch to simplified mode
                _textEditorRtb.Text = "Q -> 0ms\nW -> 100ms\nE -> 50ms";
            }
            else
            {
                // Switch to JSON mode
                if (_currentMacro != null)
                {
                    string json = System.Text.Json.JsonSerializer.Serialize(_currentMacro, new System.Text.Json.JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                    _textEditorRtb.Text = json;
                }
                else
                {
                    _textEditorRtb.Text = "{\n  \"Name\": \"Macro_Example\",\n  \"Actions\": [\n    {\n      \"Type\": \"KeyPress\",\n      \"KeyCode\": 81,\n      \"DelayMs\": 0\n    }\n  ]\n}";
                }
            }
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
        /// Refresh the text editor display
        /// </summary>
        private void RefreshActionsDisplay()
        {
            if (_textEditorRtb == null || _currentMacro == null) return;

            _textEditorRtb.Text = GenerateMacroText();
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
        /// Add a new action line to the text editor
        /// </summary>
        private void AddActionFromText()
        {
            if (_textEditorRtb == null || _currentMacro == null) return;

            string keyName = PromptInput("Enter key name (Q, W, A, S, etc.):", "");
            if (!string.IsNullOrEmpty(keyName))
            {
                try
                {
                    Keys key = (Keys)Enum.Parse(typeof(Keys), keyName, true);
                    string newLine = $"{keyName} -> 0ms";
                    
                    if (_textEditorRtb.Text.Length > 0)
                        _textEditorRtb.Text += Environment.NewLine + newLine;
                    else
                        _textEditorRtb.Text = newLine;

                    // Update the macro
                    ParseMacroText(_textEditorRtb.Text);
                }
                catch
                {
                    MessageBox.Show("Invalid key name.", "Error");
                }
            }
        }

        /// <summary>
        /// Remove the current line from text editor
        /// </summary>
        private void RemoveLineFromText()
        {
            if (_textEditorRtb == null || _currentMacro == null) return;

            int cursorPos = _textEditorRtb.SelectionStart;
            int lineIndex = _textEditorRtb.GetLineFromCharIndex(cursorPos);
            int lineStart = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex);
            int lineEnd = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex + 1);

            if (lineEnd == -1)
                lineEnd = _textEditorRtb.Text.Length;

            _textEditorRtb.Text = _textEditorRtb.Text.Remove(lineStart, Math.Min(lineEnd - lineStart, _textEditorRtb.Text.Length - lineStart));
            ParseMacroText(_textEditorRtb.Text);
        }

        /// <summary>
        /// Duplicate the current line in text editor
        /// </summary>
        private void DuplicateLineFromText()
        {
            if (_textEditorRtb == null || _currentMacro == null) return;

            int cursorPos = _textEditorRtb.SelectionStart;
            int lineIndex = _textEditorRtb.GetLineFromCharIndex(cursorPos);
            int lineStart = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex);
            int lineEnd = _textEditorRtb.GetFirstCharIndexFromLine(lineIndex + 1);

            if (lineEnd == -1)
                lineEnd = _textEditorRtb.Text.Length;

            string line = _textEditorRtb.Text.Substring(lineStart, lineEnd - lineStart).TrimEnd();
            _textEditorRtb.Text += Environment.NewLine + line;
            ParseMacroText(_textEditorRtb.Text);
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

        private void PlayCurrentMacro()
        {
            if (_currentMacro?.Actions.Count == 0)
            {
                MessageBox.Show("No actions to play.", "Warning");
                return;
            }

            _ = _player.PlayAsync(_currentMacro);
        }

        private void UndoAction()
        {
            if (_textEditorRtb != null)
                _textEditorRtb.Undo();
        }

        private void RedoAction()
        {
            if (_textEditorRtb != null)
                _textEditorRtb.Redo();
        }

        private void CutAction()
        {
            if (_textEditorRtb != null && _textEditorRtb.SelectionLength > 0)
                _textEditorRtb.Cut();
        }

        private void CopyAction()
        {
            if (_textEditorRtb != null && _textEditorRtb.SelectionLength > 0)
                _textEditorRtb.Copy();
        }

        private void PasteAction()
        {
            if (_textEditorRtb != null)
                _textEditorRtb.Paste();
        }

        private void SelectAllAction()
        {
            if (_textEditorRtb != null)
                _textEditorRtb.SelectAll();
        }

        private string GenerateMacroText()
        {
            if (_currentMacro == null) return "";

            var lines = new List<string>();
            foreach (var action in _currentMacro.Actions)
            {
                string keyDisplay = GetKeyDisplay(action);
                lines.Add($"{keyDisplay} -> {action.DelayMs}ms");
            }
            return string.Join("\r\n", lines);
        }

        private void ParseMacroText(string text)
        {
            // Simple parser: "Q -> 0ms\r\nW -> 100ms\r\nS -> 50ms"
            _currentMacro.Actions.Clear();

            foreach (var line in text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split(new[] { "->" }, StringSplitOptions.None);
                if (parts.Length == 2)
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
            MessageBox.Show("Playback started.", "Info");
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            MessageBox.Show("Playback stopped.", "Info");
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