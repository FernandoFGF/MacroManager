using System;
using System.Windows.Forms;

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
            
            // Start the application
            Application.Run(new MainForm());
        }
    }
}
