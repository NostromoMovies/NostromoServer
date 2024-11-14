using System.Collections.Concurrent;

namespace Nostromo.Server.Utilities.FileSystemWatcher;
// --------------------------------------------------------------------------------------------------------------------------
public class RecoveringFileSystemWatcher : IDisposable
{
    private System.IO.FileSystemWatcher _watcher;
    private readonly TimeSpan _directoryFailedRetryInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _directoryRetryInterval = TimeSpan.FromMinutes(5);
    private readonly string _path;

    public event EventHandler<string> FileAdded;
    public event EventHandler<string> FileDeleted;

    public RecoveringFileSystemWatcher(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!Directory.Exists(path)) throw new ArgumentException(nameof(path) + $" must be a directory that exists: {path}");

        _path = path;
    }

    public void Start()
    {
        if (_watcher is { EnableRaisingEvents: true }) return;
        _watcher ??= InitWatcher();
    }
    public void Dispose()
    {
        if (_watcher != null)
        {
            //process watcher
            _watcher.Dispose();
        }
    }

    private System.IO.FileSystemWatcher InitWatcher()
    {
        var watcher = new System.IO.FileSystemWatcher
        {
            Path = _path,
            NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true,
            InternalBufferSize = 65536, //64KiB
        };

        watcher.Created += (s, e) => FileAdded?.Invoke(this, e.FullPath);
        //watcher.Changed += (s, e) => FileAdded?.Invoke(this, e.FullPath);
        watcher.Deleted += (s, e) => FileDeleted?.Invoke(this, e.FullPath);

        watcher.EnableRaisingEvents = true;

        return watcher;
    }
}