using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Service to record keyboard and mouse actions
    /// Uses Windows global hooks to capture events
    /// </summary>
    public class MacroRecorder : IMacroRecorder
    {
        // Windows hooks for keyboard and mouse
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        // Delegates for hooks
        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private LowLevelProc _keyboardProc;
        private LowLevelProc _mouseProc;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;

        // Recording state
        private bool _isRecording;
        private List<MacroAction> _recordedActions;
        private Stopwatch _recordingTimer;
        // UI shortcut suppression window to avoid recording Ctrl+R, etc.
        private DateTime _suppressKeysUntilUtc = DateTime.MinValue;
        private HashSet<int> _suppressedVkCodes = new HashSet<int> { 0x11, 0xA2, 0xA3, 0x52, 0x53, 0x4E }; // + 'S' (0x53), 'N' (0x4E)

        // Event raised when a new action is recorded
        public event EventHandler<MacroAction> ActionRecorded;

        /// <summary>
        /// Indicates if currently recording
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Gets the recorded actions so far
        /// </summary>
        public List<MacroAction> RecordedActions => new List<MacroAction>(_recordedActions);

        /// <summary>
        /// Constructor
        /// </summary>
        public MacroRecorder()
        {
            _recordedActions = new List<MacroAction>();
            _recordingTimer = new Stopwatch();
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
        }

        /// <summary>
        /// Starts recording actions
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            _recordedActions.Clear();
            _recordingTimer.Restart();
            _isRecording = true;

            // Evitar registrar las teclas del atajo de inicio (Ctrl+R) durante unos milisegundos
            _suppressKeysUntilUtc = DateTime.UtcNow.AddMilliseconds(400);

            // Install global hooks
            _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
            _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
        }

        /// <summary>
        /// Stops recording actions
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
            _recordingTimer.Stop();

            // Evitar registrar las teclas del atajo de parada (Ctrl+R) durante unos milisegundos
            _suppressKeysUntilUtc = DateTime.UtcNow.AddMilliseconds(400);

            // Uninstall hooks
            try { UnhookWindowsHookEx(_keyboardHookID); } catch { }
            try { UnhookWindowsHookEx(_mouseHookID); } catch { }
            _keyboardHookID = IntPtr.Zero;
            _mouseHookID = IntPtr.Zero;
        }

        /// <summary>
        /// Callback for keyboard events
        /// </summary>
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isRecording)
            {
                KBDLLHOOKSTRUCT kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int vkCode = kbd.vkCode;

                // Ventana corta para ignorar específicamente las teclas del atajo (Ctrl + R)
                if (DateTime.UtcNow <= _suppressKeysUntilUtc && _suppressedVkCodes.Contains(vkCode))
                {
                    return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
                }
                
                MacroAction action = new MacroAction
                {
                    KeyCode = vkCode,
                    TimestampMs = _recordingTimer.ElapsedMilliseconds
                };

                // Determine action type based on wParam
                if (wParam == (IntPtr)0x0100 || wParam == (IntPtr)0x0104) // WM_KEYDOWN / WM_SYSKEYDOWN
                {
                    action.Type = ActionType.KeyDown;
                }
                else if (wParam == (IntPtr)0x0101 || wParam == (IntPtr)0x0105) // WM_KEYUP / WM_SYSKEYUP
                {
                    action.Type = ActionType.KeyUp;
                }

                _recordedActions.Add(action);
                ActionRecorded?.Invoke(this, action);
            }

            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Callback for mouse events
        /// </summary>
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isRecording)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                // Ignorar clics realizados dentro de la propia aplicación (ventana en primer plano del mismo proceso)
                try
                {
                    IntPtr fg = GetForegroundWindow();
                    if (fg != IntPtr.Zero)
                    {
                        uint pid;
                        GetWindowThreadProcessId(fg, out pid);
                        if (pid == (uint)Process.GetCurrentProcess().Id)
                        {
                            // No registrar eventos de ratón de nuestra propia UI
                            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
                        }
                    }
                }
                catch { /* fallback: si algo falla, seguimos grabando */ }
                
                MacroAction action = new MacroAction
                {
                    X = hookStruct.pt.x,
                    Y = hookStruct.pt.y,
                    TimestampMs = _recordingTimer.ElapsedMilliseconds
                };

                // Determine action type based on wParam
                switch ((int)wParam)
                {
                    case 0x0201: // WM_LBUTTONDOWN
                        action.Type = ActionType.MouseLeftDown;
                        _recordedActions.Add(action);
                        ActionRecorded?.Invoke(this, action);
                        break;
                    case 0x0202: // WM_LBUTTONUP
                        action.Type = ActionType.MouseLeftUp;
                        _recordedActions.Add(action);
                        ActionRecorded?.Invoke(this, action);
                        break;
                    case 0x0204: // WM_RBUTTONDOWN
                        action.Type = ActionType.MouseRightDown;
                        _recordedActions.Add(action);
                        ActionRecorded?.Invoke(this, action);
                        break;
                    case 0x0205: // WM_RBUTTONUP
                        action.Type = ActionType.MouseRightUp;
                        _recordedActions.Add(action);
                        ActionRecorded?.Invoke(this, action);
                        break;
                    case 0x0200: // WM_MOUSEMOVE
                        // Optional: record movements (generates many events)
                        // Uncomment if you want to record mouse movements
                        // action.Type = ActionType.MouseMove;
                        // _recordedActions.Add(action);
                        // ActionRecorded?.Invoke(this, action);
                        break;
                }
            }

            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Installs a Windows hook
        /// </summary>
        private IntPtr SetHook(LowLevelProc proc, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }


        // Windows API structures and functions
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Windows API function imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Releases resources when object is destroyed
        /// </summary>
        ~MacroRecorder()
        {
            try { StopRecording(); } catch { }
        }

        public void Dispose()
        {
            try { StopRecording(); } catch { }
            GC.SuppressFinalize(this);
        }
    }
}
