using NHibernate;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database;

public class DatabaseFactory
{
    private readonly object _sessionLock = new();
    private ISessionFactory _sessionFactory;
    private IDatabase _instance;
    public ISessionFactory SessionFactory
    {
        get
        {
            lock(_sessionLock)
            {
                return _sessionFactory ??= Instance.CreateSessionFactory();
            }
        }
    }

    public IDatabase Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = new SQLite();

            return _instance;
        }
    }
}
