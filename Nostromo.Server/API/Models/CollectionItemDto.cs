namespace Nostromo.Server.API.Models
{
    public class CollectionItemDto
    {
        public int? MovieID { get; set; }
        public int? TvShowID { get; set; }

        public string Title { get; set; }
        public string PosterPath { get; set; }
        public string MediaType { get; set; }

        public bool IsInCollection { get; set; }
        public int CollectionId { get; set; }
    }
}