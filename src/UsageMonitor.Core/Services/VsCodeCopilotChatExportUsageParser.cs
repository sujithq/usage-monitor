using System.Text.RegularExpressions;
using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public static partial class VsCodeCopilotChatExportUsageParser
{
    [GeneratedRegex(@"^\s*(?:#+\s*)?(?:user|you)\b", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex UserTurnRegex();

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
