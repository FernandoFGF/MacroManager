using System;
using MacroManager.Models;

namespace MacroManager.Commands
{
    /// <summary>
    /// Comando para eliminar una acción de la macro actual
    /// </summary>
    public class DeleteActionCommand : ICommand
    {
        private readonly Model _model;
        private readonly int _actionIndex;
        private MacroAction _deletedAction;

        public bool CanUndo => true;
        public string Description => "Eliminar acción";

        public DeleteActionCommand(Model model, int actionIndex)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _actionIndex = actionIndex;
        }

        public void Execute()
        {
            if (_model.CurrentMacro == null || _actionIndex < 0 || _actionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            _deletedAction = _model.CurrentMacro.Actions[_actionIndex];
            _model.CurrentMacro.Actions.RemoveAt(_actionIndex);
            _model.NotifyCurrentMacroChanged();
        }

        public void Undo()
        {
            if (_deletedAction != null && _model.CurrentMacro != null)
            {
                _model.CurrentMacro.Actions.Insert(_actionIndex, _deletedAction);
                _model.NotifyCurrentMacroChanged();
            }
        }
    }
}
