using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nostromo.Server.Database;
using Nostromo.Models;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.Services;
using db = Nostromo.Server.Database;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly NostromoDbContext _context;
    private readonly SelectedProfileService _selectedProfileService;

    public ProfilesController(NostromoDbContext context, SelectedProfileService selectedProfileService)
    {
        _context = context;
        _selectedProfileService = selectedProfileService;
    }

    public class ProfileRequest
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string PosterPath { get; set; }
    }

    public class ProfileUpdateRequest
    {
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string PosterPath { get; set; }
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
    public async Task<IActionResult> CreateProfile([FromBody] ProfileRequest request)
    {
        var userId = GetLoggedInUserIdAsync().Result;
        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        
        var profile = new Profile
        {
            Name = request.Name,
            Age = request.Age,
            Adult = request.Age >= 18 ? true : false,
            posterPath = request.PosterPath, 
            UserId = userId.Value,
        };
        
        
        
        _context.Profiles.Add(profile);
        
        await _context.SaveChangesAsync();
        return Ok(new {profileId = profile.ProfileID});
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateRequest request)
    {
        var userId = GetLoggedInUserIdAsync().Result;
        if (userId == null)
        {
            return Unauthorized("You are not logged in");
        }
        
        var profileToUpdate = await _context.Profiles.FirstOrDefaultAsync(
            p => p.ProfileID == request.ProfileId && p.UserId == userId);

        if (profileToUpdate == null)
        {
            return NotFound("Profile not found");
        }
        
        profileToUpdate.Name = request.Name;
        profileToUpdate.Age = request.Age;
        profileToUpdate.Adult = request.Age >= 18 ? true : false;
        profileToUpdate.posterPath = request.PosterPath;
        
        await _context.SaveChangesAsync();
        return Ok(new {profileId = profileToUpdate.ProfileID});
    }

    [HttpDelete("delete/{profileId}")]
    public async Task<IActionResult> DeleteProfile(int profileId)
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

    [HttpPost("selectedProfile/{profileId}")]
    public async Task<IActionResult> SelectProfile(int profileId)
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
        
        _selectedProfileService.SetSelectedProfileId(selectedProfile.ProfileID);

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
        
        var profiles = await _context.Profiles.Where(p => p.UserId == userId).Select(p => 
            new {
                    id = p.ProfileID,
                    name = p.Name,
                    age = p.Age,
                    posterPath = p.posterPath
                }).ToListAsync();
        
        return Ok(profiles);
        
    }
}