using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MacroManager
{
    /// <summary>
    /// Main application form - Simplified for MVC architecture
    /// This form now only contains basic UI setup, all business logic is in Controller/Model/View
    /// </summary>
    public partial class MainForm : Form
    {
        // Global hotkey support
        public event EventHandler GlobalHotKeyPressed;
        private int _hotKeyId = 0xBEEF; // arbitrary id
        private bool _hotKeyRegistered = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            this.Text = "MacroManager";
            this.BackColor = Color.FromArgb(12, 32, 12);
            this.ForeColor = Color.FromArgb(0, 255, 0);
        }

        // WinAPI for global hotkeys
        private const int WM_HOTKEY = 0x0312;
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        // DWM API for Windows 11 title bar customization
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        public bool RegisterGlobalHotKey(bool ctrl, bool alt, bool shift, bool win, Keys key)
        {
            try
            {
                if (_hotKeyRegistered)
                {
                    UnregisterHotKey(this.Handle, _hotKeyId);
                    _hotKeyRegistered = false;
                }
                if (key == Keys.None) return true; // consider cleared

                uint mods = 0;
                if (ctrl) mods |= MOD_CONTROL;
                if (alt) mods |= MOD_ALT;
                if (shift) mods |= MOD_SHIFT;
                if (win) mods |= MOD_WIN;

                _hotKeyRegistered = RegisterHotKey(this.Handle, _hotKeyId, mods, (uint)key);
                return _hotKeyRegistered;
            }
            catch { return false; }
        }

        public void UnregisterGlobalHotKey()
        {
            try
            {
                if (_hotKeyRegistered)
                {
                    UnregisterHotKey(this.Handle, _hotKeyId);
                    _hotKeyRegistered = false;
                }
            }
            catch { }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            
            try
            {
                // Activar modo oscuro inmersivo de Windows 11
                int darkMode = 1;
                int result = DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, 
                    ref darkMode, sizeof(int));
                
                if (result == 0)
                {
                    // Establecer color del marco y fondo de la barra de título (verde oscuro)
                    // RGB(15, 40, 15) = verde oscuro del tema
                    // Formato COLORREF (BGR): 0x00BBGGRR
                    // RGB(15, 40, 15) -> B=15(0x0F), G=40(0x28), R=15(0x0F) -> 0x000F280F
                    int captionColor = 0x000F280F; // Verde oscuro en formato BGR
                    DwmSetWindowAttribute(this.Handle, DWMWA_CAPTION_COLOR, 
                        ref captionColor, sizeof(int));
                    
                    // Establecer color del texto de la barra de título (verde amarillento/brillante)
                    // RGB(0, 255, 0) = verde brillante del tema
                    // RGB(0, 255, 0) -> B=0(0x00), G=255(0xFF), R=0(0x00) -> 0x0000FF00
                    int textColor = 0x0000FF00; // Verde brillante en formato BGR
                    DwmSetWindowAttribute(this.Handle, DWMWA_TEXT_COLOR, 
                        ref textColor, sizeof(int));
                }
            }
            catch
            {
                // Si falla (por ejemplo, en Windows 10 o anterior), ignorar silenciosamente
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && _hotKeyRegistered)
            {
                GlobalHotKeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }
    }
}