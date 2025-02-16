using Microsoft.AspNetCore.Mvc;
using Nostromo.Server.Services;

[Route("api/progress")]
[ApiController]
public class ProgressController : ControllerBase
{
    private readonly IProgressStore _progressStore;

    public ProgressController(IProgressStore progressStore)
    {
        _progressStore = progressStore;
    }

    [HttpGet("{jobId}")]
    public IActionResult GetProgress(string jobId)
    {
        var (filename, progress) = _progressStore.GetProgress(jobId);

        if (filename == null && progress == null)
        {
            return NotFound(new { message = "Job ID not found or completed." });
        }

        return Ok(new { jobId, filename, progress });
    }


    [HttpGet("active")]
    public IActionResult HasActiveJobs()
    {
        return Ok(new { active = _progressStore.HasActiveJobs() });
    }

    [HttpGet("jobs")]
    public IActionResult GetActiveJobIds()
    {
        var jobIds = _progressStore.GetActiveJobIds();
        return Ok(new { jobIds });
    }

    [HttpDelete("remove/{jobId}")]
    public IActionResult RemoveProgress(string jobId)
    {
        _progressStore.RemoveProgress(jobId);
        return Ok(new { message = "Job removed successfully." });
    }

}