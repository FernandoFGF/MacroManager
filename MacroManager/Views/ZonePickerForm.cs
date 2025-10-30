using System;
using System.Drawing;
using System.Windows.Forms;

namespace MacroManager
{
	internal class ZonePickerForm : Form
	{
		public Point SelectedPoint { get; private set; }
		private bool _picked = false;

		public ZonePickerForm()
		{
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			TopMost = true;
			StartPosition = FormStartPosition.Manual;
			BackColor = Color.Gray;
			Opacity = 0.35;
			Cursor = Cursors.Cross;
			KeyPreview = true;
			// Cubrir escritorio virtual (todos los monitores)
			var vs = SystemInformation.VirtualScreen;
			Bounds = new Rectangle(vs.X, vs.Y, vs.Width, vs.Height);

			MouseDown += OnMouseDownPick;
			KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
		}

		private void OnMouseDownPick(object sender, MouseEventArgs e)
		{
			SelectedPoint = Control.MousePosition; // coordenadas absolutas de pantalla
			_picked = true;
			DialogResult = DialogResult.OK;
			Close();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			if (!_picked)
			{
				DialogResult = DialogResult.Cancel;
			}
		}
	}
}



