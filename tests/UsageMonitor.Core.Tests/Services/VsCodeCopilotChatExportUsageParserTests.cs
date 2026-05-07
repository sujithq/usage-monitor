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

    [Fact]
    public void Parse_ShouldFallbackToSingleRequest_ForEmptyExport()
    {
        var measurement = VsCodeCopilotChatExportUsageParser.Parse(string.Empty);

        measurement.RequestCount.Should().Be(1);
        measurement.InputCharacters.Should().Be(0);
    }

    [Fact]
    public void Parse_ShouldFallbackToSingleRequest_WhenOnlyCopilotResponsesExist()
    {
        const string export = """
            ## Copilot
            Here is one response.
            ## Copilot
            Here is another response.
            """;

        var measurement = VsCodeCopilotChatExportUsageParser.Parse(export);

        measurement.RequestCount.Should().Be(1);
    }

    [Fact]
    public void Parse_ShouldIgnoreMidLineUserWords()
    {
        const string export = """
            ## Copilot
            If the user asks for X, do Y.
            another line mentioning user text
            """;

        var measurement = VsCodeCopilotChatExportUsageParser.Parse(export);

        measurement.RequestCount.Should().Be(1);
    }
}
