using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public static class UsageCostEstimator
{
    public static decimal EstimateUsd(
        UsageMode mode,
        UsageComputation computation,
        decimal inputCostPer1KTokens,
        decimal outputCostPer1KTokens,
        decimal requestCost,
        int? promptTokens,
        int? completionTokens)
    {
        if (mode == UsageMode.Requests)
        {
            return decimal.Round(requestCost * computation.Units, 6, MidpointRounding.AwayFromZero);
        }

        if (promptTokens.HasValue && completionTokens.HasValue)
        {
            var promptCost = (promptTokens.Value / 1000m) * inputCostPer1KTokens;
            var completionCost = (completionTokens.Value / 1000m) * outputCostPer1KTokens;
            return decimal.Round(promptCost + completionCost, 6, MidpointRounding.AwayFromZero);
        }

        var blendedCostPer1KTokens = (inputCostPer1KTokens + outputCostPer1KTokens) / 2m;
        var estimated = (computation.Units / 1000m) * blendedCostPer1KTokens;
        return decimal.Round(estimated, 6, MidpointRounding.AwayFromZero);
    }
}
