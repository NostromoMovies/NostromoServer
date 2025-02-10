using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Nostromo.Server.Services;


namespace Nostromo.Server.API.Controllers
{
    [ApiController]
    [Route("api/progress")] 
    public class HashProgressController : ControllerBase
    {
        private readonly IProgressStore _progressStore;
        private readonly IHubContext<ProgressHub> _hubContext;


        public HashProgressController(IProgressStore progressStore, IHubContext<ProgressHub> hubContext)
        {
            _progressStore = progressStore;
            _hubContext = hubContext;
        }
        //gets job id to track progress [HttpPost("progress")]
        [HttpGet("{jobId}")]
        public IActionResult GetProgress(string jobId)
        {
            var progress = _progressStore.GetProgress(jobId);
            if (progress == null)
                return NotFound(new { Message = $"Progress not found for Job ID: {jobId}" });


            return Ok(new { JobId = jobId, Progress = progress });
        }
        //Sends real time update via Signal R
        [HttpPost("{jobId}/notify")]
        public async Task<IActionResult> NotifyProgress(string jobId)
        {
            var progress = _progressStore.GetProgress(jobId);
            if (progress == null)
                return NotFound(new { Message = $"Progress not found for Job ID: {jobId}" });
          
            await _hubContext.Clients.All.SendAsync("ReceiveProgressUpdate", jobId, progress);
            return Ok(new { Message = $"Progress update sent for Job ID: {jobId}", Progress = progress });
        }
    }
}