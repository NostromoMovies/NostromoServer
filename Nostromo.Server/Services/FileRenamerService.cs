using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Settings;

namespace Nostromo.Server.Services
{

    public interface IFileRenamerService
    {
        public Task<bool> RenameFile(string file, string newName);
    }

    public class FileRenamerService : IFileRenamerService
    {
        private readonly ILogger<IFileRenamerService> _logger;

        public FileRenamerService(ILogger<IFileRenamerService> logger)
        {
            _logger = logger;
        }

        public Task<bool> RenameFile(string file, string newName)
        {
            try
            {
                // Convert to absolute path and remove any quotes
                string absolutePath = Path.GetFullPath(file.Trim('"'));
                _logger.LogInformation($"Attempting to rename: {absolutePath}");

                if (!File.Exists(absolutePath))
                {
                    _logger.LogError($"File doesn't exist: {absolutePath}");
                    return Task.FromResult(false);
                }

                string newAbsolutePath = Path.GetFullPath(newName.Trim('"'));
                File.Move(absolutePath, newAbsolutePath);
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(false);
            }
        }
    }
}