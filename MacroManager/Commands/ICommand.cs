using System;

namespace MacroManager.Commands
{
    /// <summary>
    /// Interface para el patrón Command
    /// Permite encapsular operaciones y soportar deshacer/rehacer
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Ejecuta el comando
        /// </summary>
        void Execute();

        /// <summary>
        /// Deshace el comando
        /// </summary>
        void Undo();

        /// <summary>
        /// Indica si el comando puede ser deshecho
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Descripción del comando para mostrar en la UI
        /// </summary>
        string Description { get; }
    }
}
