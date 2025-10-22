using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Servicio para grabar acciones del teclado y mouse
    /// Utiliza hooks globales de Windows para capturar eventos
    /// </summary>
    public class MacroRecorder
    {
        // Hooks de Windows para teclado y mouse
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        // Delegados para los hooks
        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private LowLevelProc _keyboardProc;
        private LowLevelProc _mouseProc;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;

        // Estado de grabación
        private bool _isRecording;
        private List<MacroAction> _recordedActions;
        private Stopwatch _recordingTimer;

        // Evento que se dispara cuando se graba una nueva acción
        public event EventHandler<MacroAction> ActionRecorded;

        /// <summary>
        /// Indica si está grabando actualmente
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Obtiene las acciones grabadas hasta el momento
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
        /// Inicia la grabación de acciones
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            _recordedActions.Clear();
            _recordingTimer.Restart();
            _isRecording = true;

            // Instalar hooks globales
            _keyboardHookID = SetHook(_keyboardProc, WH_KEYBOARD_LL);
            _mouseHookID = SetHook(_mouseProc, WH_MOUSE_LL);
        }

        /// <summary>
        /// Detiene la grabación de acciones
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
            _recordingTimer.Stop();

            // Desinstalar hooks
            UnhookWindowsHookEx(_keyboardHookID);
            UnhookWindowsHookEx(_mouseHookID);
            _keyboardHookID = IntPtr.Zero;
            _mouseHookID = IntPtr.Zero;
        }

        /// <summary>
        /// Callback para eventos de teclado
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

                // Determinar tipo de acción según wParam
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
        /// Callback para eventos de mouse
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

                // Determinar tipo de acción según wParam
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
                        // Opcional: grabar movimientos (puede generar muchos eventos)
                        // Descomentar si quieres grabar movimientos de mouse
                        // action.Type = ActionType.MouseMove;
                        // _recordedActions.Add(action);
                        // ActionRecorded?.Invoke(this, action);
                        break;
                }
            }

            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Instala un hook de Windows
        /// </summary>
        private IntPtr SetHook(LowLevelProc proc, int hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // Estructuras y funciones de Windows API
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

        // Importaciones de funciones de Windows API
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
        /// Libera recursos al destruir el objeto
        /// </summary>
        ~MacroRecorder()
        {
            StopRecording();
        }
    }
}
