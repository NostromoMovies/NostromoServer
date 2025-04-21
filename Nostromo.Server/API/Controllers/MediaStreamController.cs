using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Nostromo.Server.Scheduling.Jobs;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database.Repositories;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace Nostromo.Server.API.Controllers
{ 
    [ApiController]
    [Route("api/media")]
    public class MediaStreamController : ControllerBase
    {
        private readonly IMediaPlaybackService _mediaPlaybackService;
        private readonly ILogger<MediaStreamController> _logger;

        public MediaStreamController(IMediaPlaybackService mediaPlaybackService, ILogger<MediaStreamController> logger)
        {
            _mediaPlaybackService = mediaPlaybackService;
            _logger = logger;
        }

        [HttpGet("stream/{videoId}")]
        public async Task<IResult> StreamVideo(int videoId)
        {
            var videoPath = await _mediaPlaybackService.GetVideoPath(videoId);

            if (!System.IO.File.Exists(videoPath))
                return Results.NotFound("Video not found");

            //HACK: store type in database probably instead or something idk this just shouldnt go here
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".mkv"] = "video/x-matroska"; //lol

            if (!provider.TryGetContentType(videoPath, out var mimeType))
                mimeType = "application/octet-stream";

            var stream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Results.File(stream, mimeType, enableRangeProcessing: true);
        }
    }
}