using System;
using System.Windows.Forms;
using MacroManager.Services;

namespace MacroManager
{
    /// <summary>
    /// Main application entry point
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable modern visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create and initialize the MVC architecture with dependency injection
            Controller controller = null;
            try
            {
                // Create service instances
                var settingsManager = new SettingsManager();
                var macroRecorder = new MacroRecorder();
                var macroPlayer = new MacroPlayer();
                var uiConfigService = new UIConfigurationService();

                // Create controller with injected dependencies
                controller = new Controller(macroRecorder, macroPlayer, settingsManager, uiConfigService);
                controller.Initialize();
                
                // Start the application with the main form from the view
                Application.Run(controller.GetMainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Cleanup resources
                controller?.Dispose();
            }
        }
    }
}
