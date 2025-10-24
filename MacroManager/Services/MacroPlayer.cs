using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Service to play recorded macros
    /// Simulates keyboard and mouse events according to saved sequence
    /// </summary>
    public class MacroPlayer
    {
        private bool _isPlaying;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Indicates if currently playing a macro
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Event raised when playback starts
        /// </summary>
        public event EventHandler PlaybackStarted;

        /// <summary>
        /// Event raised when playback stops
        /// </summary>
        public event EventHandler PlaybackStopped;

        /// <summary>
        /// Event raised with playback progress
        /// </summary>
        public event EventHandler<int> PlaybackProgress;

        /// <summary>
        /// Plays a macro asynchronously
        /// </summary>
        /// <param name="macro">The macro configuration to play</param>
        /// <param name="repeatCount">Number of repetitions (1 = once, 0 = infinite)</param>
        public async Task PlayAsync(MacroConfig macro, int repeatCount = 1)
        {
            if (_isPlaying || macro == null || macro.Actions.Count == 0)
                return;

            _isPlaying = true;
            _cancellationTokenSource = new CancellationTokenSource();
            PlaybackStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                int iterations = repeatCount == 0 ? int.MaxValue : repeatCount;

                for (int i = 0; i < iterations; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    await PlayMacroOnceAsync(macro.Actions, _cancellationTokenSource.Token);
                    
                    PlaybackProgress?.Invoke(this, i + 1);

                    // Small pause between repetitions
                    if (i < iterations - 1)
                        await Task.Delay(100, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Playback cancelled, expected
            }
            finally
            {
                _isPlaying = false;
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Plays a sequence of actions once
        /// </summary>
        private async Task PlayMacroOnceAsync(List<MacroAction> actions, CancellationToken cancellationToken)
        {
            long lastTimestamp = 0;

            foreach (var action in actions)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Calculate delay from last action
                long delay = action.TimestampMs - lastTimestamp;
                if (delay > 0)
                {
                    await Task.Delay((int)delay, cancellationToken);
                }

                // Execute the action
                ExecuteAction(action);

                lastTimestamp = action.TimestampMs;
            }
        }

        /// <summary>
        /// Stops current playback
        /// </summary>
        public void Stop()
        {
            if (_isPlaying)
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// Executes a single action
        /// </summary>
        private void ExecuteAction(MacroAction action)
        {
            switch (action.Type)
            {
                case ActionType.KeyDown:
                    SendKeyDown((ushort)action.KeyCode);
                    break;

                case ActionType.KeyUp:
                    SendKeyUp((ushort)action.KeyCode);
                    break;

                case ActionType.KeyPress:
                    SendKeyDown((ushort)action.KeyCode);
                    Thread.Sleep(50);
                    SendKeyUp((ushort)action.KeyCode);
                    break;

                case ActionType.MouseLeftDown:
                    SendMouseDown(action.X, action.Y, MouseButton.Left);
                    break;

                case ActionType.MouseLeftUp:
                    SendMouseUp(action.X, action.Y, MouseButton.Left);
                    break;

                case ActionType.MouseRightDown:
                    SendMouseDown(action.X, action.Y, MouseButton.Right);
                    break;

                case ActionType.MouseRightUp:
                    SendMouseUp(action.X, action.Y, MouseButton.Right);
                    break;

                case ActionType.MouseMove:
                    SetCursorPosition(action.X, action.Y);
                    break;

                case ActionType.Delay:
                    Thread.Sleep(action.DelayMs);
                    break;
            }
        }

        #region Windows API for input simulation

        private enum MouseButton
        {
            Left,
            Right
        }

        // Constants for SendInput
        private const int INPUT_KEYBOARD = 1;
        private const int INPUT_MOUSE = 0;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Sends a key down event
        /// </summary>
        private void SendKeyDown(ushort keyCode)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = keyCode;
            inputs[0].u.ki.dwFlags = 0;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Sends a key up event
        /// </summary>
        private void SendKeyUp(ushort keyCode)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = keyCode;
            inputs[0].u.ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Sends a mouse button down event
        /// </summary>
        private void SendMouseDown(int x, int y, MouseButton button)
        {
            SetCursorPosition(x, y);

            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].u.mi.dwFlags = button == MouseButton.Left ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_RIGHTDOWN;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Sends a mouse button up event
        /// </summary>
        private void SendMouseUp(int x, int y, MouseButton button)
        {
            SetCursorPosition(x, y);

            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].u.mi.dwFlags = button == MouseButton.Left ? MOUSEEVENTF_LEFTUP : MOUSEEVENTF_RIGHTUP;

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Moves cursor to specific position
        /// </summary>
        private void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        #endregion
    }
}
