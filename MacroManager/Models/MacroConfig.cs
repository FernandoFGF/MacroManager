using System;
using System.Collections.Generic;

namespace MacroManager.Models
{
    /// <summary>
    /// Modelo de datos para una configuración de macro
    /// Representa una secuencia de acciones guardadas que pueden ser reproducidas
    /// </summary>
    public class MacroConfig
    {
        /// <summary>
        /// Identificador único de la macro
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre descriptivo de la macro
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción opcional de lo que hace la macro
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Lista de acciones (eventos) que componen la macro
        /// </summary>
        public List<MacroAction> Actions { get; set; }

        /// <summary>
        /// Tecla de atajo para ejecutar esta macro
        /// </summary>
        public string Hotkey { get; set; }

        /// <summary>
        /// Indica si la macro está habilitada
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Fecha de creación de la macro
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Fecha de última modificación
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public MacroConfig()
        {
            Id = Guid.NewGuid();
            Name = "Nueva Macro";
            Description = string.Empty;
            Actions = new List<MacroAction>();
            Hotkey = string.Empty;
            IsEnabled = true;
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }
    }

    /// <summary>
    /// Representa una acción individual dentro de una macro
    /// </summary>
    public class MacroAction
    {
        /// <summary>
        /// Tipo de acción (Click, KeyPress, KeyDown, KeyUp, MouseMove, Delay)
        /// </summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// Código de tecla o botón del mouse
        /// </summary>
        public int KeyCode { get; set; }

        /// <summary>
        /// Coordenada X para acciones de mouse
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Coordenada Y para acciones de mouse
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Tiempo de espera en milisegundos (para delays)
        /// </summary>
        public int DelayMs { get; set; }

        /// <summary>
        /// Timestamp relativo desde el inicio de la grabación
        /// </summary>
        public long TimestampMs { get; set; }
    }

    /// <summary>
    /// Enumeración de tipos de acciones posibles
    /// </summary>
    public enum ActionType
    {
        KeyDown,        // Tecla presionada
        KeyUp,          // Tecla liberada
        KeyPress,       // Tecla presionada y liberada
        MouseLeftDown,  // Botón izquierdo presionado
        MouseLeftUp,    // Botón izquierdo liberado
        MouseRightDown, // Botón derecho presionado
        MouseRightUp,   // Botón derecho liberado
        MouseMove,      // Movimiento del mouse
        Delay           // Pausa/espera
    }
}
