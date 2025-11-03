using System;
using MacroManager.Models;

namespace MacroManager.Commands
{
    /// <summary>
    /// Command to create a new shortcut
    /// </summary>
    public class CreateShortcutCommand : ICommand
    {
        private readonly Model _model;
        private MacroConfig _previousShortcut;

        public bool CanUndo => true;
        public string Description => "Create new shortcut";

        public CreateShortcutCommand(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void Execute()
        {
            _previousShortcut = _model.CurrentShortcut;
            _model.CreateNewShortcut();
        }

        public void Undo()
        {
            if (_previousShortcut != null)
            {
                _model.LoadShortcut(_previousShortcut);
            }
        }
    }
}

