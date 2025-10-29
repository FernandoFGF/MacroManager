using System;

namespace MacroManager.Commands
{
    /// <summary>
    /// Interface for the Command pattern
    /// Allows encapsulating operations and supporting undo/redo
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        void Execute();

        /// <summary>
        /// Undoes the command
        /// </summary>
        void Undo();

        /// <summary>
        /// Indicates if the command can be undone
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Command description to display in the UI
        /// </summary>
        string Description { get; }
    }
}
