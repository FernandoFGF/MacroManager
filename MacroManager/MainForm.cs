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