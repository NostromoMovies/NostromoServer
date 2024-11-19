using Microsoft.EntityFrameworkCore;
using System;

namespace Nostromo.Server.Database
{
    public abstract class BaseDatabase<TContext> where TContext : DbContext
    {
        protected readonly TContext _context;

        public abstract string Name { get; }
        public abstract int RequiredVersion { get; }

        protected BaseDatabase(TContext context)
        {
            _context = context;
        }

        public abstract void BackupDatabase(string filename);

        public virtual async Task CreateAndUpdateSchema()
        {
            await _context.Database.MigrateAsync();
        }

        public virtual async Task CreateDatabase()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        public virtual bool DBExists()
        {
            return _context.Database.CanConnect();
        }

        public abstract void Init();

        protected virtual async Task Execute(string command)
        {
            await _context.Database.ExecuteSqlRawAsync(command);
        }

        public abstract Task PopulateInitialData();

        public virtual async Task<bool> TestConnection()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}