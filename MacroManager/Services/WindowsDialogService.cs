using System;
using System.Windows.Forms;

namespace MacroManager.Services
{
	public class WindowsDialogService : IDialogService
	{
		private readonly Func<Form> _ownerProvider;

		public WindowsDialogService(Func<Form> ownerProvider)
		{
			_ownerProvider = ownerProvider;
		}

		public void ShowInfo(string message, string title = "Info")
		{
			MessageBox.Show(_ownerProvider(), message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public void ShowWarning(string message, string title = "Warning")
		{
			MessageBox.Show(_ownerProvider(), message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public void ShowError(string message, string title = "Error")
		{
			MessageBox.Show(_ownerProvider(), message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public DialogResult ShowConfirm(string message, string title = "Confirm")
		{
			return MessageBox.Show(_ownerProvider(), message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		public string PromptInput(string prompt, string defaultValue = "")
		{
			Form form = new Form
			{
				Text = "Input",
				Width = 300,
				Height = 150,
				StartPosition = FormStartPosition.CenterParent,
				Owner = _ownerProvider()
			};

			Label label = new Label { Text = prompt, Top = 20, Left = 20, Width = 250 };
			TextBox textBox = new TextBox { Top = 50, Left = 20, Width = 250, Text = defaultValue };
			Button btn = new Button { Text = "OK", Top = 80, Left = 150, Width = 80, DialogResult = DialogResult.OK };

			form.Controls.Add(label);
			form.Controls.Add(textBox);
			form.Controls.Add(btn);
			form.AcceptButton = btn;

			return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
		}
	}
}


