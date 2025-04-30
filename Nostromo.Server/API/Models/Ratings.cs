using System.Collections.Generic;
using System.Linq;

public static class Ratings
{
    private static readonly Dictionary<string,int> _ages = new Dictionary<string,int>
    {

        ["TV-Y"]     = 2,
        ["TV-Y7"]    = 7,
        ["TV-Y7-FV"] = 7,
        ["TV-G"]     = 0,
        ["TV-PG"]    = 10,
        ["TV-14"]    = 14,
        ["TV-MA"]    = 17,
        ["G"]        = 0,
        ["PG"]       = 10,
        ["PG-13"]    = 13,
        ["R"]        = 17,
        ["NC-17"]    = 18
    };
    
    public static IEnumerable<string> GetRatingsForAge(int age)
    {
        return _ages
            .Where(kv => kv.Value <= age)
            .Select(kv => kv.Key);
    }
}