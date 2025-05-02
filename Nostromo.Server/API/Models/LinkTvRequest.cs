namespace Nostromo.Server.API.Models
{
    public class LinkTvRequest
    {
        public int VideoID { get; set; }
        public int TMDBTvID { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
    }
}
