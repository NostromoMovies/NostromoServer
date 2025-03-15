using Microsoft.Extensions.Logging;
using Nostromo.Server.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Services
{
    public class MediaPlaybackService : IMediaPlaybackService
    {
        private readonly IVideoPlaceRepository _videoPlaceRepository;
        private readonly ILogger<MediaPlaybackService> _logger;

        public MediaPlaybackService(IVideoPlaceRepository videoPlaceRepository, ILogger<MediaPlaybackService> logger)
        {
            _videoPlaceRepository = videoPlaceRepository;
            _logger = logger;
        }

        public async Task<string> GetVideoPath(int videoId)
        {
            return await _videoPlaceRepository.GetVideoFilePathByVideoID(videoId);
        }
    }
}
