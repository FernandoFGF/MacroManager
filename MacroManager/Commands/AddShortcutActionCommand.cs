using System;
using MacroManager.Models;

namespace MacroManager.Commands
{
    /// <summary>
    /// Command to add an action to the current shortcut
    /// </summary>
    public class AddShortcutActionCommand : ICommand
    {
        private readonly Model _model;
        private MacroAction _addedAction;
        private int _actionIndex;

        public bool CanUndo => true;
        public string Description => "Add shortcut action";

        public AddShortcutActionCommand(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void Execute()
        {
            if (_model.CurrentShortcut == null)
                return;

            var newAction = new MacroAction
            {
                Type = ActionType.KeyPress,
                KeyCode = (int)System.Windows.Forms.Keys.A,
                DelayMs = 0
            };

            if (_model.ValidateAction(newAction))
            {
                _model.CurrentShortcut.Actions.Add(newAction);
                _actionIndex = _model.CurrentShortcut.Actions.Count - 1;
                _addedAction = newAction;
                _model.NotifyCurrentShortcutChanged();
            }
        }

        public void Undo()
        {
            if (_addedAction != null && _model.CurrentShortcut != null && _actionIndex >= 0)
            {
                _model.CurrentShortcut.Actions.RemoveAt(_actionIndex);
                _model.NotifyCurrentShortcutChanged();
            }
        }
    }
}

