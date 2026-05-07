using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public sealed class TokenUsageUnitStrategy : IUsageUnitStrategy
{
    public UsageMode Mode => UsageMode.Tokens;

    public UsageComputation Compute(UsageMeasurement measurement)
    {
        var total = measurement.TotalTokens ??
            ((measurement.PromptTokens.HasValue && measurement.CompletionTokens.HasValue)
                ? measurement.PromptTokens.Value + measurement.CompletionTokens.Value
                : (int?)null);

        if (total.HasValue)
        {
            return new UsageComputation(total.Value, "tokens", false);
        }

        var estimatedTokens = (long)Math.Ceiling((measurement.InputCharacters + measurement.OutputCharacters) / 4.0);
        return new UsageComputation(estimatedTokens, "tokens", true);
    }
}
