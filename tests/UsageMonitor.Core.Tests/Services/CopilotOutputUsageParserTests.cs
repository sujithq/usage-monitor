using FluentAssertions;
using UsageMonitor.Core.Services;

namespace UsageMonitor.Core.Tests.Services;

public sealed class CopilotOutputUsageParserTests
{
    [Fact]
    public void Parse_ShouldExtractPromptAndCompletionAndTotalTokens()
    {
        const string output = "Prompt tokens: 210\nCompletion tokens: 90\nTotal tokens: 300";

        var measurement = CopilotOutputUsageParser.Parse("gh copilot ask \"hi\"", output);

        measurement.PromptTokens.Should().Be(210);
        measurement.CompletionTokens.Should().Be(90);
        measurement.TotalTokens.Should().Be(300);
    }

    [Fact]
    public void Parse_ShouldCaptureInputAndOutputLengths()
    {
        var measurement = CopilotOutputUsageParser.Parse("abc", "defg");

        measurement.InputCharacters.Should().Be(3);
        measurement.OutputCharacters.Should().Be(4);
    }
}
