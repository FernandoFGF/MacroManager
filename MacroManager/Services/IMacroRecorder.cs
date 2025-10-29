using System;
using System.Collections.Generic;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Interface para el servicio de grabación de macros
    /// </summary>
    public interface IMacroRecorder
    {
        /// <summary>
        /// Indica si actualmente se está grabando
        /// </summary>
        bool IsRecording { get; }

        /// <summary>
        /// Obtiene las acciones grabadas hasta el momento
        /// </summary>
        List<MacroAction> RecordedActions { get; }

        /// <summary>
        /// Evento que se dispara cuando se graba una nueva acción
        /// </summary>
        event EventHandler<MacroAction> ActionRecorded;

        /// <summary>
        /// Inicia la grabación de acciones
        /// </summary>
        void StartRecording();

        /// <summary>
        /// Detiene la grabación de acciones
        /// </summary>
        void StopRecording();
    }
}
