using CNCController.Core.Models;

namespace CNCController.Core.Services.GCodeProcessing
{
    public class GCodeModel
    {
        private readonly List<GCodeCommand> _commands = new List<GCodeCommand>();

        // Expose commands as a read-only collection
        public IReadOnlyCollection<GCodeCommand> Commands => _commands.AsReadOnly();

        // Adds a command, optionally returns `this` for method chaining
        public GCodeModel AddCommand(GCodeCommand command)
        {
            _commands.Add(command);
            return this;
        }

        // Clears all commands, optionally returns `this` for method chaining
        public GCodeModel ClearCommands()
        {
            _commands.Clear();
            return this;
        }
    }
}