using System;
using MacroManager.Models;

namespace MacroManager.Commands
{
    /// <summary>
    /// Command to create a new macro
    /// </summary>
    public class CreateMacroCommand : ICommand
    {
        private readonly Model _model;
        private MacroConfig _previousMacro;

        public bool CanUndo => true;
        public string Description => "Create new macro";

        public CreateMacroCommand(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void Execute()
        {
            _previousMacro = _model.CurrentMacro;
            _model.CreateNewMacro();
        }

        public void Undo()
        {
            if (_previousMacro != null)
            {
                _model.LoadMacro(_previousMacro);
            }
        }
    }
}
