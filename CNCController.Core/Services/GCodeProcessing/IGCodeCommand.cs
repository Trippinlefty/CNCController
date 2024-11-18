using System.Collections.Generic;

namespace CNCController.Core.Services.GCodeProcessing
{
    public interface IGCodeCommand
    {
        /// <summary>
        /// The type of G-code command (e.g., "G0", "G1" for linear moves).
        /// </summary>
        string CommandType { get; }

        /// <summary>
        /// Parameters for the G-code command, such as axis values (X, Y, Z).
        /// </summary>
        Dictionary<string, double> Parameters { get; }

        /// <summary>
        /// Converts the GCodeCommand back to a G-code formatted string.
        /// </summary>
        /// <returns>A formatted G-code string.</returns>
        string ToGCodeString();
    }
}