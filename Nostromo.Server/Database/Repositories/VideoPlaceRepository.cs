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
    }
}
