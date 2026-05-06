using Spectre.Console;
using Spectre.Console.Cli;
using UsageMonitor.Cli.Infrastructure;
using UsageMonitor.Core.Models;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Cli.Commands;

public sealed class TrackCommand : AsyncCommand<TrackCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--tag <TAG>")]
        public string? TaskTag { get; init; }

        [CommandOption("--mode <MODE>")]
        public string Mode { get; init; } = "tokens";

        [CommandOption("--input-cost-per-1k <USD>")]
        public decimal InputCostPer1K { get; init; } = 0.003m;

        [CommandOption("--output-cost-per-1k <USD>")]
        public decimal OutputCostPer1K { get; init; } = 0.015m;

        [CommandOption("--request-cost <USD>")]
        public decimal RequestCost { get; init; } = 0.01m;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var commandParts = context.Remaining.Raw.ToList();
        if (commandParts.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No command provided.[/] Example: [grey]usage-monitor track -- gh copilot ask \"help me\"[/]");
            return 1;
        }

        var mode = string.Equals(settings.Mode, "requests", StringComparison.OrdinalIgnoreCase)
            ? UsageMode.Requests
            : UsageMode.Tokens;

        var strategy = mode == UsageMode.Requests
            ? (IUsageUnitStrategy)new RequestUsageUnitStrategy()
            : new TokenUsageUnitStrategy();

        var (repoRoot, branch) = GitContextProvider.TryGetContext(Environment.CurrentDirectory);

        var commandLine = string.Join(' ', commandParts.Select(QuoteIfNeeded));

        var (exitCode, combinedOutput) = await ProcessRunner.RunAsync(commandParts, cancellationToken);
        var measurement = CopilotOutputUsageParser.Parse(commandLine, combinedOutput);
        var usage = strategy.Compute(measurement);

        var cost = UsageCostEstimator.EstimateUsd(
            mode,
            usage,
            settings.InputCostPer1K,
            settings.OutputCostPer1K,
            settings.RequestCost,
            measurement.PromptTokens,
            measurement.CompletionTokens);

        var record = new UsageRecord(
            TimestampUtc: DateTimeOffset.UtcNow,
            CommandType: commandParts[0],
            CommandLine: commandLine,
            RepositoryPath: repoRoot,
            Branch: branch,
            TaskTag: settings.TaskTag,
            Mode: mode,
            UsageUnits: usage.Units,
            UnitLabel: usage.UnitLabel,
            PromptTokens: measurement.PromptTokens,
            CompletionTokens: measurement.CompletionTokens,
            EstimatedUnits: usage.IsEstimated,
            CostUsd: cost,
            ExitCode: exitCode);

        var store = new UsageLogStore();
        store.Append(record);

        var estimateLabel = usage.IsEstimated ? " (estimated)" : string.Empty;
        AnsiConsole.MarkupLine(
            $"[green]Used {usage.Units:N0} {usage.UnitLabel}{estimateLabel} (~${cost:F6})[/] for [blue]{Markup.Escape(commandParts[0])}[/].");
        AnsiConsole.MarkupLine($"[grey]Log:[/] {Markup.Escape(store.FilePath)}");

        return exitCode;
    }

    private static string QuoteIfNeeded(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal) ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"" : value;
    }
}
