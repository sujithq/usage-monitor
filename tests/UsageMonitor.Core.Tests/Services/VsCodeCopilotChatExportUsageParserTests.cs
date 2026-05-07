using FluentAssertions;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Core.Tests.Services;

public sealed class VsCodeCopilotChatExportUsageParserTests
{
    [Fact]
    public void Parse_ShouldCountUserTurnsAsRequests()
    {
        const string export = """
            # Chat
            ## User
            Explain this error
            ## Copilot
            Here is what happened
            ## User
            Suggest a fix
            ## Copilot
            Update dependency X
            """;

        var measurement = VsCodeCopilotChatExportUsageParser.Parse(export);

        measurement.RequestCount.Should().Be(2);
    }

    [Fact]
    public void Parse_ShouldFallbackToSingleRequest_WhenNoUserMarkerExists()
    {
        var measurement = VsCodeCopilotChatExportUsageParser.Parse("just some exported content");

        measurement.RequestCount.Should().Be(1);
    }
}
