using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Database.Repositories
{
    public class VideoPlaceRepository : Repository<VideoPlace>, IVideoPlaceRepository
    {
        public VideoPlaceRepository(NostromoDbContext context) : base(context)
        {
        }

        public async Task<string> GetVideoFilePathByVideoID(int videoID)
        {
           var vp = await Query().FirstOrDefaultAsync<VideoPlace>(m=>m.VideoID == videoID);

            return vp.FilePath;
        }
    }
}
