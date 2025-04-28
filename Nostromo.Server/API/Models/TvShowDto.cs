using System.Collections.Generic; // List
namespace Nostromo.Models;

public class TvShowDto
{
    public int TvShowID { get; set; }
    public string OriginalName { get; set; }
    public string PosterPath { get; set; }
    public string BackdropPath { get; set; }
    public string Overview { get; set; }
    public DateTime? FirstAirDate { get; set; }

    public int? CollectionId { get; set; }
    public bool IsInCollection { get; set; }

    public double Popularity { get; set; }
    public double VoteAverage { get; set; }
}
