using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Nostromo.Server.Scheduling.Jobs;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;
using Nostromo.Server.Database.Repositories;

namespace Nostromo.Server.API.Controllers
{ 
    [ApiController]
    [Route("api/media")]
    public class MediaStreamController : ControllerBase
    {
        private readonly MediaPlaybackService _mediaPlaybackService;
        private readonly ILogger _logger;

        public MediaStreamController(MediaPlaybackService mediaPlaybackService, ILogger logger)
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

            var stream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Results.File(stream, "video/x-matroska", enableRangeProcessing: true);
        }
    }
}