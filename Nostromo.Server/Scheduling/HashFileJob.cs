using Microsoft.Extensions.Logging;
using Nostromo.Server.FileHelper;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;

namespace Nostromo.Server.Scheduling;

[DisallowConcurrentExecution]
public class HashFileJob : BaseJob
{
    public static readonly string FILE_PATH_KEY = "FilePath";
    private readonly ILogger<HashFileJob> _logger;

    public HashFileJob(ILogger<HashFileJob> logger)
    {
        _logger = logger;
    }

    public override string Name => "Hash File Job";
    public override string Type => "FileProcessing";

    public override async Task ProcessJob()
    {
        var filePath = Context.JobDetail.JobDataMap.GetString(FILE_PATH_KEY)
            ?? throw new InvalidOperationException("File path not provided in job data");

        try
        {
            _logger.LogInformation("Starting hash calculation for {FilePath}", filePath);

            var result = await NativeHasher.CalculateHashesAsync(
                filePath,
                (filename, progress) =>
                {
                    _logger.LogDebug("Hashing progress for {FileName}: {Progress}%", filename, progress);
                },
                Context.CancellationToken
            );

            _logger.LogInformation(
                "File hashed successfully: {FilePath}\nMD5: {MD5}\nSHA1: {SHA1}\nCRC32: {CRC32}\nED2K: {ED2K}\nTime: {Time:N2}s",
                filePath,
                result.MD5,
                result.SHA1,
                result.CRC32,
                result.ED2K,
                result.ProcessingTime.TotalSeconds
            );
        }
        catch (OperationCanceledException) when (Context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Hash calculation cancelled for {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing file: {FilePath}", filePath);
            throw;
        }
    }
}