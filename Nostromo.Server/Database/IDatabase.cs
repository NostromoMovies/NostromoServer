using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Database
{
    public interface IDatabase
    {
        ISessionFactory CreateSessionFactory();
        bool DBExists();
        void CreateDatabase();
        void CreateAndUpdateSchema();
        void BackupDatabase(string filename);
        string Name { get; }
        int RequiredVersion { get; } // may not need yet
        void PopulateInitialData();
        void Init();
        bool TestConnection();
    }
}
