using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WebSockets.Web.Utils;

namespace WebSockets.Web.WebSockets
{
    public class FitzyWebSocketManager : WebSocketManager
    {
        private readonly IMemoryCache _cache;

        public FitzyWebSocketManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task SendAllCurrentRecordAsync()
        {
            var (wins, losses, draws) = _cache.GetCurrentFitzyRecord();
            await SendAllAsync($"{wins} {losses} {draws}");
        }
    }
}
