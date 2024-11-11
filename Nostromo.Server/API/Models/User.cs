using System.Collections.Generic; // List
namespace Nostromo.Models;


public class User
{
    public int Id { get; set; }
    public string First_Name { get; set; }
    public string Last_Name { get; set; }
    public string Username { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] Salt { get; set; }
}