using System.Collections.Generic; // List
namespace Nostromo.Models;


public class User
{
    public int Id { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string salt { get; set; }
}