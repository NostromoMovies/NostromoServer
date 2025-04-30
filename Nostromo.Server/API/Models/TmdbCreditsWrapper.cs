namespace Nostromo.Server.API.Models;

public class TmdbCreditsWrapper
{
    public int Id { get; set; }
    public List<TmdbCastMember> Cast { get; set; }
    public List<TmdbCrewMember> Crew { get; set; }
}