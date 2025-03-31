using System.Text.Json.Serialization;

namespace Nostromo.Server.API.Models;

public class TvEpisodeCreditWrapper
{
    [JsonPropertyName("cast")]
    public List<TmdbCastMember> Cast { get; set; }
    
    [JsonPropertyName("guest_stars")]
    public List<TmdbCastMember> GuestStars { get; set; }
    
    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; }
}