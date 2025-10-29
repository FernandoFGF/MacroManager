using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MacroManager.Services
{
    /// <summary>
    /// Servicio para gestionar la configuración de la interfaz de usuario
    /// Separado del Model para mejor separación de responsabilidades
    /// </summary>
    public class UIConfigurationService
    {
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

        // Theme colors
        private Color _panelBackColor;
        private Color _panelForeColor;
        private Color _accentColor;
        private Color _cardBackColor;
        private Color _borderColor;

        /// <summary>
        /// Constructor
        /// </summary>
        public UIConfigurationService()
        {
            LoadUIConfiguration();
            ApplySystemTheme();
        }

        #region Properties

        // UI Configuration Properties
        public int MinWindowWidth => _minWindowWidth;
        public int MinWindowHeight => _minWindowHeight;
        public int DefaultWindowWidth => _defaultWindowWidth;
        public int DefaultWindowHeight => _defaultWindowHeight;
        public double TreeViewPercentage => _treeViewPercentage;
        public double EditorPercentage => _editorPercentage;
        public int PlaybackPanelHeight => _playbackPanelHeight;
        public int MinimumTreeViewWidth => _minimumTreeViewWidth;
        public int MinimumEditorWidth => _minimumEditorWidth;

        // Theme Properties
        public Color PanelBackColor => _panelBackColor;
        public Color PanelForeColor => _panelForeColor;
        public Color AccentColor => _accentColor;
        public Color CardBackColor => _cardBackColor;
        public Color BorderColor => _borderColor;

        #endregion

        #region Configuration Loading

        /// <summary>
        /// Aplica el tema retro verde LCD
        /// </summary>
        private void ApplySystemTheme()
        {
            // Retro green LCD color scheme (ignoring system theme)
            // Dark green background with bright LCD green text
            
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
        /// Carga la configuración de UI desde uiconfig.json
        /// Busca en múltiples ubicaciones: directorio ejecutable, directorio del proyecto
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
            }
        }

        #endregion
    }
}
