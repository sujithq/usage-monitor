using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public interface IUsageUnitStrategy
{
    UsageMode Mode { get; }

    UsageComputation Compute(UsageMeasurement measurement);
}
