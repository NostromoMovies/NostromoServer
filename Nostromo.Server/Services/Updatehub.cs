using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace Nostromo.Server.Services
{
    public class ProgressHub : Hub
    {
        public async Task SendProgressUpdate(string jobId, string message, int progress)
        {
            await Clients.All.SendAsync("ReceiveProgressUpdate", jobId, message, progress);
        }
    }
}