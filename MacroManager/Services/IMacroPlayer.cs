using System;
using System.Threading.Tasks;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Interface para el servicio de reproducción de macros
    /// </summary>
    public interface IMacroPlayer
    {
        /// <summary>
        /// Indica si actualmente se está reproduciendo una macro
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Indica si actualmente está pausado
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Evento que se dispara cuando comienza la reproducción
        /// </summary>
        event EventHandler PlaybackStarted;

        /// <summary>
        /// Evento que se dispara cuando se detiene la reproducción
        /// </summary>
        event EventHandler PlaybackStopped;

        /// <summary>
        /// Evento que se dispara con el progreso de reproducción
        /// </summary>
        event EventHandler<int> PlaybackProgress;

        /// <summary>
        /// Función opcional para comprobar si la ventana objetivo está activa.
        /// Si devuelve false durante la reproducción, el reproductor puede pausar.
        /// </summary>
        Func<bool> IsTargetActiveFunc { get; set; }

        /// <summary>
        /// Reproduce una macro de forma asíncrona
        /// </summary>
        /// <param name="macro">La configuración de macro a reproducir</param>
        /// <param name="repeatCount">Número de repeticiones (1 = una vez, 0 = infinito)</param>
        Task PlayAsync(MacroConfig macro, int repeatCount = 1);

        /// <summary>
        /// Detiene la reproducción actual
        /// </summary>
        void Stop();

        /// <summary>
        /// Pausa la reproducción actual
        /// </summary>
        void Pause();

        /// <summary>
        /// Reanuda la reproducción pausada
        /// </summary>
        void Resume();

        /// <summary>
        /// Fuerza la detención y limpieza
        /// </summary>
        void ForceStop();
    }
}
