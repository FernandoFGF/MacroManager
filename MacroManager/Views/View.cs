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
        private TreeView _macroTreeView;
        
        // Rule editor controls
        private TextBox _txtKey;
        private TextBox _txtDelay;
        private ComboBox _cmbActionType;
        
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
        
        // Editor event suppression (to avoid false dirty when cargando datos)
        private bool _suppressEditorEvents = false;

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


