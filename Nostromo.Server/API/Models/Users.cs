using System.Collections.Generic; // List
namespace Nostromo.Models;


public class Users
{
    public int Id { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string username { get; set; }
    public string passwordHash { get; set; }
    public string salt { get; set; }
}