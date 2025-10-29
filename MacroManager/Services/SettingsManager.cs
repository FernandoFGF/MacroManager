using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using MacroManager.Models;

namespace MacroManager.Services
{
    /// <summary>
    /// Service for saving and loading configurations and macros
    /// Uses JSON files for data persistence
    /// </summary>
    public class SettingsManager : ISettingsManager
    {
        private readonly string _macrosDirectory;
        private readonly string _settingsFilePath;

        /// <summary>
        /// JSON serialization configuration
        /// </summary>
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsManager()
        {
            // Determine user data folder
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "MacroManager");

            // Create folders if they don't exist
            _macrosDirectory = Path.Combine(appFolder, "Macros");
            _settingsFilePath = Path.Combine(appFolder, "settings.json");

            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            if (!Directory.Exists(_macrosDirectory))
                Directory.CreateDirectory(_macrosDirectory);
        }

        /// <summary>
        /// Saves a macro to a file
        /// </summary>
        /// <param name="macro">The macro to save</param>
        /// <returns>True if saved successfully</returns>
        public bool SaveMacro(MacroConfig macro)
        {
            try
            {
                if (macro == null)
                    return false;

                macro.LastModified = DateTime.Now;

                string fileName = $"{SanitizeFileName(macro.Name)}_{macro.Id}.json";
                string filePath = Path.Combine(_macrosDirectory, fileName);

                string json = JsonConvert.SerializeObject(macro, _jsonSettings);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving macro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carga una macro desde un archivo
        /// </summary>
        /// <param name="macroId">ID de la macro a cargar</param>
        /// <returns>La macro cargada o null si no existe</returns>
        public MacroConfig LoadMacro(Guid macroId)
        {
            try
            {
                string[] files = Directory.GetFiles(_macrosDirectory, $"*_{macroId}.json");
                
                if (files.Length == 0)
                    return null;

                string json = File.ReadAllText(files[0]);
                return JsonConvert.DeserializeObject<MacroConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar macro: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Carga todas las macros guardadas
        /// </summary>
        /// <returns>Lista de macros</returns>
        public List<MacroConfig> LoadAllMacros()
        {
            var macros = new List<MacroConfig>();

            try
            {
                string[] files = Directory.GetFiles(_macrosDirectory, "*.json");

                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var macro = JsonConvert.DeserializeObject<MacroConfig>(json);
                        
                        if (macro != null)
                            macros.Add(macro);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar macros: {ex.Message}");
            }

            return macros.OrderBy(m => m.Name).ToList();
        }

        /// <summary>
        /// Elimina una macro
        /// </summary>
        /// <param name="macroId">ID de la macro a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        public bool DeleteMacro(Guid macroId)
        {
            try
            {
                string[] files = Directory.GetFiles(_macrosDirectory, $"*_{macroId}.json");

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar macro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exporta una macro a un archivo específico
        /// </summary>
        /// <param name="macro">Macro a exportar</param>
        /// <param name="filePath">Ruta del archivo destino</param>
        /// <returns>True si se exportó correctamente</returns>
        public bool ExportMacro(MacroConfig macro, string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(macro, _jsonSettings);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al exportar macro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Importa una macro desde un archivo
        /// </summary>
        /// <param name="filePath">Ruta del archivo a importar</param>
        /// <returns>La macro importada o null si falla</returns>
        public MacroConfig ImportMacro(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                var macro = JsonConvert.DeserializeObject<MacroConfig>(json);

                if (macro != null)
                {
                    // Generar nuevo ID para evitar conflictos
                    macro.Id = Guid.NewGuid();
                    macro.CreatedDate = DateTime.Now;
                    macro.LastModified = DateTime.Now;

                    SaveMacro(macro);
                }

                return macro;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al importar macro: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Guarda configuraciones generales de la aplicación
        /// </summary>
        /// <param name="settings">Diccionario con configuraciones</param>
        /// <returns>True si se guardó correctamente</returns>
        public bool SaveSettings(Dictionary<string, object> settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, _jsonSettings);
                File.WriteAllText(_settingsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar configuraciones: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carga configuraciones generales de la aplicación
        /// </summary>
        /// <returns>Diccionario con configuraciones</returns>
        public Dictionary<string, object> LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return new Dictionary<string, object>();

                string json = File.ReadAllText(_settingsFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) 
                       ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar configuraciones: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Limpia caracteres inválidos de nombres de archivo
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "macro";

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = new string(fileName.Select(c => 
                invalidChars.Contains(c) ? '_' : c
            ).ToArray());

            return sanitized.Trim();
        }

        /// <summary>
        /// Obtiene la ruta del directorio de macros
        /// </summary>
        public string GetMacrosDirectory() => _macrosDirectory;
    }
}
