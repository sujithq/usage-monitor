namespace UsageMonitor.Core.Models;

public sealed record UsageMeasurement(
    int? PromptTokens,
    int? CompletionTokens,
    int? TotalTokens,
    int RequestCount,
    int InputCharacters,
    int OutputCharacters);
