using System.IO;

namespace SIGRA.Services;

public class ImapFailedUidsTracker
{
    private readonly string _storageFilePath;
    private readonly object _lock = new();
    private readonly Dictionary<ulong, int> _failedUids;

    public ImapFailedUidsTracker(string storageFilePath)
    {
        _storageFilePath = storageFilePath;
        _failedUids = LoadFailedUidsFromFile();
    }

    public IReadOnlyDictionary<ulong, int> FailedUids => _failedUids;

    public void RecordFailure(ulong uid)
    {
        lock (_lock)
        {
            _failedUids[uid] = _failedUids.GetValueOrDefault(uid) + 1;
            PersistFailedUidsToFile();
        }
    }

    public void RecordSuccess(ulong uid)
    {
        lock (_lock)
        {
            if (_failedUids.Remove(uid))
                PersistFailedUidsToFile();
        }
    }

    public void Remove(ulong uid)
    {
        lock (_lock)
        {
            if (_failedUids.Remove(uid))
                PersistFailedUidsToFile();
        }
    }

    public bool IsRetriable(ulong uid, int maxRetries = 3)
    {
        lock (_lock)
        {
            return _failedUids.TryGetValue(uid, out var count) && count < maxRetries;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _failedUids.Clear();
            PersistFailedUidsToFile();
        }
    }

    private Dictionary<ulong, int> LoadFailedUidsFromFile()
    {
        try
        {
            if (!File.Exists(_storageFilePath))
                return new Dictionary<ulong, int>();

            var lines = File.ReadAllLines(_storageFilePath);
            var dict = new Dictionary<ulong, int>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(':');
                if (parts.Length == 2 && ulong.TryParse(parts[0], out var uid) && int.TryParse(parts[1], out var count))
                {
                    dict[uid] = count;
                }
            }

            return dict;
        }
        catch
        {
            return new Dictionary<ulong, int>();
        }
    }

    private void PersistFailedUidsToFile()
    {
        try
        {
            var directory = Path.GetDirectoryName(_storageFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var lines = _failedUids.Select(kvp => $"{kvp.Key}:{kvp.Value}");
            File.WriteAllLines(_storageFilePath, lines);
        }
        catch
        {
        }
    }
}
