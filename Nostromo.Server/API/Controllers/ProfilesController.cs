using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Models;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using db = Nostromo.Server.Database;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly NostromoDbContext _context;

    public ProfilesController(NostromoDbContext context)
    {
        _context = context;
    }
    
    private async Task<int?> GetLoggedInUserIdAsync()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _context.AuthTokens
            .Where(t => t.Token == token)
            .Select(t => (int?)t.UserId)
            .FirstOrDefaultAsync();
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProfile([FromBody] Profile profile)
    {
        var userId = GetLoggedInUserIdAsync().Result;
        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        profile.Adult = profile.Age >= 18 ? true : false;
        profile.UserId = userId.Value;
        
        _context.Profiles.Add(profile);
        
        await _context.SaveChangesAsync();
        return Ok(new {profileId = profile.ProfileID});
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] Profile profile)
    {
        var userId = GetLoggedInUserIdAsync().Result;
        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        var profileToUpdate = await _context.Profiles.FirstOrDefaultAsync(
            p => p.ProfileID == profile.ProfileID && p.UserId == userId);

        if (profileToUpdate == null)
        {
            return NotFound("Profile not found");
        }
        
        _context.Profiles.Update(profile);
        
        await _context.SaveChangesAsync();
        return Ok(new {profileId = profile.ProfileID});
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteProfile([FromBody]  int profileId)
    {
        var userId = GetLoggedInUserIdAsync().Result;
        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        var profileToDelete = await _context.Profiles.FirstOrDefaultAsync(
            p => p.ProfileID == profileId && p.UserId == userId);

        if (profileToDelete == null)
        {
            return NotFound("Profile not found");
        }
        
        _context.Profiles.Remove(profileToDelete);
        await _context.SaveChangesAsync();
        return Ok("Profile deleted");
    }

    [HttpPost("selectedProfile")]
    public async Task<IActionResult> SelectProfile([FromBody] int profileId)
    {
        var userId = GetLoggedInUserIdAsync().Result;
        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        var selectedProfile = await _context.Profiles.FirstOrDefaultAsync(
            p => p.ProfileID == profileId && p.UserId == userId);

        if (selectedProfile == null)
        {
            return NotFound("Profile not found");
        }
        
        HttpContext.Session.SetInt32("SelectedProfile", selectedProfile.ProfileID);

        return Ok(new
        {
            message = "Profile selected",
            profileId = selectedProfile.ProfileID,
            userId = userId.Value
        });
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetProfiles()
    {
        var userId = GetLoggedInUserIdAsync().Result;

        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        var profiles = await _context.Profiles.Where(p => p.UserId == userId).ToListAsync();
        
        return Ok(profiles);
        
    }
}