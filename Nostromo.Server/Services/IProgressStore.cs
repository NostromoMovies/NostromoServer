using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public interface IProgressStore
{
    void UpdateProgress(string jobId, string filename, int progress);
    (string? filename, int? progress) GetProgress(string jobId);
    void RemoveProgress(string jobId);
    bool HasActiveJobs();
    List<string> GetActiveJobIds();
}

public class InMemoryProgressStore : IProgressStore
{
    private readonly ConcurrentDictionary<string, (string filename, int progress)> _progress = new();
    private readonly ConcurrentBag<string> _activeJobs = new();

    public void UpdateProgress(string jobId, string filename, int progress)
    {
        _progress[jobId] = (filename, progress);

        if (!_activeJobs.Contains(jobId))
        {
            _activeJobs.Add(jobId);
        }
    }

    public (string? filename, int? progress) GetProgress(string jobId)
    {
        return _progress.TryGetValue(jobId, out var value) ? value : (null, null);
    }

    public void RemoveProgress(string jobId)
    {
        _progress.TryRemove(jobId, out _);

        var updatedJobs = _activeJobs.Where(id => id != jobId).ToList();

        _activeJobs.Clear();
        foreach (var job in updatedJobs)
        {
            _activeJobs.Add(job);
        }
    }

    public bool HasActiveJobs()
    {
        return !_progress.IsEmpty;
    }

    public List<string> GetActiveJobIds()
    {
        return _activeJobs.ToList();
    }
}