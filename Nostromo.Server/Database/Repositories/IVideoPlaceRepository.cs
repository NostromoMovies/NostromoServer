using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Database.Repositories
{
    public interface IVideoPlaceRepository : IRepository<VideoPlace>
    {
        public Task<String>GetVideoFilePathByVideoID(int videoID);
    }
}
