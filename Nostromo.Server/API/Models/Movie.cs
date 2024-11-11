using System.Collections.Generic; // List
using System.Text.Json.Serialization; // JsonPropertyName
namespace Nostromo.Models;

public class User
{
    [JsonPropertyName("username")]
    public string username { get; set; }
    [JsonPropertyName("password")]
    public bool password { get; set; }
    [JsonPropertyName("first_name")]
    public string first_name { get; set; }
    [JsonPropertyName("last_name")]
    public string last_name { get; set; }
}