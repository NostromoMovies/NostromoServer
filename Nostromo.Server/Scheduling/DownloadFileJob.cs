using Microsoft.Extensions.Logging;
using Nostromo.Server.FileHelper;
using Nostromo.Server.Scheduling.Jobs;
using Quartz;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

[DisallowConcurrentExecution]
public class DownloadFileJob : BaseJob
{
    public static readonly string URL_KEY = "url";
    public static readonly string PATH_KEY = "path";
    private readonly ILogger<DownloadFileJob> _logger;

    public DownloadFileJob(ILogger<DownloadFileJob> logger)
    {
        _logger = logger;
    }

    public override string Name => "Download File Job";
    public override string Type => "FileProcessing";

    public override async Task ProcessJob()
    {
        var url = Context.JobDetail.JobDataMap.GetString(URL_KEY)
            ?? throw new InvalidOperationException("URL not provided in job data");

        var path = Context.JobDetail.JobDataMap.GetString(PATH_KEY)
            ?? throw new InvalidOperationException("Path not provided in job data");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var fileUrl) || (fileUrl.Scheme != Uri.UriSchemeHttp && fileUrl.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Invalid or unsupported URL provided in job data");
        }

        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            _logger.LogInformation($"Starting file download from: {fileUrl}");
            using (WebClient wc = new WebClient())
            {
                // Log progress to the console.
                wc.DownloadProgressChanged += (s, e) =>
                {
                    _logger.LogInformation($"Download progress: {e.ProgressPercentage}%");
                };

                // Download file asynchronously
                await wc.DownloadFileTaskAsync(fileUrl, path);
            }

            _logger.LogInformation($"File successfully downloaded to: {path}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while downloading the file");
            throw; 
        }
    }
}
