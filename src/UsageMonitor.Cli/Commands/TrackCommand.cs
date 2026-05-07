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
        [CommandOption("--vscode-chat-export <PATH>")]
        public string? VsCodeChatExportPath { get; init; }

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
        var hasVsCodeExport = !string.IsNullOrWhiteSpace(settings.VsCodeChatExportPath);

        if (hasVsCodeExport && commandParts.Count > 0)
        {
            AnsiConsole.MarkupLine("[red]Use either [grey]--vscode-chat-export[/] or a wrapped command, not both.[/]");
            return 1;
        }

        if (!hasVsCodeExport && commandParts.Count == 0)
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
        string commandType;
        string commandLine;
        int exitCode;
        UsageMeasurement measurement;

        if (hasVsCodeExport)
        {
            var exportPath = Path.GetFullPath(settings.VsCodeChatExportPath!);
            if (!File.Exists(exportPath))
            {
                AnsiConsole.MarkupLine($"[red]VS Code chat export not found:[/] {Markup.Escape(exportPath)}");
                return 1;
            }

            var chatExport = await File.ReadAllTextAsync(exportPath, cancellationToken);
            measurement = VsCodeCopilotChatExportUsageParser.Parse(chatExport);
            commandType = "vscode-copilot-chat";
            commandLine = $"vscode-chat-export {QuoteIfNeeded(exportPath)}";
            exitCode = 0;
        }
        else
        {
            commandType = commandParts[0];
            commandLine = string.Join(' ', commandParts.Select(QuoteIfNeeded));

            var processResult = await ProcessRunner.RunAsync(commandParts, cancellationToken);
            exitCode = processResult.ExitCode;
            measurement = CopilotOutputUsageParser.Parse(commandLine, processResult.CombinedOutput);
        }

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
            CommandType: commandType,
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
            $"[green]Used {usage.Units:N0} {usage.UnitLabel}{estimateLabel} (~${cost:F6})[/] for [blue]{Markup.Escape(commandType)}[/].");
        AnsiConsole.MarkupLine($"[grey]Log:[/] {Markup.Escape(store.FilePath)}");

        return exitCode;
    }

    private static string QuoteIfNeeded(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal) ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"" : value;
    }
}
