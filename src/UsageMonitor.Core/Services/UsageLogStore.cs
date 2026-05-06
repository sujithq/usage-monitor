using System.Text.Json;
using UsageMonitor.Core.Models;

namespace UsageMonitor.Core.Services;

public sealed class UsageLogStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
    };

    private readonly string _filePath;

    public UsageLogStore(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultPath();
    }

    public string FilePath => _filePath;

    public void Append(UsageRecord record)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(record, SerializerOptions);
        File.AppendAllText(_filePath, json + Environment.NewLine);
    }

    public IReadOnlyList<UsageRecord> ReadAll()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var records = new List<UsageRecord>();
        foreach (var line in File.ReadLines(_filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parsed = JsonSerializer.Deserialize<UsageRecord>(line, SerializerOptions);
            if (parsed is not null)
            {
                records.Add(parsed);
            }
        }

        return records;
    }

    private static string GetDefaultPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("USAGE_MONITOR_LOG_PATH");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".usage-monitor", "usage-log.jsonl");
    }
}
