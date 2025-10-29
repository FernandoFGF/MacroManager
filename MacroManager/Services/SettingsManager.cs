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

                // Find the existing file for this macro (by ID)
                string existingFilePath = FindMacroFile(macro.Id);
                
                // Use the macro name as filename (with .json extension)
                string newFileName = $"{SanitizeFileName(macro.Name)}.json";
                string newFilePath = Path.Combine(_macrosDirectory, newFileName);

                // If the file name has changed, delete the old file
                if (existingFilePath != null && existingFilePath != newFilePath)
                {
                    try
                    {
                        File.Delete(existingFilePath);
                    }
                    catch
                    {
                        // Continue even if deletion fails
                    }
                }

                // Check if there's already a file with this name but different ID
                if (File.Exists(newFilePath))
                {
                    // Load the existing file to check if it's the same macro
                    try
                    {
                        string existingJson = File.ReadAllText(newFilePath);
                        var existingMacro = JsonConvert.DeserializeObject<MacroConfig>(existingJson);
                        
                        // If it's a different macro, add a number suffix
                        if (existingMacro != null && existingMacro.Id != macro.Id)
                        {
                            int counter = 1;
                            string baseName = SanitizeFileName(macro.Name);
                            
                            do
                            {
                                newFileName = $"{baseName}_{counter}.json";
                                newFilePath = Path.Combine(_macrosDirectory, newFileName);
                                counter++;
                            } while (File.Exists(newFilePath));
                        }
                    }
                    catch
                    {
                        // If we can't read the existing file, add a number suffix
                        int counter = 1;
                        string baseName = SanitizeFileName(macro.Name);
                        
                        do
                        {
                            newFileName = $"{baseName}_{counter}.json";
                            newFilePath = Path.Combine(_macrosDirectory, newFileName);
                            counter++;
                        } while (File.Exists(newFilePath));
                    }
                }

                string json = JsonConvert.SerializeObject(macro, _jsonSettings);
                File.WriteAllText(newFilePath, json);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving macro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Find the file path for a macro by its ID
        /// </summary>
        private string FindMacroFile(Guid macroId)
        {
            try
            {
                string[] files = Directory.GetFiles(_macrosDirectory, "*.json");
                
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var macro = JsonConvert.DeserializeObject<MacroConfig>(json);
                        
                        if (macro != null && macro.Id == macroId)
                        {
                            return file;
                        }
                    }
                    catch
                    {
                        // Skip invalid files
                        continue;
                    }
                }
            }
            catch
            {
                // Directory access error
            }
            
            return null;
        }

        /// <summary>
        /// Loads a macro from a file
        /// </summary>
        /// <param name="macroId">ID of the macro to load</param>
        /// <returns>The loaded macro or null if it doesn't exist</returns>
        public MacroConfig LoadMacro(Guid macroId)
        {
            try
            {
                // Search for files that contain this macro ID
                string[] files = Directory.GetFiles(_macrosDirectory, "*.json");
                
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var macro = JsonConvert.DeserializeObject<MacroConfig>(json);
                        
                        if (macro != null && macro.Id == macroId)
                        {
                            return macro;
                        }
                    }
                    catch
                    {
                        // Skip invalid files
                        continue;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading macro: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads all saved macros
        /// </summary>
        /// <returns>List of macros</returns>
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
                        {
                            // Set the display name to the actual filename (without extension)
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            macro.Name = fileName;
                            macros.Add(macro);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading macros: {ex.Message}");
            }

            return macros.OrderBy(m => m.Name).ToList();
        }

        /// <summary>
        /// Deletes a macro
        /// </summary>
        /// <param name="macroId">ID of the macro to delete</param>
        /// <returns>True if deleted successfully</returns>
        public bool DeleteMacro(Guid macroId)
        {
            try
            {
                // Search for files that contain this macro ID
                string[] files = Directory.GetFiles(_macrosDirectory, "*.json");
                
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var macro = JsonConvert.DeserializeObject<MacroConfig>(json);
                        
                        if (macro != null && macro.Id == macroId)
                        {
                            File.Delete(file);
                            return true;
                        }
                    }
                    catch
                    {
                        // Skip invalid files
                        continue;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting macro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exports a macro to a specific file
        /// </summary>
        /// <param name="macro">Macro to export</param>
        /// <param name="filePath">Destination file path</param>
        /// <returns>True if exported successfully</returns>
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
                Console.WriteLine($"Error exporting macro: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Imports a macro from a file
        /// </summary>
        /// <param name="filePath">Path of the file to import</param>
        /// <returns>The imported macro or null if it fails</returns>
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
                    // Generate new ID to avoid conflicts
                    macro.Id = Guid.NewGuid();
                    macro.CreatedDate = DateTime.Now;
                    macro.LastModified = DateTime.Now;

                    SaveMacro(macro);
                }

                return macro;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing macro: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves general application settings
        /// </summary>
        /// <param name="settings">Dictionary with settings</param>
        /// <returns>True if saved successfully</returns>
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
                Console.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads general application settings
        /// </summary>
        /// <returns>Dictionary with settings</returns>
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
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Cleans invalid characters from file names
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
        /// Gets the macros directory path
        /// </summary>
        public string GetMacrosDirectory() => _macrosDirectory;
    }
}
