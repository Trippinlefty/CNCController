namespace CNCController.Core.Services.GCodeProcessing;

public enum GCodeCommandType
{
    Motion,        // G0, G1, etc.
    SpindleControl, // M3, M5, etc.
    ToolChange,    // T commands
    CoolantControl, // M8, M9, etc.
    Pause,         // M0, M1
    Other          // Any other command
}