using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public sealed class RequestUsageUnitStrategy : IUsageUnitStrategy
{
    public UsageMode Mode => UsageMode.Requests;

    public UsageComputation Compute(UsageMeasurement measurement)
    {
        var units = measurement.RequestCount <= 0 ? 1 : measurement.RequestCount;
        return new UsageComputation(units, "requests", false);
    }
}
