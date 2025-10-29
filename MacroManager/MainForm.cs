using System;
using System.Drawing;
using System.Windows.Forms;

namespace MacroManager
{
    /// <summary>
    /// Main application form - Simplified for MVC architecture
    /// This form now only contains basic UI setup, all business logic is in Controller/Model/View
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            this.Text = "MacroManager - Multi-Tab Editor";
            this.BackColor = Color.FromArgb(12, 32, 12);
            this.ForeColor = Color.FromArgb(0, 255, 0);
        }

    }
}