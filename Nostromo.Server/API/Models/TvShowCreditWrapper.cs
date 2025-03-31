using System.Text.Json.Serialization;

namespace Nostromo.Server.API.Models;


public class TvShowCreditWrapper
{
    [JsonPropertyName("cast")]
    public List<TmdbCastMember> Cast { get; set; }
    
    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; }
}