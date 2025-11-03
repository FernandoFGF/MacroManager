using System;
using MacroManager.Models;

namespace MacroManager.Commands
{
    /// <summary>
    /// Command to delete an action from the current shortcut
    /// </summary>
    public class DeleteShortcutActionCommand : ICommand
    {
        private readonly Model _model;
        private readonly int _actionIndex;
        private MacroAction _deletedAction;

        public bool CanUndo => true;
        public string Description => "Delete shortcut action";

        public DeleteShortcutActionCommand(Model model, int actionIndex)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _actionIndex = actionIndex;
        }

        public void Execute()
        {
            if (_model.CurrentShortcut == null || _actionIndex < 0 || _actionIndex >= _model.CurrentShortcut.Actions.Count)
                return;

            _deletedAction = _model.CurrentShortcut.Actions[_actionIndex];
            _model.CurrentShortcut.Actions.RemoveAt(_actionIndex);
            _model.NotifyCurrentShortcutChanged();
        }

        public void Undo()
        {
            if (_deletedAction != null && _model.CurrentShortcut != null)
            {
                _model.CurrentShortcut.Actions.Insert(_actionIndex, _deletedAction);
                _model.NotifyCurrentShortcutChanged();
            }
        }
    }
}

