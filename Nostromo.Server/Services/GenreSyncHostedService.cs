using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Nostromo.Server.Services;

public sealed class GenreSyncHostedService : BackgroundService
{
    private readonly IGenreSyncService _sync;

    public GenreSyncHostedService(IGenreSyncService sync) => _sync = sync;

    protected override Task ExecuteAsync(CancellationToken ct) =>
        _sync.SyncGenresAsync(ct);
}