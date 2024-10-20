using System.Collections.Concurrent;
// --------------------------------------------------------------------------------------------------------------------------
public class MultiFolderWatcher : IDisposable
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
    private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
// --------------------------------------------------------------------------------------------------------------------------
    public void AddFolder(string folderPath, string fileType)
    {
        if (_watchers.ContainsKey(folderPath))
            return;

        var watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = $"*.{fileType}",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Created += OnFileChanged;
        watcher.Changed += OnFileChanged;
        _watchers[folderPath] = watcher;
    }
// --------------------------------------------------------------------------------------------------------------------------
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _changedFiles.Enqueue(e.FullPath);
    }
// --------------------------------------------------------------------------------------------------------------------------
    public void RemoveFolder(string folderPath)
    {
        if (_watchers.TryGetValue(folderPath, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            _watchers.Remove(folderPath);
        }
    }
// --------------------------------------------------------------------------------------------------------------------------
    public IEnumerable<string> GetChangedFiles()
    {
        while (_changedFiles.TryDequeue(out string? file))
        {
            if (file != null)
            {
                yield return file;
            }
        }
    }
// --------------------------------------------------------------------------------------------------------------------------
    public IEnumerable<KeyValuePair<string, string>> GetWatchedFolders()
    {
        return _watchers.Select(w => new KeyValuePair<string, string>(w.Key, w.Value.Filter));
    }
// --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
    }
}
