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
    public class MacroPlayer : IMacroPlayer
    {
        private bool _isPlaying;
        private bool _isPaused;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEventSlim _pauseEvent;
        private const int MinActionGapMs = 2; // seguridad para evitar bucles sin respiro

        /// <summary>
        /// Indicates if currently playing a macro
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Indicates if currently paused
        /// </summary>
        public bool IsPaused => _isPaused;

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

        public Func<bool> IsTargetActiveFunc { get; set; }

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
            _isPaused = false;
            _cancellationTokenSource = new CancellationTokenSource();
            _pauseEvent = new ManualResetEventSlim(true); // Initially not paused
            PlaybackStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                int iterations = repeatCount == 0 ? int.MaxValue : repeatCount;

                // Ejecutar todo el bucle en un hilo en segundo plano para no capturar el contexto de UI
                await Task.Run(async () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                            break;

                        await PlayMacroOnceAsync(macro.Actions, _cancellationTokenSource.Token).ConfigureAwait(false);

                        PlaybackProgress?.Invoke(this, i + 1);

                        // Pequeña pausa entre repeticiones
                        if (i < iterations - 1)
                        {
                            // Incluso en loop infinito, meter una pausa mínima para no saturar CPU/SO
                            int interDelay = repeatCount == 0 ? 10 : 100;
                            if (interDelay > 0)
                                await Task.Delay(interDelay, _cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                }, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Playback cancelled, expected
            }
            finally
            {
                _isPlaying = false;
                _isPaused = false;
                _pauseEvent?.Dispose();
                _pauseEvent = null;
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Plays a sequence of actions once
        /// </summary>
        private async Task PlayMacroOnceAsync(List<MacroAction> actions, CancellationToken cancellationToken)
        {
            foreach (var action in actions)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Wait for pause to be released
                _pauseEvent.Wait(cancellationToken);
                if (IsTargetActiveFunc != null && !IsTargetActiveFunc())
                {
                    // Auto-pause if target is not active
                    _isPaused = true;
                    _pauseEvent.Reset();
                    _pauseEvent.Wait(cancellationToken);
                }

                // Aplicar Delay configurado o, si es 0, un gap mínimo de seguridad
                int waitMs = action.DelayMs > 0 ? action.DelayMs : MinActionGapMs;
                if (waitMs > 0)
                {
                    await DelayRespectingPause(waitMs, cancellationToken).ConfigureAwait(false);
                }

                // Wait for pause to be released again after delay
                _pauseEvent.Wait(cancellationToken);
                if (IsTargetActiveFunc != null && !IsTargetActiveFunc())
                {
                    _isPaused = true;
                    _pauseEvent.Reset();
                    _pauseEvent.Wait(cancellationToken);
                }

                // Execute the action (pause/cancel aware)
                await ExecuteActionAsync(action, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Delay that reacts quickly to Pause/Resume and cancellation
        /// </summary>
        private async Task DelayRespectingPause(int totalMs, CancellationToken token)
        {
            int remaining = totalMs;
            while (remaining > 0)
            {
                // If paused, block here until resume or cancellation
                _pauseEvent.Wait(token);

                int step = remaining < 50 ? remaining : 50;
                await Task.Delay(step, token).ConfigureAwait(false);
                remaining -= step;
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
                _isPaused = false;
                _pauseEvent?.Set(); // Release any waiting threads
            }
        }

        /// <summary>
        /// Pauses current playback
        /// </summary>
        public void Pause()
        {
            if (_isPlaying && !_isPaused)
            {
                _isPaused = true;
                _pauseEvent?.Reset();
            }
        }

        /// <summary>
        /// Resumes paused playback
        /// </summary>
        public void Resume()
        {
            if (_isPlaying && _isPaused)
            {
                _isPaused = false;
                _pauseEvent?.Set();
            }
        }

        /// <summary>
        /// Force stop and cleanup
        /// </summary>
        public void ForceStop()
        {
            _isPlaying = false;
            _isPaused = false;
            _cancellationTokenSource?.Cancel();
            _pauseEvent?.Set(); // Release any waiting threads
        }

        /// <summary>
        /// Executes a single action
        /// </summary>
        private async Task ExecuteActionAsync(MacroAction action, CancellationToken token)
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
                    await DelayRespectingPause(50, token);
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
                    await DelayRespectingPause(action.DelayMs, token);
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
