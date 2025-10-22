using System;
using System.Windows.Forms;

namespace MacroManager
{
    /// <summary>
    /// Clase principal - Punto de entrada de la aplicación
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Habilitar estilos visuales modernos de Windows
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Iniciar la aplicación con el formulario principal
            Application.Run(new MainForm());
        }
    }
}
