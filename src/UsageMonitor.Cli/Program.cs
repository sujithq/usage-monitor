using Spectre.Console.Cli;
using UsageMonitor.Cli.Commands;

var app = new CommandApp();

app.Configure(configuration =>
{
    configuration.SetApplicationName("usage-monitor");
    configuration.AddCommand<TrackCommand>("track")
        .WithDescription("Run a Copilot CLI command and log usage/cost attribution")
        .WithExample(["track", "--", "gh", "copilot", "ask", "explain this error"]);
    configuration.AddCommand<SummaryCommand>("summary")
        .WithDescription("Show historical usage aggregated by command/repo/branch")
        .WithExample(["summary", "--group-by", "repo", "--since-days", "7"]);
});

return app.Run(args);
