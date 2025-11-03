using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    /// <summary>
    /// Main view for the MacroManager application
    /// Handles all UI logic and presentation
    /// </summary>
    public partial class View : IDisposable
    {
        private Controller _controller;
        private Model _model;
        private MainForm _mainForm;

        // UI Controls
        private Panel _actionsPanel;
        private Button _btnPlay;
        private Button _btnRecord;
        private Button _btnStopRecord;
        private NumericUpDown _numLoopCount;
        private ComboBox _cmbTargetWindow;
        private TreeView _macroTreeView;
        private ToolTip _toolTip;
        
        // Rule editor controls
        private TextBox _txtKey;
        private TextBox _txtDelay;
        private ComboBox _cmbActionType;
        
        // Shortcut UI Controls
        private Panel _shortcutActionsPanel;
        // private Button _btnPlayShortcut; // No usado - solo se muestra el campo shortcut
        private Button _btnRecordShortcut;
        private Button _btnStopRecordShortcut;
        // private NumericUpDown _numLoopCountShortcut; // No usado - solo se muestra el campo shortcut
        // private ComboBox _cmbTargetWindowShortcut; // Para funcionalidad futura de ventana objetivo
        private TreeView _shortcutTreeView;
        
        // Shortcut Rule editor controls
        private TextBox _txtKeyShortcut;
        private TextBox _txtDelayShortcut;
        private TextBox _txtHotkeyShortcut; // Campo para el hotkey del shortcut
        private ComboBox _cmbActionTypeShortcut;
        private ComboBox _cmbMacroShortcut; // Selector de macro cuando ActionType es Macro
        private Panel _zonePanelShortcut; // Panel de Zone para acciones de ratón
        private Button _btnZoneShortcut; // Botón Zone
        private Label _lblCoordsShortcut; // Label de coordenadas
        
        // Shortcut playback panel controls
        private CheckBox _chkEnableShortcut; // Checkbox Enable para shortcuts
        
        // Unsaved changes state and UI
        private bool _hasUnsavedChanges = false;
        private Label _unsavedCenterIcon;
        
        // Selected action tracking (multi-selección)
        private int _selectedActionIndex = -1; // último ancla/click
        private List<int> _selectedActionIndices = new List<int>();
        private bool _isDraggingSelection = false;
        private Point _dragStartPoint;
        private Rectangle _dragSelectionRect = Rectangle.Empty;
        private bool _pendingClick = false;
        private int _pendingClickIndex = -1;
        private Point _pendingClickStartPoint;
        
        // Selected shortcut action tracking (multi-selección)
        private int _selectedShortcutActionIndex = -1;
        private List<int> _selectedShortcutActionIndices = new List<int>();
        // Campos para funcionalidad futura de arrastre y selección múltiple:
        // private bool _isDraggingShortcutSelection = false;
        // private Point _dragStartPointShortcut;
        // private Rectangle _dragSelectionRectShortcut = Rectangle.Empty;
        // private bool _pendingClickShortcut = false;
        // private int _pendingClickIndexShortcut = -1;
        // private Point _pendingClickStartPointShortcut;
        
        // Editor event suppression (to avoid false dirty when cargando datos)
        private bool _suppressEditorEvents = false;
        private bool _suppressShortcutEditorEvents = false;
        
        // Hotkey handlers para cada pestaña
        private EventHandler _macroHotkeyHandler;
        private EventHandler _shortcutHotkeyHandler;

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
            _toolTip = new ToolTip();
            SubscribeToModelEvents();
        }

        /// <summary>
        /// Get the main form
        /// </summary>
        public Form GetMainForm()
        {
            return _mainForm;
        }

        // Métodos de implementación se reparten en archivos parciales

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


