using FluentAssertions;
using UsageMonitor.Core.Models;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Core.Tests.Services;

public sealed class TokenUsageUnitStrategyTests
{
    [Fact]
    public void Compute_ShouldUseExplicitTotalTokens_WhenProvided()
    {
        var strategy = new TokenUsageUnitStrategy();
        var measurement = new UsageMeasurement(12, 20, 40, 1, 0, 0);

        var result = strategy.Compute(measurement);

        result.Units.Should().Be(40);
        result.IsEstimated.Should().BeFalse();
        result.UnitLabel.Should().Be("tokens");
    }

    [Fact]
    public void Compute_ShouldEstimateTokens_WhenNoTokenCountsArePresent()
    {
        var strategy = new TokenUsageUnitStrategy();
        var measurement = new UsageMeasurement(null, null, null, 1, 41, 39);

        var result = strategy.Compute(measurement);

        result.Units.Should().Be(20);
        result.IsEstimated.Should().BeTrue();
    }
}
