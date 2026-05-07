namespace UsageMonitor.Core.Models;

public sealed record UsageComputation(long Units, string UnitLabel, bool IsEstimated);
