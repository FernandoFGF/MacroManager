using System.Windows.Forms;

namespace MacroManager.Services
{
	public interface IDialogService
	{
		void ShowInfo(string message, string title = "Info");
		void ShowWarning(string message, string title = "Warning");
		void ShowError(string message, string title = "Error");
		DialogResult ShowConfirm(string message, string title = "Confirm");
		string PromptInput(string prompt, string defaultValue = "");
	}
}


