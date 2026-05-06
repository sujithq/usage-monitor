namespace UsageMonitor.Core.Models;

public sealed record UsageRecord(
    DateTimeOffset TimestampUtc,
    string CommandType,
    string CommandLine,
    string? RepositoryPath,
    string? Branch,
    string? TaskTag,
    UsageMode Mode,
    long UsageUnits,
    string UnitLabel,
    int? PromptTokens,
    int? CompletionTokens,
    bool EstimatedUnits,
    decimal CostUsd,
    int ExitCode);
