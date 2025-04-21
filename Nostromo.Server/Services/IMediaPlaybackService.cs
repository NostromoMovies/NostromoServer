
namespace Nostromo.Server.Services
{
    public interface IMediaPlaybackService
    {
        Task<string> GetVideoPath(int videoId);
    }
}