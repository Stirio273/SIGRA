using System.IO;

namespace SIGRA.Services;

public class ImapSeenTracker
{
    private readonly string _storageFilePath;
    private readonly object _lock = new();
    private ulong _lastSeenUid;

    public ImapSeenTracker(string storageFilePath)
    {
        _storageFilePath = storageFilePath;
        _lastSeenUid = LoadLastSeenUidFromFile();
    }

    public ulong LastSeenUid => _lastSeenUid;

    public bool IsSeen(ulong uid) => uid <= _lastSeenUid;

    public void MarkSeen(IEnumerable<ulong> uids)
    {
        lock (_lock)
        {
            foreach (var uid in uids)
            {
                if (uid > _lastSeenUid)
                    _lastSeenUid = uid;
            }

            if (_lastSeenUid > 0)
                PersistLastSeenUidToFile();
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _lastSeenUid = 0;
            PersistLastSeenUidToFile();
        }
    }

    public IEnumerable<ulong> FilterSeen(IEnumerable<ulong> uids)
    {
        foreach (var uid in uids)
        {
            if (!IsSeen(uid))
                yield return uid;
        }
    }

    private ulong LoadLastSeenUidFromFile()
    {
        try
        {
            if (!File.Exists(_storageFilePath))
                return 0;

            var text = File.ReadAllText(_storageFilePath).Trim();
            return ulong.TryParse(text, out var uid) ? uid : 0;
        }
        catch
        {
            return 0;
        }
    }

    private void PersistLastSeenUidToFile()
    {
        try
        {
            var directory = Path.GetDirectoryName(_storageFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_storageFilePath, _lastSeenUid.ToString());
        }
        catch
        {
        }
    }
}
