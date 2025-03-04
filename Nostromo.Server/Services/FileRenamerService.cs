using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nostromo.Models;
using Nostromo.Server.API.Models;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Settings;
using System.Runtime.InteropServices;

namespace Nostromo.Server.Services
{

    public interface IFileRenamerService
    {
        public Task<bool> RenameFile(string file, string newName);
        public Task<bool> RenameWithMetadata(int videoPlaceID);
    }

    public class FileRenamerService : IFileRenamerService
    {
        private readonly ILogger<IFileRenamerService> _logger;
        private readonly IMovieRepository _movieRepository;
        private readonly NostromoDbContext _dbContext;

        public FileRenamerService(ILogger<IFileRenamerService> logger, IMovieRepository movieRepository, NostromoDbContext dbContext)
        {
            _logger = logger;
            _movieRepository = movieRepository;
            _dbContext = dbContext;
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

        public async Task<bool> RenameWithMetadata(int videoPlaceID)
        {
            try
            {
                // Fetch the VideoPlace entity
                var videoPlace = await _dbContext.VideoPlaces
                    .FindAsync(videoPlaceID);

                if (videoPlace == null)
                {
                    _logger.LogError($"VideoPlace with ID {videoPlaceID} not found.");
                    return false;
                }

                // Fetch the associated Video entity
                var video = await _dbContext.Videos
                    .FindAsync(videoPlace.VideoID);

                if (video == null)
                {
                    _logger.LogError($"Video with ID {videoPlace.VideoID} not found.");
                    return false;
                }

                // Fetch the CrossRefVideoTMDBMovie entity
                var crossRef = await _dbContext.CrossRefVideoTMDBMovies
                    .FirstOrDefaultAsync(c => c.VideoID == video.VideoID);
                if (crossRef == null)
                {
                    _logger.LogError($"CrossRefVideoTMDBMovie for VideoID {video.VideoID} not found.");
                    return false;
                }

                // Fetch the associated TMDBMovie entity
                var movie = await _movieRepository.GetByIdAsync(crossRef.TMDBMovieID);
                if (movie == null)
                {
                    _logger.LogError($"TMDBMovie with ID {crossRef.TMDBMovieID} not found.");
                    return false;
                }

                // Construct the new file name using Title and ReleaseDate
                string newFileName = $"{movie.Title} - {movie.ReleaseDate:yyyy-MM-dd}{Path.GetExtension(videoPlace.FilePath)}";

                // Get the directory of the original file
                string directory = Path.GetDirectoryName(videoPlace.FilePath);

                // Construct the new full path
                string newFilePath = Path.Combine(directory, newFileName);

                // Rename the file
                return await RenameFile(videoPlace.FilePath, newFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error renaming file with metadata: {e.Message}");
                return false;
            }
        }
    }
}