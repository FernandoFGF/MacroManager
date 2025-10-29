using System;
using System.Collections.Generic;
using System.Linq;

namespace MacroManager.Commands
{
    /// <summary>
    /// Command manager that handles operation history
    /// Allows undo and redo actions
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
        /// Executes a command and adds it to history
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
                return;

            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo when executing a new command

            // Limit history size
            if (_undoStack.Count > MaxHistorySize)
            {
                var commands = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 1; i < commands.Length; i++) // Keep all except the oldest
                {
                    _undoStack.Push(commands[i]);
                }
            }
        }

        /// <summary>
        /// Undoes the last executed command
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
        /// Redoes the last undone command
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
        /// Indicates if undo is possible
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0 && _undoStack.Peek().CanUndo;

        /// <summary>
        /// Indicates if redo is possible
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Gets the description of the next command to undo
        /// </summary>
        public string GetUndoDescription()
        {
            return CanUndo ? _undoStack.Peek().Description : string.Empty;
        }

        /// <summary>
        /// Gets the description of the next command to redo
        /// </summary>
        public string GetRedoDescription()
        {
            return CanRedo ? _redoStack.Peek().Description : string.Empty;
        }

        /// <summary>
        /// Clears all history
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
