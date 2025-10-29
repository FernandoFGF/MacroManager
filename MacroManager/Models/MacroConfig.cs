using System;
using System.Collections.Generic;

namespace MacroManager.Models
{
    /// <summary>
    /// Data model for a macro configuration
    /// Represents a sequence of saved actions that can be played back
    /// </summary>
    public class MacroConfig
    {
        /// <summary>
        /// Unique identifier for the macro
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Descriptive name for the macro
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of what the macro does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of actions (events) that make up the macro
        /// </summary>
        public List<MacroAction> Actions { get; set; }

        /// <summary>
        /// Hotkey to execute this macro
        /// </summary>
        public string Hotkey { get; set; }

        /// <summary>
        /// Indicates if the macro is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Macro creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Last modification date
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Last used date
        /// </summary>
        public DateTime LastUsed { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MacroConfig()
        {
            Id = Guid.NewGuid();
            Name = "New Macro";
            Description = string.Empty;
            Actions = new List<MacroAction>();
            Hotkey = string.Empty;
            IsEnabled = true;
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
            LastUsed = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Represents an individual action within a macro
    /// </summary>
    public class MacroAction
    {
        /// <summary>
        /// Action type (Click, KeyPress, KeyDown, KeyUp, MouseMove, Delay)
        /// </summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// Key code or mouse button
        /// </summary>
        public int KeyCode { get; set; }

        /// <summary>
        /// X coordinate for mouse actions
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y coordinate for mouse actions
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Wait time in milliseconds (for delays)
        /// </summary>
        public int DelayMs { get; set; }

        /// <summary>
        /// Relative timestamp from the start of recording
        /// </summary>
        public long TimestampMs { get; set; }
    }

    /// <summary>
    /// Enumeration of possible action types
    /// </summary>
    public enum ActionType
    {
        KeyDown,        // Key pressed
        KeyUp,          // Key released
        KeyPress,       // Key pressed and released
        MouseLeftDown,  // Left button pressed
        MouseLeftUp,    // Left button released
        MouseRightDown, // Right button pressed
        MouseRightUp,   // Right button released
        MouseMove,      // Mouse movement
        Delay           // Pause/wait
    }
}
