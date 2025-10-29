using System;
using System.Collections.Generic;
using System.Linq;

namespace MacroManager.Commands
{
    /// <summary>
    /// Gestor de comandos que maneja el historial de operaciones
    /// Permite deshacer y rehacer acciones
    /// </summary>
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;
        private const int MaxHistorySize = 50;

        public CommandManager()
        {
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
        }

        /// <summary>
        /// Ejecuta un comando y lo agrega al historial
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
                return;

            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Limpiar redo cuando se ejecuta un nuevo comando

            // Limitar el tamaño del historial
            if (_undoStack.Count > MaxHistorySize)
            {
                var commands = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 1; i < commands.Length; i++) // Mantener todos excepto el más antiguo
                {
                    _undoStack.Push(commands[i]);
                }
            }
        }

        /// <summary>
        /// Deshace el último comando ejecutado
        /// </summary>
        public bool Undo()
        {
            if (!CanUndo)
                return false;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            return true;
        }

        /// <summary>
        /// Rehace el último comando deshecho
        /// </summary>
        public bool Redo()
        {
            if (!CanRedo)
                return false;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            return true;
        }

        /// <summary>
        /// Indica si se puede deshacer
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0 && _undoStack.Peek().CanUndo;

        /// <summary>
        /// Indica si se puede rehacer
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Obtiene la descripción del siguiente comando a deshacer
        /// </summary>
        public string GetUndoDescription()
        {
            return CanUndo ? _undoStack.Peek().Description : string.Empty;
        }

        /// <summary>
        /// Obtiene la descripción del siguiente comando a rehacer
        /// </summary>
        public string GetRedoDescription()
        {
            return CanRedo ? _redoStack.Peek().Description : string.Empty;
        }

        /// <summary>
        /// Limpia todo el historial
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
