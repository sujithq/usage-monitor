using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public static class UsageSummaryService
{
    public static IReadOnlyList<UsageSummaryRow> Summarize(
        IEnumerable<UsageRecord> records,
        DateTimeOffset sinceUtc,
        string groupBy)
    {
        Func<UsageRecord, string> keySelector = groupBy.ToLowerInvariant() switch
        {
            "repo" => record => record.RepositoryPath ?? "(unknown)",
            "branch" => record => record.Branch ?? "(unknown)",
            _ => record => record.CommandType,
        };

        return records
            .Where(record => record.TimestampUtc >= sinceUtc)
            .GroupBy(keySelector)
            .Select(group => new UsageSummaryRow(
                Key: group.Key,
                Invocations: group.Count(),
                UsageUnits: group.Sum(item => item.UsageUnits),
                CostUsd: decimal.Round(group.Sum(item => item.CostUsd), 6, MidpointRounding.AwayFromZero)))
            .OrderByDescending(row => row.CostUsd)
            .ThenByDescending(row => row.UsageUnits)
            .ToList();
    }
}
