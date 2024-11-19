using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Nostromo.Server.Database
{
    public interface IDatabase
    {
        string Name { get; }
        int RequiredVersion { get; }

        bool DBExists();
        void BackupDatabase(string filename);

        // Convert database operations to async
        Task CreateDatabaseAsync();
        Task CreateAndUpdateSchemaAsync();
        Task PopulateInitialDataAsync();
        Task<bool> TestConnectionAsync();

        // Initialization can stay sync if it's just setting up properties
        void Init();
    }
}