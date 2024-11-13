using CNCController.Core.Models;

namespace CNCController.Core.Services.GCodeProcessing
{
    public class GCodeModel
    {
        private readonly List<GCodeCommand> _commands = new();

        public IReadOnlyCollection<GCodeCommand> Commands => _commands.AsReadOnly();

        public GCodeModel AddCommand(GCodeCommand command)
        {
            _commands.Add(command);
            return this;
        }

        public GCodeModel ClearCommands()
        {
            _commands.Clear();
            return this;
        }

        public IEnumerable<GCodeCommand> GetCommandsByType(GCodeCommandType type)
        {
            return _commands.Where(c => c.CommandType == type);
        }
    }
}