using System.Text.RegularExpressions;
using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public static partial class VsCodeCopilotChatExportUsageParser
{
    // Matches user-turn headers at line starts (for example "## User" or "User:").
    [GeneratedRegex(@"^\s*(?:#+\s*)?user\b", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex UserTurnRegex();

    /// <summary>
    /// Parses a VS Code Copilot Chat export and estimates usage for logging.
    /// </summary>
    /// <param name="chatExport">The exported chat text.</param>
    /// <returns>
    /// A <see cref="UsageMeasurement"/> with token fields parsed when present, input characters set from export length,
    /// and request count estimated from user-turn markers such as "## User" or "User:".
    /// </returns>
    public static UsageMeasurement Parse(string chatExport)
    {
        var measurement = CopilotOutputUsageParser.Parse("vscode copilot chat", chatExport);
        var requestCount = UserTurnRegex().Matches(chatExport).Count;

        return measurement with
        {
            RequestCount = Math.Max(requestCount, 1),
            InputCharacters = chatExport.Length
        };
    }
}
