using FluentAssertions;
using UsageMonitor.Core.Models;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Core.Tests.Services;

public sealed class UsageCostEstimatorTests
{
    [Fact]
    public void EstimateUsd_ShouldCalculateTokenCostFromPromptAndCompletion()
    {
        var result = UsageCostEstimator.EstimateUsd(
            UsageMode.Tokens,
            new UsageComputation(300, "tokens", false),
            inputCostPer1KTokens: 0.003m,
            outputCostPer1KTokens: 0.015m,
            requestCost: 0.01m,
            promptTokens: 200,
            completionTokens: 100);

        result.Should().Be(0.0021m);
    }

    [Fact]
    public void EstimateUsd_ShouldCalculateRequestCostFromRequestUnits()
    {
        var result = UsageCostEstimator.EstimateUsd(
            UsageMode.Requests,
            new UsageComputation(3, "requests", false),
            inputCostPer1KTokens: 0.003m,
            outputCostPer1KTokens: 0.015m,
            requestCost: 0.01m,
            promptTokens: null,
            completionTokens: null);

        result.Should().Be(0.03m);
    }
}
