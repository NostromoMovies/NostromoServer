namespace Nostromo.Server.Services;
using System.Collections.Concurrent;

public interface IProgressStore
{
    void UpdateProgress(string jobId, string progress);
    string? GetProgress(string jobId);
    void RemoveProgress(string jobId);
}
public class InMemoryProgressStore : IProgressStore
{
    private readonly ConcurrentDictionary<string, string> _progress = new();

    public void UpdateProgress(string jobId, string progress)
    {
        _progress[jobId] = progress;
    }

    public string? GetProgress(string jobId)
    {
        return _progress.TryGetValue(jobId, out var progress) ? progress : null;
    }

    public void RemoveProgress(string jobId)
    {
        _progress.TryRemove(jobId, out _);
    }
}