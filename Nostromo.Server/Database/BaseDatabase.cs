using NHibernate;

namespace Nostromo.Server.Database
{
    public abstract class BaseDatabase<T> : IDatabase
    {
        public abstract string Name { get; }
        public abstract int RequiredVersion { get; }

        public abstract void BackupDatabase(string filename);
        public abstract void CreateAndUpdateSchema();
        public abstract void CreateDatabase();
        public abstract ISessionFactory CreateSessionFactory();
        public abstract bool DBExists();
        public abstract void Init();

        protected abstract void Execute(T connection, string command);
        public abstract void PopulateInitialData();
        public abstract bool TestConnection();
    }
}
