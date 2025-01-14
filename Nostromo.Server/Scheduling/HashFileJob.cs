using Microsoft.Extensions.Logging;
using Nostromo.Server.FileHelper;
using Nostromo.Server.Scheduling.Jobs;
using Nostromo.Server.Database;
using Quartz;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Nostromo.Server.Services;
using Quartz.Logging;

namespace Nostromo.Server.Scheduling;

[DisallowConcurrentExecution]
public class HashFileJob : BaseJob
{
    public static readonly string FILE_PATH_KEY = "FilePath";
    private readonly ILogger<HashFileJob> _logger;
    private readonly NostromoDbContext _dbContext;
    private readonly IProgressStore _progressStore;

    public HashFileJob(ILogger<HashFileJob> logger, NostromoDbContext dbContext,IProgressStore progressStore)
    {
        _logger = logger;
        _dbContext = dbContext;
        _progressStore = progressStore;
    }

    public override string Name => "Hash File Job";
    public override string Type => "FileProcessing";

    public override async Task ProcessJob()
    { 
        var jobId = Context.JobDetail.Key.Name;
        var filePath = Context.JobDetail.JobDataMap.GetString(FILE_PATH_KEY)
            ?? throw new InvalidOperationException("File path not provided in job data");
        _progressStore.UpdateProgress(jobId,"Job Intialized");

        try
        {
            _logger.LogInformation("HashFileJob started for {FilePath}", filePath);

            // Check if file exists
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}. Skipping job.", filePath);
                _progressStore.UpdateProgress(jobId, $"HashFileJob started for {filePath}");
    
                return;
            }

            // Log file details
            var fileInfo = new FileInfo(filePath);
            _logger.LogDebug("File details - Name: {FileName}, Size: {FileSize} bytes, LastModified: {LastModified}",
                fileInfo.Name, fileInfo.Length, fileInfo.LastWriteTime);
           

            _logger.LogInformation("Starting hash calculation for {FilePath}", filePath);
            _progressStore.UpdateProgress(jobId, string.Format("Starting hash calculation for {0}", filePath));


            // Hashing process
            var result = await NativeHasher.CalculateHashesAsync(
                filePath,
                (filename, progress) =>
                {
                    _progressStore.UpdateProgress(jobId, 
                        string.Format("Hashing progress for {0}: {1}%", filename, progress));
                },
                Context.CancellationToken
            );

            _logger.LogInformation(
                "Hash calculation completed for {FilePath}. MD5: {MD5}, SHA1: {SHA1}, CRC32: {CRC32}, ED2K: {ED2K}",
                filePath, result.MD5, result.SHA1, result.CRC32, result.ED2K);
            
            _progressStore.UpdateProgress(jobId, string.Format("Hash calculation completed for {0}", filePath));

            // Check if file already exists in the database
            _logger.LogDebug("Checking database for existing entry for {FilePath}", filePath);
            _progressStore.UpdateProgress(jobId, string.Format("Checking database for existing entry for {0}", filePath));
            var fileName = Path.GetFileName(filePath);
            var existingVideo = await _dbContext.Videos
                .FirstOrDefaultAsync(v => v.FileName == fileName &&
                                          v.ED2K == result.ED2K &&
                                          v.MD5 == result.MD5 &&
                                          v.SHA1 == result.SHA1 &&
                                          v.CRC32 == result.CRC32,
                                      Context.CancellationToken);

            if (existingVideo == null)
            {
                _logger.LogInformation("No existing database entry found for {FilePath}. Adding new entry.", filePath);
                _progressStore.UpdateProgress(jobId, string.Format("No existing database entry found for {0}. Adding new entry.", filePath));

                // Create new video entry
                var video = new Video
                {
                    FileName = fileName,
                    ED2K = result.ED2K,
                    CRC32 = result.CRC32,
                    MD5 = result.MD5,
                    SHA1 = result.SHA1
                };

                _dbContext.Videos.Add(video);
                await _dbContext.SaveChangesAsync(Context.CancellationToken);

                _logger.LogInformation(
                    "File successfully hashed and saved to database: {FilePath}\nMD5: {MD5}\nSHA1: {SHA1}\nCRC32: {CRC32}\nED2K: {ED2K}\nTime: {Time:N2}s",
                    filePath,
                    result.MD5,
                    result.SHA1,
                    result.CRC32,
                    result.ED2K,
                    result.ProcessingTime.TotalSeconds
                );
                _progressStore.UpdateProgress(jobId, string.Format("File successfully hashed and saved to database: {FilePath}\nMD5: {MD5}\nSHA1: {SHA1}\nCRC32: {CRC32}\nED2K: {ED2K}\nTime: {Time:N2}s",
                    filePath,
                    result.MD5,
                    result.SHA1,
                    result.CRC32,
                    result.ED2K,
                    result.ProcessingTime.TotalSeconds));
            }
            else
            {
                _logger.LogInformation(
                    "File hash already exists in the database for {FilePath}. Skipping save.",
                    filePath
                );
                _progressStore.UpdateProgress(jobId, string.Format("File hash already exists in the database for {0}. Skipping save.", filePath));
            }
        }
        catch (OperationCanceledException) when (Context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Hash calculation was cancelled for {FilePath}", filePath);
            _progressStore.UpdateProgress(jobId, string.Format("Hash calculation was cancelled for {0}.", filePath));
            
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing file: {FilePath}", filePath);
            _progressStore.UpdateProgress(jobId, string.Format("An error occurred while processing file: {0}", filePath));
            throw;
        }
    }
}
