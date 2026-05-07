using System.Text.RegularExpressions;
using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public static partial class CopilotOutputUsageParser
{
    [GeneratedRegex(@"(?:prompt|input)\s*tokens?\D+(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex PromptTokensRegex();

    [GeneratedRegex(@"(?:completion|output)\s*tokens?\D+(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex CompletionTokensRegex();

    [GeneratedRegex(@"(?:total\s*tokens?|tokens?\s*used)\D+(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex TotalTokensRegex();

    public static UsageMeasurement Parse(string commandLine, string output)
    {
        var prompt = TryParseSingle(PromptTokensRegex(), output);
        var completion = TryParseSingle(CompletionTokensRegex(), output);
        var total = TryParseSingle(TotalTokensRegex(), output);

        return new UsageMeasurement(
            PromptTokens: prompt,
            CompletionTokens: completion,
            TotalTokens: total,
            RequestCount: 1,
            InputCharacters: commandLine.Length,
            OutputCharacters: output.Length);
    }

    private static int? TryParseSingle(Regex regex, string input)
    {
        var match = regex.Match(input);
        if (!match.Success)
        {
            return null;
        }

        return int.TryParse(match.Groups[1].Value, out var parsed) ? parsed : null;
    }
}
