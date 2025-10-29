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

            // Uninstall hooks
            UnhookWindowsHookEx(_keyboardHookID);
            UnhookWindowsHookEx(_mouseHookID);
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
                int vkCode = Marshal.ReadInt32(lParam);
                
                MacroAction action = new MacroAction
                {
                    KeyCode = vkCode,
                    TimestampMs = _recordingTimer.ElapsedMilliseconds
                };

                // Determine action type based on wParam
                if (wParam == (IntPtr)0x0100) // WM_KEYDOWN
                {
                    action.Type = ActionType.KeyDown;
                }
                else if (wParam == (IntPtr)0x0101) // WM_KEYUP
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

        /// <summary>
        /// Releases resources when object is destroyed
        /// </summary>
        ~MacroRecorder()
        {
            StopRecording();
        }
    }
}
