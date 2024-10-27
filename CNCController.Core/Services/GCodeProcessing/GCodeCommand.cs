namespace CNCController.Core.Services.GCodeProcessing
{
    public class GCodeCommand
    {
        public string CommandText { get; set; }
        public GCodeCommandType CommandType { get; set; }

        // Constructor to create a GCodeCommand instance
        public GCodeCommand(string commandText, GCodeCommandType commandType)
        {
            CommandText = commandText;
            CommandType = commandType;
        }

    }
}