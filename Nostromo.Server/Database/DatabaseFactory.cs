using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database;

public class DatabaseFactory
{
    private readonly object _dbLock = new();
    private readonly IServiceProvider _serviceProvider;
    private IDatabase _instance;

    public DatabaseFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDatabase Instance
    {
        get
        {
            if (_instance != null) return _instance;

            lock (_dbLock)
            {
                if (_instance == null)
                {
                    var dbContext = _serviceProvider.GetRequiredService<NostromoDbContext>();

                    _instance = new SQLite(dbContext);
                }
                return _instance;
            }
        }
    }
}