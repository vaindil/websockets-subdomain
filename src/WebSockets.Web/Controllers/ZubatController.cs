using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WebSockets.Web.Auth;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("zubat")]
    public class ZubatController : ControllerBase
    {
        private readonly ZubatWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;

        public ZubatController(ZubatWebSocketManager wsMgr, IMemoryCache cache)
        {
            _wsMgr = wsMgr;
            _cache = cache;
        }

        [HttpPut("{seconds}")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> AddSeconds(int seconds)
        {
            _cache.TryGetValue<int>(CacheKeys.ZubatSecondsRemaining, out var curSeconds);
            curSeconds += seconds;

            await _wsMgr.SendAllAsync(curSeconds.ToString());

            _cache.Set(CacheKeys.ZubatSecondsRemaining, curSeconds);

            return NoContent();
        }
    }
}
