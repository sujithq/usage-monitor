namespace UsageMonitor.Core.Models;

public sealed record UsageSummaryRow(string Key, int Invocations, long UsageUnits, decimal CostUsd);
