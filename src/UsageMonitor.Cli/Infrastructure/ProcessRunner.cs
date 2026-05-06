using System.Diagnostics;
using System.Text;

namespace UsageMonitor.Cli.Infrastructure;

public static class ProcessRunner
{
    public static async Task<(int ExitCode, string CombinedOutput)> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        var outputBuffer = new StringBuilder();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(args[0])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        for (var i = 1; i < args.Count; i++)
        {
            process.StartInfo.ArgumentList.Add(args[i]);
        }

        process.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is null)
            {
                return;
            }

            Console.WriteLine(eventArgs.Data);
            outputBuffer.AppendLine(eventArgs.Data);
        };

        process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is null)
            {
                return;
            }

            Console.Error.WriteLine(eventArgs.Data);
            outputBuffer.AppendLine(eventArgs.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, outputBuffer.ToString());
    }
}
