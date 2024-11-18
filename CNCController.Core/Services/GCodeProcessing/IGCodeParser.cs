using System.Collections.Generic;
using CNCController.Core.Models;

namespace CNCController.Core.Services.GCodeProcessing
{
    public interface IGCodeParser
    {
        /// <summary>
        /// Parses a G-code file from the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the G-code file.</param>
        /// <returns>A list of parsed GCodeCommands.</returns>
        List<IGCodeCommand> ParseFile(string filePath);

        /// <summary>
        /// Parses a single line of G-code into a command.
        /// </summary>
        /// <param name="line">The G-code line.</param>
        /// <returns>A parsed GCodeCommand.</returns>
        IGCodeCommand ParseLine(string line);

        /// <summary>
        /// Validates a G-code command to check for syntax or formatting errors.
        /// </summary>
        /// <param name="command">The G-code command to validate.</param>
        /// <returns>True if the command is valid; otherwise, false.</returns>
        bool ValidateCommand(IGCodeCommand command);
    }
}