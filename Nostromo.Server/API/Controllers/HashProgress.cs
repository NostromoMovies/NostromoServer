using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Nostromo.Server.Scheduling.Jobs;
using Microsoft.AspNetCore.Http;
using Nostromo.Server.API.Models;
using Nostromo.Server.Services;


namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/progress")]
    public class HashProgressController : ControllerBase
    {
        private readonly IProgressStore _progressStore;
        

        public HashProgressController(IProgressStore progressStore)
        {
            _progressStore = progressStore;
        }

        [HttpGet("{jobId}")]
        public IActionResult GetProgress(string jobId)
        {
            var progress = _progressStore.GetProgress(jobId);
            if (progress == null)
                return NotFound($"Progress not found for Job ID: {jobId}");

            return Ok(new { JobId = jobId, Progress = progress });
        }
    }

}
