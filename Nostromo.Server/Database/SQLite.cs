using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Microsoft.Data.Sqlite;
using NHibernate;
using System.Reflection;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Database
{
    public class SQLite: BaseDatabase<SqliteConnection>
    {
        private static string _databasePath;
        public static string DatabasePath
        {
            get
            {
                if (_databasePath != null)
                    return _databasePath;

                var directoryPath = ""; // TODO: provide from settings
            }
        }

        private static string GetDatabaseFilePath()
        {
            throw new NotImplementedException();
        }

        public override string Name => throw new NotImplementedException();

        public override int RequiredVersion => throw new NotImplementedException();

        public override void BackupDatabase(string filename)
        {
            throw new NotImplementedException();
        }

        public override void CreateAndUpdateSchema()
        {
            throw new NotImplementedException();
        }

        public override void CreateDatabase()
        {
            throw new NotImplementedException();
        }

        public override ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard
                .ConnectionString("Data Source=mydatabase.db;"))
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .BuildSessionFactory();
        }

        public override bool DBExists()
        {
            return File.Exists(GetDatabaseFilePath());
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void PopulateInitialData()
        {
            throw new NotImplementedException();
        }

        public override bool TestConnection()
        {
            throw new NotImplementedException();
        }
    }
}
