using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class WatcherHostedService : IHostedService, IDisposable
{
    private readonly MultiFolderWatcher _watcher;
    private Timer _timer;

    public WatcherHostedService(MultiFolderWatcher watcher)
    {
        _watcher = watcher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        foreach (var file in _watcher.GetChangedFiles())
        {
            Console.WriteLine($"File changed: {file}");
            // Process the file here
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _watcher.Dispose();
    }
}