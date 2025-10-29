using System;
using MacroManager.Models;

namespace MacroManager.Commands
{
    /// <summary>
    /// Command to add an action to the current macro
    /// </summary>
    public class AddActionCommand : ICommand
    {
        private readonly Model _model;
        private MacroAction _addedAction;
        private int _actionIndex;

        public bool CanUndo => true;
        public string Description => "Add action";

        public AddActionCommand(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void Execute()
        {
            if (_model.CurrentMacro == null)
                return;

            var newAction = new MacroAction
            {
                Type = ActionType.KeyPress,
                KeyCode = (int)System.Windows.Forms.Keys.A,
                DelayMs = 0
            };

            if (_model.ValidateAction(newAction))
            {
                _model.CurrentMacro.Actions.Add(newAction);
                _actionIndex = _model.CurrentMacro.Actions.Count - 1;
                _addedAction = newAction;
                _model.NotifyCurrentMacroChanged();
            }
        }

        public void Undo()
        {
            if (_addedAction != null && _model.CurrentMacro != null && _actionIndex >= 0)
            {
                _model.CurrentMacro.Actions.RemoveAt(_actionIndex);
                _model.NotifyCurrentMacroChanged();
            }
        }
    }
}
