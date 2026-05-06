# usage-monitor

Real-time Copilot CLI usage monitor built with **.NET 10** and **Spectre.Console**.

## What it does

- Wraps CLI invocations and records usage per command
- Tracks timestamp + repo/branch + optional task tag
- Calculates usage units using an abstraction:
  - `tokens` mode (default)
  - `requests` mode (future billing model)
- Estimates cost in USD using configurable rates
- Stores local history in JSONL (`~/.usage-monitor/usage-log.jsonl`)
- Shows aggregated summaries by command, repo, or branch

## Commands

```bash
# Track a command invocation and log usage
 dotnet run --project src/UsageMonitor.Cli/UsageMonitor.Cli.csproj -- \
   track --tag my-task --mode tokens -- gh copilot ask "explain this bug"

# Show summary grouped by repository
 dotnet run --project src/UsageMonitor.Cli/UsageMonitor.Cli.csproj -- \
   summary --group-by repo --since-days 7 --top 10
```

## Notes

- If token details are present in command output, they are parsed and used directly.
- If token details are unavailable, token usage is estimated from character counts.
- Set `USAGE_MONITOR_LOG_PATH` to override the default log file path.
