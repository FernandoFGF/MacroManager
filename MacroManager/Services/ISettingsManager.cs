using System;
using System.Collections.Generic;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Interface para el servicio de gestión de configuraciones
    /// </summary>
    public interface ISettingsManager
    {
        /// <summary>
        /// Guarda una macro en un archivo
        /// </summary>
        /// <param name="macro">La macro a guardar</param>
        /// <returns>True si se guardó exitosamente</returns>
        bool SaveMacro(MacroConfig macro);

        /// <summary>
        /// Carga una macro desde un archivo
        /// </summary>
        /// <param name="macroId">ID de la macro a cargar</param>
        /// <returns>La macro cargada o null si no existe</returns>
        MacroConfig LoadMacro(Guid macroId);

        /// <summary>
        /// Carga todas las macros guardadas
        /// </summary>
        /// <returns>Lista de macros</returns>
        List<MacroConfig> LoadAllMacros();

        /// <summary>
        /// Elimina una macro
        /// </summary>
        /// <param name="macroId">ID de la macro a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        bool DeleteMacro(Guid macroId);

        /// <summary>
        /// Exporta una macro a un archivo específico
        /// </summary>
        /// <param name="macro">Macro a exportar</param>
        /// <param name="filePath">Ruta del archivo destino</param>
        /// <returns>True si se exportó correctamente</returns>
        bool ExportMacro(MacroConfig macro, string filePath);

        /// <summary>
        /// Importa una macro desde un archivo
        /// </summary>
        /// <param name="filePath">Ruta del archivo a importar</param>
        /// <returns>La macro importada o null si falla</returns>
        MacroConfig ImportMacro(string filePath);

        /// <summary>
        /// Guarda configuraciones generales de la aplicación
        /// </summary>
        /// <param name="settings">Diccionario con configuraciones</param>
        /// <returns>True si se guardó correctamente</returns>
        bool SaveSettings(Dictionary<string, object> settings);

        /// <summary>
        /// Carga configuraciones generales de la aplicación
        /// </summary>
        /// <returns>Diccionario con configuraciones</returns>
        Dictionary<string, object> LoadSettings();

        /// <summary>
        /// Obtiene la ruta del directorio de macros
        /// </summary>
        string GetMacrosDirectory();

        /// <summary>
        /// Guarda un shortcut en un archivo
        /// </summary>
        /// <param name="shortcut">El shortcut a guardar</param>
        /// <returns>True si se guardó exitosamente</returns>
        bool SaveShortcut(MacroConfig shortcut);

        /// <summary>
        /// Carga un shortcut desde un archivo
        /// </summary>
        /// <param name="shortcutId">ID del shortcut a cargar</param>
        /// <returns>El shortcut cargado o null si no existe</returns>
        MacroConfig LoadShortcut(Guid shortcutId);

        /// <summary>
        /// Carga todos los shortcuts guardados
        /// </summary>
        /// <returns>Lista de shortcuts</returns>
        List<MacroConfig> LoadAllShortcuts();

        /// <summary>
        /// Elimina un shortcut
        /// </summary>
        /// <param name="shortcutId">ID del shortcut a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        bool DeleteShortcut(Guid shortcutId);

        /// <summary>
        /// Exporta un shortcut a un archivo específico
        /// </summary>
        /// <param name="shortcut">Shortcut a exportar</param>
        /// <param name="filePath">Ruta del archivo destino</param>
        /// <returns>True si se exportó correctamente</returns>
        bool ExportShortcut(MacroConfig shortcut, string filePath);

        /// <summary>
        /// Importa un shortcut desde un archivo
        /// </summary>
        /// <param name="filePath">Ruta del archivo a importar</param>
        /// <returns>El shortcut importado o null si falla</returns>
        MacroConfig ImportShortcut(string filePath);

        /// <summary>
        /// Obtiene la ruta del directorio de shortcuts
        /// </summary>
        string GetShortcutsDirectory();
    }
}
