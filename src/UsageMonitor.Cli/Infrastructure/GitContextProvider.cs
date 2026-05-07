using System.Diagnostics;

namespace UsageMonitor.Cli.Infrastructure;

public static class GitContextProvider
{
    public static (string? RepoRoot, string? Branch) TryGetContext(string workingDirectory)
    {
        return (RunGit("rev-parse --show-toplevel", workingDirectory), RunGit("rev-parse --abbrev-ref HEAD", workingDirectory));
    }

    private static string? RunGit(string arguments, string workingDirectory)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo("git", arguments)
                {
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }
}
