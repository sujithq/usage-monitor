using FluentAssertions;
using UsageMonitor.Core.Models;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Core.Tests.Services;

public sealed class UsageSummaryServiceTests
{
    [Fact]
    public void Summarize_ShouldGroupByRepo()
    {
        var now = DateTimeOffset.UtcNow;
        var records = new[]
        {
            new UsageRecord(now, "gh", "gh copilot ask", "/repo-a", "main", null, UsageMode.Tokens, 100, "tokens", 50, 50, false, 0.001m, 0),
            new UsageRecord(now, "gh", "gh copilot ask", "/repo-a", "main", null, UsageMode.Tokens, 200, "tokens", 100, 100, false, 0.002m, 0),
            new UsageRecord(now, "gh", "gh copilot ask", "/repo-b", "main", null, UsageMode.Tokens, 50, "tokens", 25, 25, false, 0.0005m, 0),
        };

        var summary = UsageSummaryService.Summarize(records, now.AddMinutes(-5), "repo");

        summary.Should().ContainSingle(row => row.Key == "/repo-a" && row.Invocations == 2 && row.UsageUnits == 300);
        summary.Should().ContainSingle(row => row.Key == "/repo-b" && row.Invocations == 1 && row.UsageUnits == 50);
    }
}
