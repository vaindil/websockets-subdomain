using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WebSockets.Data.Services;
using WebSockets.Web.Auth;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("crendor")]
    public class CrendorSubPointsController : ControllerBase
    {
        private readonly CrendorWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;
        private readonly KeyValueService _kvSvc;

        public CrendorSubPointsController(CrendorWebSocketManager wsMgr, IMemoryCache cache, KeyValueService kvSvc)
        {
            _wsMgr = wsMgr;
            _cache = cache;
            _kvSvc = kvSvc;
        }

        [HttpGet("points")]
        public IActionResult Get()
        {
            _cache.TryGetValue(CacheKeys.CrendorSubPoints, out int points);
            return Ok(points);
        }

        [HttpPut("points/{num}")]
        [MiddlewareFilter(typeof(CrendorHeaderAuthPipeline))]
        public async Task<IActionResult> Points(int num)
        {
            var numString = num.ToString();

            _cache.Set(CacheKeys.CrendorSubPoints, num, CacheHelpers.EntryOptions);
            await _wsMgr.SendAllAsync(numString);

            await _kvSvc.CreateOrUpdateAsync(CacheKeys.CrendorSubPoints, numString);

            return NoContent();
        }
    }
}
