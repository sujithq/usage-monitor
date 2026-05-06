using Spectre.Console;
using Spectre.Console.Cli;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Cli.Commands;

public sealed class SummaryCommand : Command<SummaryCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--group-by <GROUP>")]
        public string GroupBy { get; init; } = "command";

        [CommandOption("--since-days <DAYS>")]
        public int SinceDays { get; init; } = 7;

        [CommandOption("--top <N>")]
        public int Top { get; init; } = 10;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var store = new UsageLogStore();
        var rows = UsageSummaryService.Summarize(
            store.ReadAll(),
            DateTimeOffset.UtcNow.AddDays(-Math.Abs(settings.SinceDays)),
            settings.GroupBy)
            .Take(Math.Max(settings.Top, 1))
            .ToList();

        if (rows.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No usage entries found for the selected window.[/]");
            return 0;
        }

        var table = new Table().Border(TableBorder.Rounded).Title($"Usage summary (group: {settings.GroupBy})");
        table.AddColumn("Group");
        table.AddColumn("Invocations");
        table.AddColumn("Usage Units");
        table.AddColumn("Cost (USD)");

        foreach (var row in rows)
        {
            table.AddRow(
                Markup.Escape(row.Key),
                row.Invocations.ToString(),
                row.UsageUnits.ToString("N0"),
                row.CostUsd.ToString("F6"));
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
